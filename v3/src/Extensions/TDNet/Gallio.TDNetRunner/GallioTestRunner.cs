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
using System.Reflection;
using Gallio.Loader;
using Gallio.Runtime.Logging;
using Gallio.TDNetRunner.Core;
using Gallio.TDNetRunner.Facade;
using Gallio.TDNetRunner.Properties;

namespace Gallio.TDNetRunner
{
    /// <summary>
    /// Gallio test runner for TestDriven.NET.
    /// </summary>
    public class GallioTestRunner : BaseFacadeTestRunner
    {
        internal readonly static string testRunnerName = typeof(GallioTestRunner).FullName;
        private IGallioTestRunner testRunner;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (testRunner != null)
            {
                if (disposing)
                    testRunner.Dispose();

                testRunner = null;
            }
        }

        /// <inheritdoc />
        protected override FacadeTestRunState Run(IFacadeTestListener testListener, string assemblyPath, string cref)
        {
            Version appVersion = Assembly.GetCallingAssembly().GetName().Version;
            testListener.WriteLine(String.Format(Resources.RunnerNameAndVersion + "\n",
                appVersion.Major, appVersion.Minor, appVersion.Build, appVersion.Revision), FacadeCategory.Info);

            EnsureTestRunnerIsCreated();
            return testRunner.Run(testListener, assemblyPath, cref);
        }

        /// <inheritdoc />
        protected override void Abort()
        {
            if (testRunner != null)
                testRunner.Abort();
        }

        internal virtual IGallioTestRunner CreateTestRunner()
        {
            try
            {
                IGallioRemoteEnvironment environment = EnvironmentManager.GetSharedEnvironment();

                AppDomain.CurrentDomain.AssemblyResolve += ResolveRunnerAssembly;

                Type runnerType = typeof(RemoteGallioTestRunner);
                object runner = environment.AppDomain.CreateInstanceAndUnwrap(runnerType.Assembly.FullName, runnerType.FullName);

                return (IGallioTestRunner)runner;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveRunnerAssembly;
            }
        }

        private void EnsureTestRunnerIsCreated()
        {
            if (testRunner == null)
                testRunner = CreateTestRunner();
        }

        /// <summary>
        /// <para>
        /// This resolver is used to ensure that we can cast the test runner's transparent proxy to IGallioTestRunner.
        /// </para>
        /// <para>
        /// TestDriven.Net initially loaded this assembly using Assembly.LoadFrom.  When the cast occurs, the runtime implicitly
        /// tries to load the interface using Assembly.Load by fullname which does not normally consider anything loaded with LoadFrom.
        /// So we introduce a resolver that recognizes when we are attempting to load this assembly by fullname and
        /// just returns it.  Without it, an InvalidCastException will occur.
        /// </para>
        /// </summary>
        private static Assembly ResolveRunnerAssembly(object sender, ResolveEventArgs e)
        {
            Assembly runnerAssembly = typeof(GallioTestRunner).Assembly;
            return e.Name == runnerAssembly.FullName ? runnerAssembly : null;
        }
    }
}