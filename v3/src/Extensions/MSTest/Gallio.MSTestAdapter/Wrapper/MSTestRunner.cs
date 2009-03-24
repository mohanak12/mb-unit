// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.MSTestAdapter.Model;
using Gallio.MSTestAdapter.Properties;
using Gallio.Runner.Caching;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.MSTestAdapter.Wrapper
{
    internal abstract class MSTestRunner
    {
        protected static readonly Guid RootTestListGuid = new Guid("8c43106b-9dc1-4907-a29f-aa66a61bf5b6");
        protected static readonly Guid SelectedTestListGuid = new Guid("05EF261C-0065-4c5f-9DE3-3D068277A643");
        protected const string SelectedTestListName = "SelectedTests";

        private readonly IDiskCache diskCache;

        public MSTestRunner(IDiskCache diskCache)
        {
            if (diskCache == null)
                throw new ArgumentNullException("diskCache");

            this.diskCache = diskCache;
        }

        public static MSTestRunner GetRunnerForFrameworkVersion(Version frameworkVersion, IDiskCache diskCache)
        {
            if (frameworkVersion.Major == 8 && frameworkVersion.Minor == 0)
                return new MSTestRunner2005(diskCache);
            if (frameworkVersion.Major == 9 && frameworkVersion.Minor == 0)
                return new MSTestRunner2008(diskCache);
            if (frameworkVersion.Major == 10 && frameworkVersion.Minor == 0)
                return new MSTestRunner2010(diskCache);

            throw new NotSupportedException(string.Format("MSTest v{0}.{1} is not supported at this time.", frameworkVersion.Major, frameworkVersion.Minor));
        }

        public TestOutcome RunSession(ITestContext assemblyContext, MSTestAssembly assemblyTest,
            ITestCommand assemblyTestCommand, ITestStep parentTestStep, IProgressMonitor progressMonitor)
        {
            IDiskCacheGroup cacheGroup = diskCache.Groups["MSTestAdapter:" + Guid.NewGuid()];
            try
            {
                cacheGroup.Create();

                string testMetadataPath = cacheGroup.GetFileInfo("tests.vsmdi").FullName;
                string testResultsPath = cacheGroup.GetFileInfo("tests.trx").FullName;
                string runConfigPath = cacheGroup.GetFileInfo("tests.runconfig").FullName;
                string workingDirectory = Environment.CurrentDirectory;

                progressMonitor.SetStatus("Generating tests list");
                CreateTestMetadataFile(testMetadataPath,
                    GetTestsFromCommands(assemblyTestCommand.PreOrderTraversal), assemblyTest.AssemblyFilePath);
                CreateRunConfigFile(runConfigPath);

                progressMonitor.SetStatus("Executing tests");
                TestOutcome outcome = ExecuteTests(assemblyContext, workingDirectory,
                    testMetadataPath, testResultsPath, runConfigPath);

                progressMonitor.SetStatus("Processing results");
                if (!ProcessTestResults(assemblyContext, assemblyTestCommand, testResultsPath))
                    outcome = outcome.CombineWith(TestOutcome.Failed);

                return outcome;
            }
            finally
            {
                cacheGroup.Delete();
            }
        }

        protected abstract string GetVisualStudioVersion();

        protected abstract void WriteTestMetadata(XmlWriter writer, IEnumerable<MSTest> tests, string assemblyFilePath);

        protected abstract void WriteRunConfig(XmlWriter writer);

        private static MSTestCommand GetMSTestCommand()
        {
            return Debugger.IsAttached
                ? (MSTestCommand) DebugMSTestCommand.Instance
                : StandaloneMSTestCommand.Instance;
        }

        private static IEnumerable<MSTest> GetTestsFromCommands(IEnumerable<ITestCommand> testCommands)
        {
            foreach (ITestCommand testCommand in testCommands)
                yield return (MSTest)testCommand.Test;
        }

        private void CreateTestMetadataFile(string testMetadataFilePath, IEnumerable<MSTest> tests, string assemblyFilePath)
        {
            using (XmlWriter writer = OpenXmlWriter(testMetadataFilePath))
            {
                WriteTestMetadata(writer, tests, assemblyFilePath);
            }
        }

        private void CreateRunConfigFile(string runConfigFilePath)
        {
            using (XmlWriter writer = OpenXmlWriter(runConfigFilePath))
            {
                WriteRunConfig(writer);
            }
        }

        private XmlWriter OpenXmlWriter(string filePath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            return XmlWriter.Create(filePath, settings);
        }

        private TestOutcome ExecuteTests(ITestContext context, string workingDirectory,
            string testMetadataPath, string testResultsPath, string runConfigPath)
        {
            MSTestCommandArguments args = new MSTestCommandArguments();
            args.NoLogo = true;
            args.TestMetadata = testMetadataPath;
            args.ResultsFile = testResultsPath;
            args.RunConfig = runConfigPath;
            args.TestList = SelectedTestListName;

            string executablePath = MSTestResolver.FindMSTestPathForVisualStudioVersion(GetVisualStudioVersion());
            if (executablePath == null)
            {
                context.LogWriter.Failures.Write(Resources.MSTestController_MSTestExecutableNotFound);
                return TestOutcome.Error;
            }

            TextWriter writer = context.LogWriter["MSTest Output"];
            int exitCode = GetMSTestCommand().Run(executablePath, workingDirectory, args, writer);

            if (exitCode != 0)
            {
                context.LogWriter.Failures.Write("MSTest returned an exit code of {0}.", exitCode);
            }

            return TestOutcome.Passed;
        }

        private bool ProcessTestResults(ITestContext assemblyContext,
            ITestCommand assemblyCommand, string resultsFilePath)
        {
            Dictionary<string, MSTestResult> testResults = new Dictionary<string, MSTestResult>();

            if (File.Exists(resultsFilePath))
            {
                using (XmlReader reader = OpenTestResultsFile(resultsFilePath))
                {
                    // Errors in the class or assembly setup/teardown methods are put in a general error
                    // section by MSTest, so we log them at the assembly level.
                    ProcessGeneralErrorMessages(assemblyContext, reader);
                }

                using (XmlReader reader = OpenTestResultsFile(resultsFilePath))
                {
                    ExtractExecutedTestsInformation(testResults, reader);
                }
            }

            // The ignored tests won't be run by MSTest. In the case where all the selected tests
            // have been ignored, we won't even have a results file, so we need to process them
            // here.
            ProcessIgnoredTests(testResults, assemblyCommand.PreOrderTraversal);

            bool passed = true;
            foreach (ITestCommand command in assemblyCommand.Children)
            {
                passed &= ProcessTestCommand(command, assemblyContext.TestStep, testResults);
            }

            return passed;
        }

        private static XmlReader OpenTestResultsFile(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;
            return XmlReader.Create(path, settings);
        }

        private static void ProcessGeneralErrorMessages(ITestContext assemblyContext,
            XmlReader reader)
        {
            while (reader.ReadToFollowing("RunInfo"))
            {
                reader.ReadToFollowing("Text");
                LogError(assemblyContext, reader.ReadString());
            }
        }

        private static bool ProcessTestCommand(ITestCommand command, ITestStep parentStep, Dictionary<string, MSTestResult> testResults)
        {
            ITestContext testContext = command.StartPrimaryChildStep(parentStep);
            MSTest test = (MSTest)command.Test;
            try
            {
                if (test.IsTestCase)
                {
                    if (testResults.ContainsKey(test.Guid))
                    {
                        MSTestResult testResult = testResults[test.Guid];

                        if (testResult.StdOut != null)
                            LogStdOut(testContext, testResult.StdOut);
                        if (testResult.Errors != null)
                            LogError(testContext, testResult.Errors);

                        testContext.FinishStep(testResult.Outcome, testResult.Duration);
                        return (testResult.Outcome != TestOutcome.Error && testResult.Outcome != TestOutcome.Failed);
                    }

                    testContext.LogWriter.Warnings.Write("No test results available!");
                    testContext.FinishStep(TestOutcome.Skipped, null);
                    return true;
                }
                else if (command.Children.Count > 0)
                {
                    bool passed = true;
                    foreach (ITestCommand child in command.Children)
                        passed &= ProcessTestCommand(child, testContext.TestStep, testResults);

                    testContext.FinishStep(passed ? TestOutcome.Passed : TestOutcome.Failed, null);
                    return passed;
                }
                else
                {
                    testContext.FinishStep(TestOutcome.Passed, null);
                    return true;
                }
            }
            catch
            {
                testContext.FinishStep(TestOutcome.Error, null);
                throw;
            }
        }

        protected abstract void ExtractExecutedTestsInformation(
            Dictionary<string, MSTestResult> testResults,
            XmlReader reader);

        private static void ProcessIgnoredTests(Dictionary<string, MSTestResult> testCommandsByTestGuid, IEnumerable<ITestCommand> allCommands)
        {
            foreach (ITestCommand command in allCommands)
            {
                MSTest test = command.Test as MSTest;
                if (test != null && test.IsTestCase)
                {
                    string ignoreReason = test.Metadata.GetValue(MetadataKeys.IgnoreReason);
                    if (!String.IsNullOrEmpty(ignoreReason))
                    {
                        MSTestResult testResult = new MSTestResult();
                        testResult.Guid = test.Guid;
                        testResult.Outcome = TestOutcome.Ignored;
                        if (!testCommandsByTestGuid.ContainsKey(testResult.Guid))
                        {
                            testCommandsByTestGuid.Add(testResult.Guid, testResult);
                        }
                        //testCommandsByTestGuid.Add(testExecutionInfo.Guid, testExecutionInfo);
                    }
                }
            }
        }

        protected static string ReadErrors(XmlReader reader)
        {
            reader.ReadToFollowing("Message");
            string message = reader.ReadString();
            reader.ReadToFollowing("StackTrace");
            message += "\n" + reader.ReadString();
            return message;
        }

        private static void LogStdOut(ITestContext context, string message)
        {
            context.LogWriter.ConsoleOutput.Write(message);
        }

        private static void LogError(ITestContext context, string message)
        {
            context.LogWriter.Failures.Write(message);
        }

        protected static TestOutcome GetTestOutcome(string outcome)
        {
            TestOutcome testOutcome;
            // The commented cases are the ones we are not sure how to map yet.
            // By default they'll become TestOutcome.Passed
            switch (outcome)
            {
                case "Aborted":
                case "3":
                    testOutcome = TestOutcome.Canceled;
                    break;
                //case "Completed":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                //case "Disconnected":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                case "Error":
                case "0":
                    testOutcome = TestOutcome.Error;
                    break;
                case "Failed":
                case "1":
                    testOutcome = TestOutcome.Failed;
                    break;
                case "Inconclusive":
                case "4":
                    testOutcome = TestOutcome.Inconclusive;
                    break;
                //case "InProgress":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                //case "Max":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                //case "Min":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                case "NotExecuted":
                case "7":
                    testOutcome = TestOutcome.Skipped;
                    break;
                case "NotRunnable":
                case "6":
                    testOutcome = TestOutcome.Skipped;
                    break;
                case "Passed":
                case "10":
                    testOutcome = TestOutcome.Passed;
                    break;
                //case "PassedButRunAborted":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                case "Pending":
                case "13":
                    testOutcome = TestOutcome.Pending;
                    break;
                case "Timeout":
                case "2":
                    testOutcome = TestOutcome.Timeout;
                    break;
                //case "Warning":
                //    testOutcome = TestOutcome.Passed;
                //    break;
                default:
                    testOutcome = TestOutcome.Passed;
                    break;
            }

            return testOutcome;
        }

        protected static TimeSpan GetDuration(string duration)
        {
            return TimeSpan.Parse(duration);
        }
    }
}
