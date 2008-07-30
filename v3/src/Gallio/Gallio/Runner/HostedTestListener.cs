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
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Model.Serialization;
using Gallio.Runner.Events;
using Gallio.Runner.Reports;

namespace Gallio.Runner
{
    internal sealed class HostedTestListener : ITestListener, IDisposable
    {
        private readonly TestRunnerEventDispatcher eventDispatcher;
        private readonly Report report;
        private readonly object syncRoot;

        private Dictionary<string, TestStepState> states;

        public HostedTestListener(TestRunnerEventDispatcher eventDispatcher, Report report, object syncRoot)
        {
            this.eventDispatcher = eventDispatcher;
            this.report = report;
            this.syncRoot = syncRoot;

            states = new Dictionary<string, TestStepState>();
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                states = null;
            }
        }

        public void NotifyTestStepStarted(TestStepData step)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestData testData = GetTestData(step.TestId);
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
            }
        }

        public void NotifyTestStepLifecyclePhaseChanged(string stepId, string lifecyclePhase)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);

                eventDispatcher.NotifyTestStepLifecyclePhaseChanged(
                    new TestStepLifecyclePhaseChangedEventArgs(report, state.TestData, state.TestStepRun, lifecyclePhase));
            }
        }

        public void NotifyTestStepMetadataAdded(string stepId, string metadataKey, string metadataValue)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.TestStepRun.Step.Metadata.Add(metadataKey, metadataValue);

                eventDispatcher.NotifyTestStepMetadataAdded(
                    new TestStepMetadataAddedEventArgs(report, state.TestData, state.TestStepRun, metadataKey, metadataValue));
            }
        }

        public void NotifyTestStepFinished(string stepId, TestResult result)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.TestStepRun.EndTime = DateTime.Now;
                state.TestStepRun.Result = result;
                report.TestPackageRun.Statistics.MergeStepStatistics(state.TestStepRun);

                state.testLogWriter.Close();

                eventDispatcher.NotifyTestStepFinished(
                    new TestStepFinishedEventArgs(report, state.TestData, state.TestStepRun));
            }
        }

        public void NotifyTestStepLogAttachText(string stepId, string attachmentName, string contentType, string text)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.AttachText(attachmentName, contentType, text);

                eventDispatcher.NotifyTestStepLogAttachText(
                    new TestStepLogAttachTextEventArgs(report, state.TestData, state.TestStepRun, attachmentName, contentType, text));
            }
        }

        public void NotifyTestStepLogAttachBytes(string stepId, string attachmentName, string contentType, byte[] bytes)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.AttachBytes(attachmentName, contentType, bytes);

                eventDispatcher.NotifyTestStepLogAttachBytes(
                    new TestStepLogBinaryAttachBytesEventArgs(report, state.TestData, state.TestStepRun, attachmentName, contentType, bytes));
            }
        }

        public void NotifyTestStepLogStreamWrite(string stepId, string streamName, string text)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.Write(streamName, text);

                eventDispatcher.NotifyTestStepLogStreamWrite(
                    new TestStepLogStreamWriteEventArgs(report, state.TestData, state.TestStepRun, streamName, text));
            }
        }

        public void NotifyTestStepLogStreamEmbed(string stepId, string streamName, string attachmentName)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.Embed(streamName, attachmentName);

                eventDispatcher.NotifyTestStepLogStreamEmbed(
                    new TestStepLogStreamEmbedEventArgs(report, state.TestData, state.TestStepRun, streamName, attachmentName));
            }
        }

        public void NotifyTestStepLogStreamBeginSection(string stepId, string streamName, string sectionName)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.BeginSection(streamName, sectionName);

                eventDispatcher.NotifyTestStepLogStreamBeginSection(
                    new TestStepLogStreamBeginSectionEventArgs(report, state.TestData, state.TestStepRun, streamName, sectionName));
            }
        }

        public void NotifyTestStepLogStreamBeginMarker(string stepId, string streamName, string @class)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.BeginMarker(streamName, @class);

                eventDispatcher.NotifyTestStepLogStreamBeginMarker(
                    new TestStepLogStreamBeginMarkerEventArgs(report, state.TestData, state.TestStepRun, streamName, @class));
            }
        }

        public void NotifyTestStepLogStreamEnd(string stepId, string streamName)
        {
            lock (syncRoot)
            {
                ThrowIfDisposed();

                TestStepState state = GetTestStepState(stepId);
                state.testLogWriter.End(streamName);

                eventDispatcher.NotifyTestStepLogStreamEnd(
                    new TestStepLogStreamEndEventArgs(report, state.TestData, state.TestStepRun, streamName));
            }
        }

        private TestData GetTestData(string testId)
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
            public readonly TestLogWriter testLogWriter;

            public TestStepState(TestData testData, TestStepRun testStepRun)
            {
                TestData = testData;
                TestStepRun = testStepRun;

                testLogWriter = new TestLogWriter();
                testStepRun.TestLog = testLogWriter.TestLog;
            }
        }
    }
}
