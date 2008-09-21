// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gallio.Runtime.Logging;
using Gallio.Runtime;
using Gallio.Reflection;
using Gallio.TDNetRunner.Properties;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Runner;
using TestDriven.Framework;
using ITestRunner=TestDriven.Framework.ITestRunner;
using TDF = TestDriven.Framework;

namespace Gallio.TDNetRunner
{
    /// <summary>
    /// Gallio test runner for TestDriven.NET.
    /// </summary>
    [Serializable]
    public class GallioTestRunner : ITestRunner
    {
        private readonly string reportType = @"html";

        #region TDF.ITestRunner Members

        /// <summary>
        /// TD.NET calls this method when you run an entire assemby (by right-clicking
        /// in a project an selecting "Run Test(s)")
        /// </summary>
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(@"assembly");

            return Run(testListener, assembly, new AssemblyFilter<ITest>(new EqualityFilter<string>(assembly.FullName)));
        }

        /// <summary>
        /// TD.NET calls this method when you run either all the tests in a fixture or
        /// an individual test.
        /// </summary>
        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            if (assembly == null)
                throw new ArgumentNullException(@"assembly");
            if (member == null)
                throw new ArgumentNullException(@"member");

            List<Filter<ITest>> filters = new List<Filter<ITest>>();
            filters.Add(new AssemblyFilter<ITest>(new EqualityFilter<string>(assembly.FullName)));
            switch (member.MemberType)
            {
                case MemberTypes.TypeInfo:
                    Type type = (Type)member;
                    // FIXME: Should we always include derived types?
                    filters.Add(new TypeFilter<ITest>(new EqualityFilter<string>(type.FullName), true));
                    break;

                case MemberTypes.Method:
                    MethodInfo methodInfo = (MethodInfo)member;
                    // We look for the declaring type so we can also use a TypeFilter
                    // to avoid ambiguity
                    Type declaringType = methodInfo.DeclaringType;
                    // FIXME: Should we always include derived types?
                    filters.Add(new TypeFilter<ITest>(new EqualityFilter<string>(declaringType.FullName), true));
                    filters.Add(new MemberFilter<ITest>(new EqualityFilter<string>(member.Name)));
                    break;

                default:
                    // This is not something we can run so just ignore it
                    InformNoTestsWereRun(testListener, String.Format(Resources.MbUnitTestRunner_MemberIsNotATest, member.Name));
                    return TestRunState.NoTests;
            }

