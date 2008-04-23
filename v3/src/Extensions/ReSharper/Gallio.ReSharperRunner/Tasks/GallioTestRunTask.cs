// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
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
using System.Xml;
using Gallio.Collections;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Model.Filters;
using Gallio.Runner;
using Gallio.Runner.Events;
using Gallio.Runner.Reports;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using Gallio.Runtime.ProgressMonitoring;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace Gallio.ReSharperRunner.Tasks
{
    /// <summary>
    /// This is the root task for running Gallio tests.
    /// It must always appear first in a task sequence followed by
    /// any number <see cref="GallioTestItemTask" /> instances that describe the work to
    /// be done.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Equality comparison is used by ReSharper to coalesce sequences of tasks into a tree.
    /// Sequential tasks form a chain of nested nodes.  When identical tasks are found they are
    /// combined and subsequent tasks in the sequence become children of the common ancestor.
    /// </para>
    /// <para>
    /// To ensure that we have full control over the node structure, we introduce a root task
    /// whose purpose is to gather all of the constituent tasks under a common parent.
    /// </para>
    /// </remarks>
    /// <todo author="jeff">
    /// Handle test steps.
    /// Handle metadata.
    /// Handle custom log streams and attachments.
    /// </todo>
    [Serializable]
    public class GallioTestRunTask : GallioRemoteTask, IEquatable<GallioTestRunTask>
    {
        /// <summary>
        /// Gets a shared instance of the task.
        /// </summary>
        public static readonly GallioTestRunTask Instance = new GallioTestRunTask();

        public GallioTestRunTask()
        {
        }

        public GallioTestRunTask(XmlElement element)
            : base(element)
        {
        }

        public bool Equals(GallioTestRunTask other)
        {
            return other != null;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GallioTestRunTask);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override TaskResult ExecuteRecursive(IRemoteTaskServer server, TaskExecutionNode node)
        {
            new Runner(server).Run(node);
            return TaskResult.Success;
        }

        private sealed class Runner
        {
            private readonly IRemoteTaskServer server;
            private readonly HashSet<string> assemblyLocations;
            private readonly Dictionary<string, GallioTestItemTask> testTasks;
            private readonly HashSet<string> explicitTestIds;

            public Runner(IRemoteTaskServer server)
            {
                this.server = server;

                assemblyLocations = new HashSet<string>();
                testTasks = new Dictionary<string, GallioTestItemTask>();
                explicitTestIds = new HashSet<string>();
            }

            public void Run(TaskExecutionNode node)
            {
                ProcessNodeRecursively(node);
                RunTests();
            }

            private void ProcessNodeRecursively(TaskExecutionNode node)
            {
                ProcessTask(node.RemoteTask);

                foreach (TaskExecutionNode childNode in node.Children)
                    ProcessNodeRecursively(childNode);
            }

            private void ProcessTask(RemoteTask task)
            {
                GallioTestItemTask itemTask = task as GallioTestItemTask;
                if (itemTask != null)
                {
                    testTasks[itemTask.TestId] = itemTask;
                    return;
                }

                GallioTestAssemblyTask assemblyTask = task as GallioTestAssemblyTask;
                if (assemblyTask != null)
                {
                    assemblyLocations.Add(assemblyTask.AssemblyLocation);
                    return;
                }

                GallioTestExplicitTask explicitTask = task as GallioTestExplicitTask;
                if (explicitTask != null)
                {
                    explicitTestIds.Add(explicitTask.TestId);
                    return;
                }
            }

            private void RunTests()
            {
                ILogger logger = RuntimeAccessor.Logger;
                ITestRunner runner = TestRunnerUtils.CreateTestRunnerByName(StandardTestRunnerFactoryNames.Local);

                // Set parameters.
                TestPackageConfig packageConfig = new TestPackageConfig();
                packageConfig.AssemblyFiles.AddRange(assemblyLocations);

                TestRunnerOptions testRunnerOptions = new TestRunnerOptions();

                TestExplorationOptions testExplorationOptions = new TestExplorationOptions();

                TestExecutionOptions testExecutionOptions = new TestExecutionOptions();
                testExecutionOptions.Filter = new IdFilter<ITest>(new OrFilter<string>(GenericUtils.ConvertAllToArray<string, Filter<string>>(
                    explicitTestIds, delegate(string testId)
                    {
                        return new EqualityFilter<string>(testId);
                    })));

                // Install the listeners.
                runner.Events.TestStepStarted += TestStepStarted;
                runner.Events.TestStepFinished += TestStepFinished;
                runner.Events.TestStepLifecyclePhaseChanged += TestStepLifecyclePhaseChanged;

                // Run the tests.
                try
                {
                    runner.Initialize(testRunnerOptions, logger, CreateProgressMonitor());
                    runner.Load(packageConfig, CreateProgressMonitor());
                    runner.Explore(testExplorationOptions, CreateProgressMonitor());
                    runner.Run(testExecutionOptions, CreateProgressMonitor());
                }
                finally
                {
                    runner.Dispose(CreateProgressMonitor());
                }
            }

            private void TestStepStarted(object sender, TestStepStartedEventArgs e)
            {
                GallioTestItemTask testTask;
                if (testTasks.TryGetValue(e.Test.Id, out testTask))
                {
                    server.TaskStarting(testTask);
                }
            }

            private void TestStepLifecyclePhaseChanged(object sender, TestStepLifecyclePhaseChangedEventArgs e)
            {
                GallioTestItemTask testTask;
                if (testTasks.TryGetValue(e.Test.Id, out testTask))
                {
                    server.TaskProgress(testTask, e.LifecyclePhase);
                }
            }

            private void TestStepFinished(object sender, TestStepFinishedEventArgs e)
            {
                GallioTestItemTask testTask;
                if (testTasks.TryGetValue(e.Test.Id, out testTask))
                {
                    server.TaskProgress(testTask, "");

                    foreach (ExecutionLogStream stream in e.TestStepRun.ExecutionLog.Streams)
                        SubmitLogStreamContents(testTask, stream);

                    SubmitTestResult(testTask, e.TestStepRun.Result);
                }
            }

            private void SubmitLogStreamContents(GallioTestItemTask testTask, ExecutionLogStream stream)
            {
                string contents = string.Concat("*** ", stream.Name, " ***\n", stream.ToString(), "\n");

                switch (stream.Name)
                {
                    case LogStreamNames.ConsoleOutput:
                    default:
                        server.TaskOutput(testTask, contents, TaskOutputType.STDOUT);
                        break;

                    case LogStreamNames.ConsoleError:
                        server.TaskOutput(testTask, contents, TaskOutputType.STDERR);
                        break;

                    case LogStreamNames.DebugTrace:
                        server.TaskOutput(testTask, contents, TaskOutputType.DEBUGTRACE);
                        break;

                    case LogStreamNames.Warnings:
                        //server.TaskExplain(testTask, stream.ToString());
                        server.TaskOutput(testTask, contents, TaskOutputType.STDERR);
                        break;

                    case LogStreamNames.Failures:
                        //server.TaskError(testTask, stream.ToString());
                        server.TaskOutput(testTask, contents, TaskOutputType.STDERR);
                        break;
                }
            }

            private void SubmitTestResult(GallioTestItemTask testTask, TestResult result)
            {
                server.TaskFinished(testTask, result.Outcome.DisplayName, GetTaskResultForOutcome(result.Outcome));
            }

            private static TaskResult GetTaskResultForOutcome(TestOutcome outcome)
            {
                switch (outcome.Status)
                {
                    case TestStatus.Passed:
                        return TaskResult.Success;
                    case TestStatus.Failed:
                    case TestStatus.Inconclusive: // FIXME: not very accurate
                        if (outcome.Category == "error")
                            return TaskResult.Error;
                        return TaskResult.Exception;
                    case TestStatus.Skipped:
                        return TaskResult.Skipped;
                    default:
                        throw new ArgumentException("outcome");
                }
            }

            private static IProgressMonitor CreateProgressMonitor()
            {
                return NullProgressMonitor.CreateInstance();
            }
        }
    }
}
