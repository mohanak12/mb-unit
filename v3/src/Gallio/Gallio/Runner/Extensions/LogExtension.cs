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
using System.Text;
using Gallio.Runner.Events;
using Gallio.Runtime.Logging;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Model.Serialization;
using Gallio.Reflection;
using Gallio.Runner.Reports;

namespace Gallio.Runner.Extensions
{
    /// <summary>
    /// <para>
    /// The log extension writes a summary of test execution progress to the test runner's logger
    /// so the user can monitor what's going on.
    /// </para>
    /// Passing tests are recorded with severity <see cref="LogSeverity.Info" />, warnings are
    /// recorded with severity <see cref="LogSeverity.Warning" /> and failures are recorded
    /// with severity <see cref="LogSeverity.Error" />.
    /// </summary>
    public class LogExtension : TestRunnerExtension
    {
        /// <inheritdoc />
        protected override void Initialize()
        {
            Events.ExploreFinished += delegate(object sender, ExploreFinishedEventArgs e)
            {
                foreach (AnnotationData annotation in e.Report.TestModel.Annotations)
                    LogAnnotation(annotation);
            };

            Events.TestStepStarted += delegate(object sender, TestStepStartedEventArgs e)
            {
                LogTestStepStarted(e);
            };

            Events.TestStepFinished += delegate(object sender, TestStepFinishedEventArgs e)
            {
                LogTestStepFinished(e);
            };
        }

        /// <summary>
        /// Logs an annotation.
        /// </summary>
        /// <param name="annotation">The annotation to log</param>
        protected virtual void LogAnnotation(AnnotationData annotation)
        {
            StringBuilder message = new StringBuilder();
            message.Append(annotation.Message);

            if (annotation.CodeLocation != CodeLocation.Unknown)
            {
                message.Append("\n\tLocation: ");
                message.Append(annotation.CodeLocation);
            }

            if (annotation.CodeLocation.Line == 0 && annotation.CodeReference != CodeReference.Unknown)
            {
                message.Append("\n\tReference: ");
                message.Append(annotation.CodeReference);
            }

            if (!string.IsNullOrEmpty(annotation.Details))
            {
                message.Append("\n\tDetails: ");
                message.Append(annotation.Details);
            }

            LogSeverity severity = GetLogSeverityForAnnotation(annotation.Type);
            Logger.Log(severity, message.ToString());
        }

        /// <summary>
        /// Logs a test step started event.
        /// </summary>
        /// <param name="e">The event</param>
        protected virtual void LogTestStepStarted(TestStepStartedEventArgs e)
        {
            Logger.Log(LogSeverity.Debug, String.Format("[starting] {0}", e.TestStepRun.Step.FullName));
        }

        /// <summary>
        /// Logs a test step finished event.
        /// </summary>
        /// <param name="e">The event</param>
        protected virtual void LogTestStepFinished(TestStepFinishedEventArgs e)
        {
            TestOutcome outcome = e.TestStepRun.Result.Outcome;
            LogSeverity severity = GetLogSeverityForOutcome(outcome);
            string warnings = FormatStream(e.TestStepRun, LogStreamNames.Warnings);
            string failures = FormatStream(e.TestStepRun, LogStreamNames.Failures);

            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendFormat("[{0}] {1} {2}", outcome.DisplayName, e.GetStepKind(), e.TestStepRun.Step.FullName);

            if (warnings.Length != 0)
            {
                if (severity < LogSeverity.Warning)
                    severity = LogSeverity.Warning;

                messageBuilder.AppendLine();
                messageBuilder.Append(warnings);
                messageBuilder.AppendLine();
            }

            if (failures.Length != 0)
            {
                if (severity < LogSeverity.Error)
                    severity = LogSeverity.Error;

                messageBuilder.AppendLine();
                messageBuilder.Append(failures);
                messageBuilder.AppendLine();
            }

            Logger.Log(severity, messageBuilder.ToString());
        }

        private static string FormatStream(TestStepRun testStepRun, string streamName)
        {
            ExecutionLogStream stream = testStepRun.ExecutionLog.GetStream(streamName);
            return stream != null ? stream.ToString() : @"";
        }

        private static LogSeverity GetLogSeverityForOutcome(TestOutcome outcome)
        {
            switch (outcome.Status)
            {
                case TestStatus.Passed:
                    return LogSeverity.Info;

                case TestStatus.Skipped:
                case TestStatus.Inconclusive:
                    return LogSeverity.Warning;

                case TestStatus.Failed:
                    return LogSeverity.Error;

                default:
                    throw new ArgumentException("outcome");
            }
        }

        private static LogSeverity GetLogSeverityForAnnotation(AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.Error:
                    return LogSeverity.Error;

                case AnnotationType.Warning:
                    return LogSeverity.Warning;

                case AnnotationType.Info:
                    return LogSeverity.Info;

                default:
                    throw new ArgumentException("type");
            }
        }
    }
}
