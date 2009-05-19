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
using System.Xml;
using Gallio.Framework;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Common.Markup;
using Gallio.MSTestAdapter.Properties;
using Gallio.MSTestAdapter.Wrapper;
using Gallio.Runtime.Caching;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.MSTestAdapter.Model
{
    internal class MSTestController : BaseTestController
    {
        private readonly Version frameworkVersion;
        private readonly IDiskCache diskCache;

        internal MSTestController(Version frameworkVersion, IDiskCache diskCache)
        {
            if (frameworkVersion == null)
                throw new ArgumentNullException("frameworkVersion");
            if (diskCache == null)
                throw new ArgumentNullException("diskCache");

            this.frameworkVersion = frameworkVersion;
            this.diskCache = diskCache;
        }

        public static MSTestController CreateController(Version frameworkVersion)
        {
            return new MSTestController(frameworkVersion, new TemporaryDiskCache());
        }

        /// <inheritdoc />
        protected override TestOutcome RunTestsImpl(ITestCommand rootTestCommand, ITestStep parentTestStep, TestExecutionOptions options, IProgressMonitor progressMonitor)
        {
            using (progressMonitor.BeginTask(Resources.MSTestController_RunningMSTestTests, rootTestCommand.TestCount))
            {
                if (options.SkipTestExecution)
                {
                    SkipAll(rootTestCommand, parentTestStep);
                    return TestOutcome.Skipped;
                }
                else
                {
                    return RunTest(rootTestCommand, parentTestStep, progressMonitor);
                }
            }
        }

        private TestOutcome RunTest(ITestCommand testCommand, ITestStep parentTestStep, IProgressMonitor progressMonitor)
        {
            ITest test = testCommand.Test;
            progressMonitor.SetStatus(test.Name);

            // The first test should be an assembly test
            MSTestAssembly assemblyTest = testCommand.Test as MSTestAssembly;
            TestOutcome outcome;
            if (assemblyTest != null)
            {
                ITestContext assemblyContext = testCommand.StartPrimaryChildStep(parentTestStep);
                try
                {
                    MSTestRunner runner = MSTestRunner.GetRunnerForFrameworkVersion(frameworkVersion, diskCache);

                    outcome = runner.RunSession(assemblyContext, assemblyTest,
                        testCommand, parentTestStep, progressMonitor);
                }
                catch (Exception ex)
                {
                    assemblyContext.LogWriter.Failures.WriteException(ex, "Internal Error");
                    outcome = TestOutcome.Error;
                }

                assemblyContext.FinishStep(outcome, null);
            }
            else
            {
                outcome = TestOutcome.Skipped;
            }

            progressMonitor.Worked(1);
            return outcome;
        }
    }
}
