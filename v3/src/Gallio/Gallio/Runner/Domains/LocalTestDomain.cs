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
using Gallio.Hosting;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Model.Serialization;
using Gallio.Runner.Harness;

namespace Gallio.Runner.Domains
{
    /// <summary>
    /// A local implementation of a test domain that performs all processing
    /// with the current app-domain including loading assemblies.
    /// </summary>
    /// <remarks>
    /// The <see cref="Runtime" /> must be initialized prior to the use of this domain.
    /// </remarks>
    public class LocalTestDomain : BaseTestDomain
    {
        private ITestHarnessFactory harnessFactory;
        private ITestHarness harness;

        private string oldWorkingDirectory;

        /// <summary>
        /// Creates a local test domain using the specified resolver manager.
        /// </summary>
        /// <param name="harnessFactory">The test harness factory</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="harnessFactory"/> is null</exception>
        public LocalTestDomain(ITestHarnessFactory harnessFactory)
        {
            if (harnessFactory == null)
                throw new ArgumentNullException(@"harnessFactory");

            this.harnessFactory = harnessFactory;
        }

        /// <inheritdoc />
        protected override void InternalDispose()
        {
            harnessFactory = null;
        }

        /// <inheritdoc />
        protected override TestPackageData InternalLoadTestPackage(TestPackageConfig packageConfig, IProgressMonitor progressMonitor)
        {
            progressMonitor.SetStatus("Creating test harness.");

            oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = packageConfig.HostSetup.ApplicationBaseDirectory;

            harness = harnessFactory.CreateHarness();

            progressMonitor.Worked(0.1);

            harness.LoadTestPackage(packageConfig, progressMonitor.CreateSubProgressMonitor(0.9));
            return new TestPackageData(harness.TestPackage);
        }

        /// <inheritdoc />
        protected override TestModelData InternalBuildTestModel(TestEnumerationOptions options, IProgressMonitor progressMonitor)
        {
            harness.BuildTestModel(options, progressMonitor.CreateSubProgressMonitor(1));
            return new TestModelData(harness.TestModel);
        }

        /// <inheritdoc />
        protected override void InternalRunTests(TestExecutionOptions options, ITestListener listener, IProgressMonitor progressMonitor)
        {
            harness.RunTests(options, listener, progressMonitor.CreateSubProgressMonitor(1));
        }

        /// <inheritdoc />
        protected override void InternalUnloadTestPackage(IProgressMonitor progressMonitor)
        {
            try
            {
                progressMonitor.SetStatus("Disposing test harness.");

                if (harness != null)
                    harness.Dispose();

                progressMonitor.Worked(1);
            }
            finally
            {
                harness = null;

                if (oldWorkingDirectory != null)
                {
                    Environment.CurrentDirectory = oldWorkingDirectory;
                    oldWorkingDirectory = null;
                }
            }
        }
    }
}