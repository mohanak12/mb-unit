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
using System.Diagnostics;
using Gallio.Model.Logging;

namespace Gallio.Runner.Harness
{
    /// <summary>
    /// Sets up trace listeners.
    /// </summary>
    public class TraceTestEnvironment : ITestEnvironment
    {
        /// <inheritdoc />
        public IDisposable SetUp()
        {
            return new State();
        }

        private sealed class State : IDisposable
        {
            private ContextualLogTraceListener debugTraceListener;

            public State()
            {
                // Inject a trace listener for debug or trace messages.
                debugTraceListener = new ContextualLogTraceListener(TestLogStreamNames.DebugTrace);
                Trace.Listeners.Add(debugTraceListener);
                Trace.AutoFlush = true;
            }

            public void Dispose()
            {
                // Remove trace listener.
                if (debugTraceListener != null)
                {
                    Trace.Listeners.Remove(debugTraceListener);
                    debugTraceListener = null;
                }
            }
        }
    }
}
