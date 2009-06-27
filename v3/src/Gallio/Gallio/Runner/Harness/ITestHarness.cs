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
using Gallio.Model.Messages;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Model;
using Gallio.Model.Execution;

namespace Gallio.Runner.Harness
{
    /// <summary>
    /// The test harness manages the lifecycle of test enumeration and execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contributors (such as test framework adapters) may attach event handlers
    /// to extend or modify the behavior at distinct points in the lifecycle.
    /// A new test harness instance is created when a test project is loaded into
    /// memory to serve as the ultimate container for all of its related resources.
    /// </para>
    /// </remarks>
    public interface ITestHarness : IDisposable
    {
        /// <summary>
        /// Gets the test exploration context in the test harness, or null if none.
        /// </summary>
        TestExplorationContext TestExplorationContext { get; }

        /// <summary>
        /// Gets the test model, or null if the test model has not been built.
        /// </summary>
        TestModel TestModel { get; }

        /// <summary>
        /// Adds a test environment.
        /// </summary>
        /// <param name="environment">The test framework to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="environment"/> is null.</exception>
        void AddTestEnvironment(ITestEnvironment environment);

        /// <summary>
        /// Loads a test package into the test harness.
        /// </summary>
        /// <param name="packageConfig">The test package configuration.</param>
        /// <param name="progressMonitor">The progress monitor.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="progressMonitor"/> or <paramref name="packageConfig"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Load" /> has already been called once
        /// because this interface does not support unloading packages except by disposing the harness
        /// and recreating it.</exception>
        void Load(TestPackageConfig packageConfig, IProgressMonitor progressMonitor);

        /// <summary>
        /// Populates the test model.
        /// </summary>
        /// <param name="testExplorationOptions">The test exploration options.</param>
        /// <param name="testExplorationListener">The test exploration listener.</param>
        /// <param name="progressMonitor">The progress monitor.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="progressMonitor"/> or <paramref name="testExplorationOptions"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Load" /> has not been called yet.</exception>
        void Explore(TestExplorationOptions testExplorationOptions, ITestExplorationListener testExplorationListener, IProgressMonitor progressMonitor);

        /// <summary>
        /// Runs the tests.
        /// </summary>
        /// <param name="testExecutionOptions">The test execution options.</param>
        /// <param name="testExecutionListener">The test execution listener.</param>
        /// <param name="progressMonitor">The progress monitor.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="progressMonitor"/>,
        /// <paramref name="testExecutionListener"/> or <paramref name="testExecutionOptions"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Explore" /> has not been called yet.</exception>
        void Run(TestExecutionOptions testExecutionOptions, ITestExecutionListener testExecutionListener, IProgressMonitor progressMonitor);

        /// <summary>
        /// Unloads the tests.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="progressMonitor"/> is null.</exception>
        void Unload(IProgressMonitor progressMonitor);
    }
}