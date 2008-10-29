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
using System.Collections.Specialized;
using System.IO;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Runner;
using Gallio.Runner.Reports;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.TDNetRunner.Facade;
using Gallio.TDNetRunner.Properties;

namespace Gallio.TDNetRunner.Core
{
    public class RemoteProxyTestRunner : BaseProxyTestRunner, IProxyTestRunner
    {
        private const string ShortReportType = @"html-condensed";
        private const string LongReportType = @"html";
        private const int ShortReportThreshold = 100;

        private TestLauncher launcher;

        public RemoteProxyTestRunner()
        {
            launcher = new TestLauncher();
        }

        protected override void Dispose(bool disposing)
        {
            launcher = null;
        }

        protected override void AbortImpl()
        {
            if (launcher != null)
                launcher.Cancel();
        }

        protected override FacadeTestRunState RunImpl(IFacadeTestListener testListener, string assemblyPath, string cref)
        {
            if (cref == null)
                return RunAssembly(testListener, assemblyPath);

            if (cref.Length >= 2)
            {
                char descriptor = cref[0];

                switch (descriptor)
                {
                    case 'T':
                        return RunType(testListener, assemblyPath, cref.Substring(2));

                    case 'N':
                        return RunNamespace(testListener, assemblyPath, cref.Substring(2));

                    case 'M':
                    case 'F':
                    case 'P':
                    case 'E':
                        int paramsPos = cref.IndexOf('(');
                        if (paramsPos < 0)
                            paramsPos = cref.Length;

                        string memberNameWithType = cref.Substring(2, paramsPos - 2);
                        int memberPos = memberNameWithType.LastIndexOf('.');
                        if (memberPos < 0)
                            break;

                        string typeName = memberNameWithType.Substring(0, memberPos);
                        string memberName = memberNameWithType.Substring(memberPos + 1);
                        return RunMember(testListener, assemblyPath, typeName, memberName);
                }
            }

            return FacadeTestRunState.NoTests;
        }

        private FacadeTestRunState RunAssembly(IFacadeTestListener testListener, string assemblyPath)
        {
            return Run(testListener, assemblyPath, new AnyFilter<ITest>());
        }

        private FacadeTestRunState RunNamespace(IFacadeTestListener testListener, string assemblyPath, string @namespace)
        {
            return Run(testListener, assemblyPath, new AndFilter<ITest>(new Filter<ITest>[]
            { 
                new NamespaceFilter<ITest>(new EqualityFilter<string>(@namespace))
            }));
        }

        private FacadeTestRunState RunType(IFacadeTestListener testListener, string assemblyPath, string typeName)
        {
            return Run(testListener, assemblyPath, new AndFilter<ITest>(new Filter<ITest>[]
            { 
                new TypeFilter<ITest>(new EqualityFilter<string>(typeName), true)
            }));
        }

        private FacadeTestRunState RunMember(IFacadeTestListener testListener, string assemblyPath, string typeName, string memberName)
        {
            return Run(testListener, assemblyPath, new AndFilter<ITest>(new Filter<ITest>[]
            { 
                new TypeFilter<ITest>(new EqualityFilter<string>(typeName), true),
                new MemberFilter<ITest>(new EqualityFilter<string>(memberName))
            }));
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Provided so that the unit tests can override test execution behavior.
        /// </summary>
        internal virtual TestLauncherResult RunLauncher(TestLauncher launcher)
        {
            return launcher.Run();
        }

        private FacadeTestRunState Run(IFacadeTestListener testListener, string assemblyPath, Filter<ITest> filter)
        {
            if (testListener == null)
                throw new ArgumentNullException(@"testListener");
            if (filter == null)
                throw new ArgumentNullException(@"filter");

            ILogger logger = new FilteredLogger(new TDNetLogger(testListener), LogSeverity.Info);

            launcher.Logger = logger;
            launcher.ProgressMonitorProvider = new LogProgressMonitorProvider(logger);
            launcher.TestExecutionOptions.Filter = filter;
            launcher.TestRunnerFactoryName = StandardTestRunnerFactoryNames.IsolatedAppDomain;

            // This monitor will inform the user in real-time what's going on
            launcher.TestRunnerExtensions.Add(new TDNetExtension(testListener));

            launcher.TestPackageConfig.AssemblyFiles.Add(assemblyPath);

            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            //launcher.TestPackageConfig.HostSetup.ShadowCopy = true;
            launcher.TestPackageConfig.HostSetup.ApplicationBaseDirectory = assemblyDirectory;
            launcher.TestPackageConfig.HostSetup.WorkingDirectory = assemblyDirectory;

            TestLauncherResult result = RunLauncher(launcher);

            string reportDirectory = GetReportDirectory(logger);
            if (reportDirectory != null)
            {
                result.GenerateReports(reportDirectory, Path.GetFileName(assemblyPath),
                    new[] {DetermineReportFormat(result.Report)}, new NameValueCollection(),
                    RuntimeAccessor.Instance.Resolve<IReportManager>(), NullProgressMonitor.CreateInstance());

                // This will generate a link to the generated report
                if (result.ReportDocumentPaths.Count != 0)
                {
                    Uri rawUrl = new Uri(result.ReportDocumentPaths[0]);
                    string displayUrl = "file:///" + rawUrl.LocalPath.Replace(" ", "%20").Replace(@"\", @"/");

                    // TDNet just prints the link on its own but it's not always clear to users what it represents.
                    // testListener.TestResultsUrl(displayUrl);

                    testListener.WriteLine("\nTest Report: " + displayUrl, FacadeCategory.Info);
                }
            }

            // Inform no tests run, if necessary.
            if (result.ResultCode == ResultCode.NoTests)
                InformNoTestsWereRun(testListener, Resources.MbUnitTestRunner_NoTestsFound);
            else if (result.Statistics.TestCount == 0)
                InformNoTestsWereRun(testListener, null);

            return GetTestRunState(result);
        }

        private static string DetermineReportFormat(Report report)
        {
            return report.TestPackageRun == null || report.TestPackageRun.Statistics.RunCount < ShortReportThreshold
                ? LongReportType : ShortReportType;
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
        /// Inform the user that no tests were run and the reason for it. TD.NET displays
        /// a message like "0 Passed, 0 Failed, 0 Skipped" but it does it in the status bar,
        /// which may be harder to notice for the user. Be aware that this message will
        /// only be displayed when the user runs an individual test or fixture (TD.NET
        /// ignores the messages we send when it's running an entire assembly).
        /// </summary>
        /// <param name="testListener">An ITestListener object to write the message to.</param>
        /// <param name="reason">The reason no tests were run for.</param>
        private static void InformNoTestsWereRun(IFacadeTestListener testListener, string reason)
        {
            if (String.IsNullOrEmpty(reason))
                reason = @"";
            else
                reason = @" (" + reason + @")";

            string message = String.Format("** {0}{1} **", Resources.MbUnitTestRunner_NoTestsWereRun, reason);

            testListener.WriteLine(message, FacadeCategory.Warning);
        }

        private static FacadeTestRunState GetTestRunState(TestLauncherResult result)
        {
            switch (result.ResultCode)
            {
                default:
                    return FacadeTestRunState.Error;

                case ResultCode.Failure:
                case ResultCode.Canceled:
                    return FacadeTestRunState.Failure;

                case ResultCode.NoTests:
                case ResultCode.Success:
                    return FacadeTestRunState.Success;
            }
        }
    }
}