            return Run(testListener, assembly, new AndFilter<ITest>(filters.ToArray()));
        }

        /// <summary>
        /// It appears this method never gets called.
        /// </summary>
        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            if (assembly == null)
                throw new ArgumentNullException(@"assembly");
            if (String.IsNullOrEmpty(ns))
                throw new ArgumentNullException(@"ns");

            List<Filter<ITest>> filters = new List<Filter<ITest>>();
            filters.Add(new AssemblyFilter<ITest>(new EqualityFilter<string>(assembly.FullName)));
            filters.Add(new NamespaceFilter<ITest>(new EqualityFilter<string>(ns)));

            return Run(testListener, assembly, new AndFilter<ITest>(filters.ToArray()));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Provided so that the unit tests can override test execution behavior.
        /// </summary>
        protected virtual TestLauncherResult RunLauncher(TestLauncher launcher)
        {
            return launcher.Run();
        }

        internal TestRunState Run(ITestListener testListener, Assembly assembly, Filter<ITest> filter)
        {
            if (testListener == null)
                throw new ArgumentNullException(@"testListener");
            if (filter == null)
                throw new ArgumentNullException(@"filter");

            ILogger logger = new FilteredLogger(new TDNetLogger(testListener), LogSeverity.Info);
            LogAddInVersion(logger);

            TestLauncher launcher = new TestLauncher();
            launcher.Logger = logger;
            launcher.ProgressMonitorProvider = new LogProgressMonitorProvider(logger);
            launcher.TestExecutionOptions.Filter = filter;
            launcher.TestRunnerFactoryName = StandardTestRunnerFactoryNames.Local;

            launcher.RuntimeSetup = new RuntimeSetup();

            // Set the runtime path explicitly to ensure that we do not encounter problems
            // when the test assembly contains a local copy of the primary runtime assemblies
            // which will confuse the runtime into searching in the wrong place for plugins.
            launcher.RuntimeSetup.RuntimePath = Path.GetDirectoryName(AssemblyUtils.GetFriendlyAssemblyLocation(typeof(GallioTestRunner).Assembly));

            // This monitor will inform the user in real-time what's going on
            launcher.TestRunnerExtensions.Add(new TDNetLogExtension(testListener));

            string location = AssemblyUtils.GetFriendlyAssemblyLocation(assembly);
            launcher.TestPackageConfig.AssemblyFiles.Add(location);

            string assemblyDirectory = Path.GetDirectoryName(location);
            //launcher.TestPackageConfig.HostSetup.ShadowCopy = true;
            launcher.TestPackageConfig.HostSetup.ApplicationBaseDirectory = assemblyDirectory;
            launcher.TestPackageConfig.HostSetup.WorkingDirectory = assemblyDirectory;

            launcher.ReportFormats.Add(reportType);
            launcher.ReportNameFormat = Path.GetFileName(location);
            launcher.ReportDirectory = GetReportDirectory(logger) ?? "";

            if (String.IsNullOrEmpty(launcher.ReportDirectory))
            {
                return TestRunState.Failure;
            }

            TestLauncherResult result = RunLauncher(launcher);

            // This will generate a link to the generated report
            if (result.ReportDocumentPaths.Count != 0)
            {
                Uri uri = new Uri(result.ReportDocumentPaths[0]);
                testListener.TestResultsUrl("file:///" + uri.LocalPath.Replace(" ", "%20").Replace(@"\", @"/"));
            }

            // Inform no tests run, if necessary.
            if (result.ResultCode == ResultCode.NoTests)
                InformNoTestsWereRun(testListener, Resources.MbUnitTestRunner_NoTestsFound);
            else if (result.Statistics.TestCount == 0)
                InformNoTestsWereRun(testListener, null);

            return GetTDNetResult(result);
        }

        /// <summary>
        /// Gets a temporary folder to store the HTML report.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <returns>The full name of the folder or null if it could not be created.</returns>
        private static string GetReportDirectory(ILogger logger)
        {
            try
            {
                DirectoryInfo reportDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), @"Gallio.TDNetRunner"));
                if (reportDirectory.Exists)
                {
                    // Make sure the folder is empty
                    reportDirectory.Delete(true);
                }

                reportDirectory.Create();

                return reportDirectory.FullName;
            }
            catch (Exception e)
            {
                logger.Log(LogSeverity.Error, "Could not create the report directory.", e);
                return null;
            }
        }

        /// <summary>
        /// Translates the test execution result into something understandable
        /// for TDNet.
        /// </summary>
        /// <param name="result">The result information</param>
        /// <returns>The TestRunState value that should be returned to TDNet.</returns>
        private static TestRunState GetTDNetResult(TestLauncherResult result)
        {
            switch (result.ResultCode)
            {
                case ResultCode.FatalException:
                case ResultCode.InvalidArguments:
                case ResultCode.Canceled:
                default:
                    return TestRunState.Error;

                case ResultCode.Failure:
                    return TestRunState.Failure;

                case ResultCode.NoTests:
                    return TestRunState.NoTests;

                case ResultCode.Success:
                    return TestRunState.Success;
            }
        }

        /// <summary>
        /// Inform the user that no tests were run and the reason for it. TD.NET displays
        /// a message like "0 Passed, 0 Failed, 0 Skipped" but it does it in the status bar,
        /// which may be harder to notice for the user. Be aware that this message will
        /// only be displayed when the user runs an individual test or fixture (TD.NET
        /// ignores the messages we send when it's running an entire assembly).
        /// </summary>
        /// <param name="testListener">An ITestListener object to write the message to.</param>
        /// <param name="reason">The reason no tests were run for.</param>
        private static void InformNoTestsWereRun(ITestListener testListener, string reason)
        {
            if (String.IsNullOrEmpty(reason))
                reason = @"";
            else
                reason = @" (" + reason + @")";

            string message = String.Format("** {0}{1} **", Resources.MbUnitTestRunner_NoTestsWereRun, reason);

            testListener.WriteLine(message, Category.Warning);
        }

        private static void LogAddInVersion(ILogger logger)
        {
            Version appVersion = Assembly.GetCallingAssembly().GetName().Version;
            logger.Log(LogSeverity.Important, String.Format(Resources.RunnerNameAndVersion + "\n",
                appVersion.Major, appVersion.Minor, appVersion.Build, appVersion.Revision));
        }

        #endregion
    }
}