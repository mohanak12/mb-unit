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
using System.Text;
using Gallio.Model;
using Gallio.Model.Logging;
using Gallio.Runner.Events;
using Gallio.Runner.Extensions;
using Gallio.Runtime.Logging;
using Gallio.Runner;

namespace Gallio.TeamCityIntegration
{
    /// <summary>
    /// Monitors <see cref="ITestRunner" /> events and writes debug messages to the
    /// runner's logger.
    /// </summary>
    public class TeamCityExtension : TestRunnerExtension
    {
        private ServiceMessageWriter writer;

        /// <inheritdoc />
        protected override void Initialize()
        {
            writer = new ServiceMessageWriter(output => Logger.Log(LogSeverity.Important, output));

            Events.InitializeStarted += delegate(object sender, InitializeStartedEventArgs e)
            {
                writer.WriteProgressMessage("Initializing test runner.");
            };

            Events.LoadStarted += delegate(object sender, LoadStartedEventArgs e)
            {
                writer.WriteProgressMessage("Loading tests.");
            };

            Events.ExploreStarted += delegate(object sender, ExploreStartedEventArgs e)
            {
                writer.WriteProgressMessage("Exploring tests.");
            };

            Events.RunStarted += delegate(object sender, RunStartedEventArgs e)
            {
                writer.WriteProgressStart("Running tests.");
            };

            Events.RunFinished += delegate(object sender, RunFinishedEventArgs e)
            {
                writer.WriteProgressFinish("Running tests."); // nb: message must be same as specified in progress start
            };

            Events.UnloadFinished += delegate(object sender, UnloadFinishedEventArgs e)
            {
                writer.WriteProgressMessage("Unloaded tests.");
            };

            Events.DisposeFinished += delegate(object sender, DisposeFinishedEventArgs e)
            {
                writer.WriteProgressMessage("Disposed test runner.");
            };

            Events.TestStepStarted += delegate(object sender, TestStepStartedEventArgs e)
            {
                if (e.TestStepRun.Step.IsPrimary)
                {
                    string name = e.TestStepRun.Step.FullName;
                    if (name.Length != 0)
                    {
                        if (e.TestStepRun.Step.IsTestCase)
                        {
                            writer.WriteTestStarted(name);
                        }
                        else
                        {
                            writer.WriteTestSuiteStarted(name);
                        }
                    }
                }
            };

            Events.TestStepFinished += delegate(object sender, TestStepFinishedEventArgs e)
            {
                if (e.TestStepRun.Step.IsPrimary)
                {
                    string name = e.TestStepRun.Step.FullName;
                    if (name.Length != 0)
                    {
                        if (e.TestStepRun.Step.IsTestCase)
                        {
                            TestOutcome outcome = e.TestStepRun.Result.Outcome;

                            var outputText = new StringBuilder();
                            var errorText = new StringBuilder();
                            var warningText = new StringBuilder();
                            var failureText = new StringBuilder();

                            foreach (StructuredTestLogStream stream in e.TestStepRun.TestLog.Streams)
                            {
                                switch (stream.Name)
                                {
                                    default:
                                    case TestLogStreamNames.ConsoleInput:
                                    case TestLogStreamNames.ConsoleOutput:
                                    case TestLogStreamNames.DebugTrace:
                                    case TestLogStreamNames.Default:
                                        AppendWithSeparator(outputText, stream.ToString());
                                        break;

                                    case TestLogStreamNames.ConsoleError:
                                        AppendWithSeparator(errorText, stream.ToString());
                                        break;

                                    case TestLogStreamNames.Failures:
                                        AppendWithSeparator(failureText, stream.ToString());
                                        break;

                                    case TestLogStreamNames.Warnings:
                                        AppendWithSeparator(warningText, stream.ToString());
                                        break;
                                }
                            }

                            if (outcome.Status != TestStatus.Skipped && warningText.Length != 0)
                                AppendWithSeparator(errorText, warningText.ToString());
                            if (outcome.Status != TestStatus.Failed && failureText.Length != 0)
                                AppendWithSeparator(errorText, failureText.ToString());

                            if (outputText.Length != 0)
                                writer.WriteTestStdOut(name, outputText.ToString());
                            if (errorText.Length != 0)
                                writer.WriteTestStdErr(name, errorText.ToString());

                            // TODO: Handle inconclusive.
                            if (outcome.Status == TestStatus.Failed)
                            {
                                writer.WriteTestFailed(name, outcome.ToString(), failureText.ToString());
                            }
                            else if (outcome.Status == TestStatus.Skipped)
                            {
                                writer.WriteTestIgnored(name, warningText.ToString());
                            }

                            writer.WriteTestFinished(name);
                        }
                        else
                        {
                            writer.WriteTestSuiteFinished(name);
                        }
                    }
                }
            };
        }

        private static void AppendWithSeparator(StringBuilder builder, string text)
        {
            if (text.Length != 0)
            {
                if (builder.Length != 0)
                    builder.Append("\n\n");

                builder.Append(text);

                while (builder.Length > 0 && char.IsWhiteSpace(builder[builder.Length - 1]))
                    builder.Length -= 1;
            }
        }
    }
}
