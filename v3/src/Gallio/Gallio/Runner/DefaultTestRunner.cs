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
using Gallio.Common.Concurrency;
using Gallio.Model;
using Gallio.Common.Diagnostics;
using Gallio.Model.Execution;
using Gallio.Common.Markup;
using Gallio.Model.Messages;
using Gallio.Model.Serialization;
using Gallio.Common.Reflection;
using Gallio.Runner.Drivers;
using Gallio.Runner.Events;
using Gallio.Runner.Extensions;
using Gallio.Runner.Reports;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.Runner
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> that runs tests using
    /// a <see cref="ITestDriver" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The driver is created when the first package is loaded and is disposed when 
    /// the runner is disposed. Consequently the test driver may be reused for multiple test runs.
    /// </para>
    /// </remarks>
    public class DefaultTestRunner : ITestRunner
    {
        private readonly ITestDriverFactory testDriverFactory;
        private readonly TestRunnerEventDispatcher eventDispatcher;
        private readonly List<ITestRunnerExtension> extensions;

        private TappedLogger tappedLogger;
        private TestRunnerOptions testRunnerOptions;

        private State state;
        private ITestDriver testDriver;

        private enum State
        {
            Created,
            Initialized,
            Disposed
        }

        /// <summary>
        /// Creates a test runner.
        /// </summary>
        /// <param name="testDriverFactory">The test driver factory.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="testDriverFactory"/> is null.</exception>
        public DefaultTestRunner(ITestDriverFactory testDriverFactory)
        {
            if (testDriverFactory == null)
                throw new ArgumentNullException("testDriverFactory");

            this.testDriverFactory = testDriverFactory;

            eventDispatcher = new TestRunnerEventDispatcher();
            state = State.Created;
            extensions = new List<ITestRunnerExtension>();
        }

        /// <summary>
        /// Gets the logger, or null if the test runner has not been initialized.
        /// </summary>
        protected ILogger Logger
        {
            get { return tappedLogger; }
        }

        /// <summary>
        /// Gets the test runner options, or null if the test runner has not been initialized.
        /// </summary>
        protected TestRunnerOptions TestRunnerOptions
        {
            get { return testRunnerOptions; }
        }

        /// <inheritdoc />
        public ITestRunnerEvents Events
        {
            get { return eventDispatcher; }
        }

        /// <inheritdoc />
        public void RegisterExtension(ITestRunnerExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            ThrowIfDisposed();
            if (state != State.Created)
                throw new InvalidOperationException("Extensions cannot be registered after the test runner has been initialized.");

            foreach (ITestRunnerExtension currentExtension in extensions)
            {
                if (currentExtension.GetType() == extension.GetType()
                    && currentExtension.Parameters == extension.Parameters)
                    throw new InvalidOperationException(string.Format("There is already an extension of type '{0}' registered with parameters '{1}'.",
                        extension.GetType(), extension.Parameters));
            }

            extensions.Add(extension);
        }

        /// <inheritdoc />
        public void Initialize(TestRunnerOptions testRunnerOptions, ILogger logger, IProgressMonitor progressMonitor)
        {
            if (testRunnerOptions == null)
                throw new ArgumentNullException("testRunnerOptions");
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");

            ThrowIfDisposed();
            if (state != State.Created)
                throw new InvalidOperationException("The test runner has already been initialized.");

            testRunnerOptions = testRunnerOptions.Copy();

            this.testRunnerOptions = testRunnerOptions;
            tappedLogger = new TappedLogger(this, logger);

            int extensionCount = extensions.Count;
            using (progressMonitor.BeginTask("Initializing the test runner.", 1 + extensionCount))
            {
                foreach (ITestRunnerExtension extension in extensions)
                {
                    string extensionName = extension.GetType().Name; // TODO: improve me

                    progressMonitor.SetStatus(String.Format("Installing extension '{0}'.", extensionName));
                    try
                    {
                        // Note: We don't pass the tapped logger to the extensions because the
                        //       extensions frequently write to the console a bunch of information we
                        //       already have represented in the report.  We are more interested in what
                        //       the test driver has to tell us.
                        extension.Install(eventDispatcher, logger);
                        progressMonitor.Worked(1);
                    }
                    catch (Exception ex)
                    {
                        throw new RunnerException(String.Format("Failed to install extension '{0}'.", extensionName), ex);
                    }
                    progressMonitor.SetStatus("");
                }

                try
                {
                    eventDispatcher.NotifyInitializeStarted(new InitializeStartedEventArgs(testRunnerOptions));

                    progressMonitor.SetStatus("Initializing the test driver.");

                    // Create test driver.
                    RuntimeSetup runtimeSetup = RuntimeAccessor.Instance.GetRuntimeSetup();
                    testDriver = testDriverFactory.CreateTestDriver();
                    testDriver.Initialize(runtimeSetup, testRunnerOptions, tappedLogger);

                    progressMonitor.Worked(1);
                }
                catch (Exception ex)
                {
                    eventDispatcher.NotifyInitializeFinished(new InitializeFinishedEventArgs(false));
                    throw new RunnerException("A fatal exception occurred while initializing the test driver.", ex);
                }

                state = State.Initialized;
                eventDispatcher.NotifyInitializeFinished(new InitializeFinishedEventArgs(true));
            }
        }

        /// <inheritdoc />
        public Report Explore(TestPackageConfig testPackageConfig, TestExplorationOptions testExplorationOptions, IProgressMonitor progressMonitor)
        {
            if (testPackageConfig == null)
                throw new ArgumentNullException("testPackageConfig");
            if (testExplorationOptions == null)
                throw new ArgumentNullException("testExplorationOptions");
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");

            ThrowIfDisposed();
            if (state != State.Initialized)
                throw new InvalidOperationException("The test runner must be initialized before this operation is performed.");

            testPackageConfig = testPackageConfig.Copy();
            testPackageConfig.Canonicalize(null);
            testExplorationOptions = testExplorationOptions.Copy();

            using (progressMonitor.BeginTask("Exploring the tests.", 10))
            {
                Report report = new Report()
                {
                    TestPackageConfig = testPackageConfig,
                    TestModel = new TestModelData()
                };
                var reportLockBox = new LockBox<Report>(report);

                using (new LogReporter(reportLockBox, eventDispatcher))
                {
                    bool success;
                    eventDispatcher.NotifyExploreStarted(new ExploreStartedEventArgs(testPackageConfig,
                        testExplorationOptions, reportLockBox));
                    try
                    {
                        using (Listener listener = new Listener(eventDispatcher, reportLockBox))
                        {
                            testDriver.Explore(testPackageConfig,
                                testExplorationOptions, listener,
                                progressMonitor.CreateSubProgressMonitor(10));
                        }

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;

                        tappedLogger.Log(LogSeverity.Error,
                            "A fatal exception occurred while exploring tests.  Possible causes include invalid test runner parameters.",
                            ex);
                        report.TestModel.Annotations.Add(new AnnotationData(AnnotationType.Error,
                            CodeLocation.Unknown, CodeReference.Unknown, "A fatal exception occurred while exploring tests.  See log for details.", null));
                    }

                    eventDispatcher.NotifyExploreFinished(new ExploreFinishedEventArgs(success, report));
                }

                return report;
            }
        }

        /// <inheritdoc />
        public Report Run(TestPackageConfig testPackageConfig, TestExplorationOptions testExplorationOptions, TestExecutionOptions testExecutionOptions, IProgressMonitor progressMonitor)
        {
            if (testPackageConfig == null)
                throw new ArgumentNullException("testPackageConfig");
            if (testExplorationOptions == null)
                throw new ArgumentNullException("testExplorationOptions");
            if (testExecutionOptions == null)
                throw new ArgumentNullException("testExecutionOptions");
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");

            ThrowIfDisposed();
            if (state != State.Initialized)
                throw new InvalidOperationException("The test runner must be initialized before this operation is performed.");

            testPackageConfig = testPackageConfig.Copy();
            testPackageConfig.Canonicalize(null);
            testExplorationOptions = testExplorationOptions.Copy();
            testExecutionOptions = testExecutionOptions.Copy();

            using (progressMonitor.BeginTask("Running the tests.", 10))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Report report = new Report()
                {
                    TestPackageConfig = testPackageConfig,
                    TestModel = new TestModelData(),
                    TestPackageRun = new TestPackageRun()
                    {
                        StartTime = DateTime.Now
                    }
                };
                var reportLockBox = new LockBox<Report>(report);

                using (new LogReporter(reportLockBox, eventDispatcher))
                {
                    eventDispatcher.NotifyRunStarted(new RunStartedEventArgs(testPackageConfig, testExplorationOptions,
                        testExecutionOptions, reportLockBox));

                    bool success;
                    try
                    {
                        using (Listener listener = new Listener(eventDispatcher, reportLockBox))
                        {
                            testDriver.Run(testPackageConfig,
                                testExplorationOptions, listener,
                                testExecutionOptions, listener,
                                progressMonitor.CreateSubProgressMonitor(10));
                        }

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;

                        tappedLogger.Log(LogSeverity.Error,
                            "A fatal exception occurred while running tests.  Possible causes include invalid test runner parameters and stack overflows.",
                            ex);
                        report.TestModel.Annotations.Add(new AnnotationData(AnnotationType.Error,
                            CodeLocation.Unknown, CodeReference.Unknown, "A fatal exception occurred while running tests.  See log for details.", null));
                    }
                    finally
                    {
                        report.TestPackageRun.EndTime = DateTime.Now;
                        report.TestPackageRun.Statistics.Duration = stopwatch.Elapsed.TotalSeconds;
                    }

                    eventDispatcher.NotifyRunFinished(new RunFinishedEventArgs(success, report));
                }

                return report;
            }
        }

        /// <inheritdoc />
        public void Dispose(IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");
            if (state == State.Disposed)
                return;

            using (progressMonitor.BeginTask("Disposing the test runner.", 10))
            {
                bool success;
                try
                {
                    eventDispatcher.NotifyDisposeStarted(new DisposeStartedEventArgs());

                    if (testDriver != null)
                    {
                        progressMonitor.SetStatus("Disposing the test driver.");

                        testDriver.Dispose();
                        testDriver = null;
                    }

                    progressMonitor.Worked(10);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (tappedLogger != null)
                        tappedLogger.Log(LogSeverity.Warning, "An exception occurred while disposing the test driver.  This may indicate that the test driver previously encountered another fault from which it could not recover.", ex);
                    success = false;
                }

                state = State.Disposed;
                eventDispatcher.NotifyDisposeFinished(new DisposeFinishedEventArgs(success));
            }
        }

        private void OnLog(LogSeverity logSeverity, string message, ExceptionData exceptionData)
        {
            eventDispatcher.NotifyLogMessage(new LogMessageEventArgs(logSeverity, message, exceptionData));
        }

        private void ThrowIfDisposed()
        {
            if (state == State.Disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private sealed class Listener : ITestExplorationListener, ITestExecutionListener, IDisposable
        {
            private readonly TestRunnerEventDispatcher eventDispatcher;
            private readonly LockBox<Report> reportBox;

            private Dictionary<string, TestStepState> states;

            public Listener(TestRunnerEventDispatcher eventDispatcher, LockBox<Report> reportBox)
            {
                this.eventDispatcher = eventDispatcher;
                this.reportBox = reportBox;

                states = new Dictionary<string, TestStepState>();
            }

            public void Dispose()
            {
                reportBox.Write(report =>
                {
                    states = null;
                });
            }

            public void NotifySubtreeMerged(string parentTestId, TestData test)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    test = report.TestModel.MergeSubtree(parentTestId, test);

                    eventDispatcher.NotifyTestModelSubtreeMerged(
                        new TestModelSubtreeMergedEventArgs(report, test));
                });
            }

            public void NotifyAnnotationAdded(AnnotationData annotation)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    report.TestModel.Annotations.Add(annotation);

                    eventDispatcher.NotifyTestModelAnnotationAdded(
                        new TestModelAnnotationAddedEventArgs(report, annotation));
                });
            }

            public void NotifyTestStepStarted(TestStepData step)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestData testData = GetTestData(report, step.TestId);
                    TestStepRun testStepRun = new TestStepRun(step);
                    testStepRun.StartTime = DateTime.Now;

                    if (step.ParentId != null)
                    {
                        TestStepState parentState = GetTestStepState(step.ParentId);
                        parentState.TestStepRun.Children.Add(testStepRun);
                    }
                    else
                    {
                        report.TestPackageRun.RootTestStepRun = testStepRun;
                    }

                    TestStepState state = new TestStepState(testData, testStepRun);
                    states.Add(step.Id, state);

                    eventDispatcher.NotifyTestStepStarted(
                        new TestStepStartedEventArgs(report, testData, testStepRun));
                });
            }

            public void NotifyTestStepLifecyclePhaseChanged(string stepId, string lifecyclePhase)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);

                    eventDispatcher.NotifyTestStepLifecyclePhaseChanged(
                        new TestStepLifecyclePhaseChangedEventArgs(report, state.TestData, state.TestStepRun, lifecyclePhase));
                });
            }

            public void NotifyTestStepMetadataAdded(string stepId, string metadataKey, string metadataValue)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.TestStepRun.Step.Metadata.Add(metadataKey, metadataValue);

                    eventDispatcher.NotifyTestStepMetadataAdded(
                        new TestStepMetadataAddedEventArgs(report, state.TestData, state.TestStepRun, metadataKey, metadataValue));
                });
            }

            public void NotifyTestStepFinished(string stepId, TestResult result)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.TestStepRun.EndTime = DateTime.Now;
                    state.TestStepRun.Result = result;
                    report.TestPackageRun.Statistics.MergeStepStatistics(state.TestStepRun);

                    state.logWriter.Close();

                    eventDispatcher.NotifyTestStepFinished(
                        new TestStepFinishedEventArgs(report, state.TestData, state.TestStepRun));
                });
            }

            public void NotifyTestStepLogAttach(string stepId, Attachment attachment)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter.Attach(attachment);

                    eventDispatcher.NotifyTestStepLogAttach(
                        new TestStepLogAttachEventArgs(report, state.TestData, state.TestStepRun, attachment));
                });
            }

            public void NotifyTestStepLogStreamWrite(string stepId, string streamName, string text)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter[streamName].Write(text);

                    eventDispatcher.NotifyTestStepLogStreamWrite(
                        new TestStepLogStreamWriteEventArgs(report, state.TestData, state.TestStepRun, streamName, text));
                });
            }

            public void NotifyTestStepLogStreamEmbed(string stepId, string streamName, string attachmentName)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter[streamName].EmbedExisting(attachmentName);

                    eventDispatcher.NotifyTestStepLogStreamEmbed(
                        new TestStepLogStreamEmbedEventArgs(report, state.TestData, state.TestStepRun, streamName, attachmentName));
                });
            }

            public void NotifyTestStepLogStreamBeginSection(string stepId, string streamName, string sectionName)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter[streamName].BeginSection(sectionName);

                    eventDispatcher.NotifyTestStepLogStreamBeginSection(
                        new TestStepLogStreamBeginSectionEventArgs(report, state.TestData, state.TestStepRun, streamName, sectionName));
                });
            }

            public void NotifyTestStepLogStreamBeginMarker(string stepId, string streamName, Marker marker)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter[streamName].BeginMarker(marker);

                    eventDispatcher.NotifyTestStepLogStreamBeginMarker(
                        new TestStepLogStreamBeginMarkerEventArgs(report, state.TestData, state.TestStepRun, streamName, marker));
                });
            }

            public void NotifyTestStepLogStreamEnd(string stepId, string streamName)
            {
                reportBox.Write(report =>
                {
                    ThrowIfDisposed();

                    TestStepState state = GetTestStepState(stepId);
                    state.logWriter[streamName].End();

                    eventDispatcher.NotifyTestStepLogStreamEnd(
                        new TestStepLogStreamEndEventArgs(report, state.TestData, state.TestStepRun, streamName));
                });
            }

            private static TestData GetTestData(Report report, string testId)
            {
                TestData testData = report.TestModel.GetTestById(testId);
                if (testData == null)
                    throw new InvalidOperationException("The test id was not recognized.  It may belong to an earlier test run that has since completed.");
                return testData;
            }

            private TestStepState GetTestStepState(string testStepId)
            {
                TestStepState testStepData;
                if (!states.TryGetValue(testStepId, out testStepData))
                    throw new InvalidOperationException("The test step id was not recognized.  It may belong to an earlier test run that has since completed.");
                return testStepData;
            }

            private void ThrowIfDisposed()
            {
                if (states == null)
                    throw new ObjectDisposedException(GetType().Name);
            }

            private sealed class TestStepState
            {
                public readonly TestData TestData;
                public readonly TestStepRun TestStepRun;
                public readonly StructuredDocumentWriter logWriter;

                public TestStepState(TestData testData, TestStepRun testStepRun)
                {
                    TestData = testData;
                    TestStepRun = testStepRun;

                    logWriter = new StructuredDocumentWriter();
                    testStepRun.TestLog = logWriter.Document;
                }
            }
        }

        private sealed class TappedLogger : BaseLogger
        {
            private readonly DefaultTestRunner runner;
            private readonly ILogger inner;

            public TappedLogger(DefaultTestRunner runner, ILogger inner)
            {
                this.runner = runner;
                this.inner = inner;
            }

            protected override void LogImpl(LogSeverity severity, string message, ExceptionData exceptionData)
            {
                inner.Log(severity, message, exceptionData);
                runner.OnLog(severity, message, exceptionData);
            }
        }

        private sealed class LogReporter : IDisposable
        {
            private readonly LockBox<Report> reportLockBox;
            private readonly TestRunnerEventDispatcher eventDispatcher;

            public LogReporter(LockBox<Report> reportLockBox, TestRunnerEventDispatcher eventDispatcher)
            {
                this.reportLockBox = reportLockBox;
                this.eventDispatcher = eventDispatcher;

                eventDispatcher.LogMessage += OnLog;
            }

            public void Dispose()
            {
                eventDispatcher.LogMessage -= OnLog;
            }

            private void OnLog(object sender, LogMessageEventArgs e)
            {
                reportLockBox.Write(report => report.AddLogEntry(new LogEntry()
                {
                    Severity = e.Severity,
                    Message = e.Message,
                    Details = e.ExceptionData != null ? e.ExceptionData.ToString() : null
                }));
            }
        }
    }
}
