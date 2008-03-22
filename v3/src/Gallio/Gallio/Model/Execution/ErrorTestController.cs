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
using Gallio.Hosting.ProgressMonitoring;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// A test controller that emits the description of the test as a runtime error.
    /// </summary>
    public class ErrorTestController : BaseTestController
    {
        /// <inheritdoc />
        protected override void RunTestsInternal(ITestCommand rootTestCommand, ITestStep parentTestStep,
            TestExecutionOptions options, IProgressMonitor progressMonitor)
        {
            ITestContext testContext = rootTestCommand.StartPrimaryChildStep(parentTestStep);

            testContext.LogWriter.Write(LogStreamNames.Failures, String.Format("An error occurred during test enumeration.  {0}\n",
                rootTestCommand.Test.Metadata[MetadataKeys.Description]));

            testContext.FinishStep(TestOutcome.Error, null);
        }
    }
}
