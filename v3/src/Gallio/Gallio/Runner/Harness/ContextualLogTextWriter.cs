// Copyright 2008 MbUnit Project - http://www.mbunit.com/
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
using System.IO;
using System.Text;
using Gallio.Logging;
using Gallio.Utilities;

namespace Gallio.Runner.Harness
{
    /// <summary>
    /// A contextual log text writer messages to a named log stream in the log associated
    /// with the test execution context that is active at the time each message is written.
    /// </summary>
    public sealed class ContextualLogTextWriter : TextWriter
    {
        private readonly string streamName;

        /// <summary>
        /// Creates a text writer that writes to the specified execution log stream.
        /// </summary>
        /// <param name="streamName">The execution log stream name</param>
        public ContextualLogTextWriter(string streamName)
        {
            this.streamName = streamName;

            base.NewLine = NewLine;
        }

        /// <inheritdoc />
        public override string NewLine
        {
            get { return "\n"; }
            set
            {
                throw new NotSupportedException("Cannot configure the new-line property of this text writer.");
            }
        }

        /// <inheritdoc />
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            try
            {
                CurrentLogStreamWriter.Write(value);
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("Could not write to the log stream.", ex);
            }
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            try
            {
                CurrentLogStreamWriter.Write(value);
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("Could not write to the log stream.", ex);
            }
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            try
            {
                CurrentLogStreamWriter.Write(buffer, index, count);
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("Could not write to the log stream.", ex);
            }
        }

        private LogStreamWriter CurrentLogStreamWriter
        {
            get { return Log.Writer[streamName]; }
        }
    }
}