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
using Gallio.Runtime.Logging;
using Gallio.Utilities;
using TestDriven.Framework;
using TDF = TestDriven.Framework;

namespace Gallio.TDNetRunner
{
    /// <summary>
    /// An <see cref="ILogger" /> implementation that writes messages to a
    /// <see cref="ITestListener" /> object.
    /// </summary>
    internal class TDNetLogger : BaseLogger
    {
        private readonly ITestListener testListener;

        /// <summary>
        /// Initializes a new instance of the TDNetLogger class.
        /// </summary>
        /// <param name="testListener">An ITestListener object where the
        /// messages will be written to.</param>
        public TDNetLogger(ITestListener testListener)
        {
            this.testListener = testListener;
        }

        /// <inheritdoc />
        protected override void LogImpl(LogSeverity severity, string message, Exception exception)
        {
            switch (severity)
            {
                case LogSeverity.Error:
                    testListener.WriteLine(message, Category.Error);
                    break;

                case LogSeverity.Warning:
                    testListener.WriteLine(message, Category.Warning);
                    break;

                case LogSeverity.Info:
                    testListener.WriteLine(message, Category.Info);
                    break;

                case LogSeverity.Debug:
                    testListener.WriteLine(message, Category.Debug);
                    break;
            }

            if (exception != null)
                testListener.WriteLine(ExceptionUtils.SafeToString(exception), Category.Error);
        }
    }
}
