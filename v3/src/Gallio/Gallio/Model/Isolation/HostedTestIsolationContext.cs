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
using Gallio.Common.Collections;
using Gallio.Common.Policies;
using Gallio.Runtime;
using Gallio.Runtime.Hosting;
using Gallio.Runtime.Logging;

namespace Gallio.Model.Isolation
{
    /// <summary>
    /// A hosted test isolation context created by <see cref="HostedTestIsolationProvider" />.
    /// </summary>
    public class HostedTestIsolationContext : BaseTestIsolationContext
    {
        private readonly IHostFactory hostFactory;
        private readonly TestIsolationOptions testIsolationOptions;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a hosted test isolation context.
        /// </summary>
        /// <param name="hostFactory">The host factory.</param>
        /// <param name="testIsolationOptions">The test isolation options.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hostFactory"/>,
        /// <paramref name="testIsolationOptions"/> or <paramref name="logger"/> is null.</exception>
        public HostedTestIsolationContext(IHostFactory hostFactory,
            TestIsolationOptions testIsolationOptions, ILogger logger)
        {
            if (hostFactory == null)
                throw new ArgumentNullException("hostFactory");
            if (testIsolationOptions == null)
                throw new ArgumentNullException("testIsolationOptions");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.hostFactory = hostFactory;
            this.testIsolationOptions = testIsolationOptions;
            this.logger = logger;
        }

        /// <inheritdoc />
        protected override object RunIsolatedTaskImpl<TIsolatedTask>(HostSetup hostSetup, StatusReporter statusReporter, object[] args)
        {
            hostSetup = hostSetup.Copy();
            foreach (var pair in testIsolationOptions.Properties)
                if (! hostSetup.Properties.ContainsKey(pair.Key))
                    hostSetup.Properties.Add(pair);

            IHost host = null;
            try
            {
                statusReporter("Creating test host.");
                host = hostFactory.CreateHost(hostSetup, logger);

                IRuntime runtime = RuntimeAccessor.Instance;
                RemoteLogger remoteLogger = new RemoteLogger(runtime.Logger);

                Shim shim = HostUtils.CreateInstance<Shim>(host);
                try
                {
                    statusReporter("Initializing the runtime.");
                    shim.Initialize(runtime.GetRuntimeSetup(), remoteLogger);
                    statusReporter("");

                    TIsolatedTask isolatedTask = null;
                    try
                    {
                        isolatedTask = HostUtils.CreateInstance<TIsolatedTask>(host);
                        return isolatedTask.Run(args);
                    }
                    finally
                    {
                        try
                        {
                            if (isolatedTask != null)
                                isolatedTask.Dispose();
                        }
                        catch (Exception ex)
                        {
                            UnhandledExceptionPolicy.Report("An unhandled exception occurred while disposing the isolated task.", ex);
                        }
                    }
                }
                finally
                {
                    statusReporter("Shutting down the runtime.");
                    shim.Shutdown();

                    GC.KeepAlive(remoteLogger);
                }
            }
            finally
            {
                statusReporter("Disposing test host.");

                try
                {
                    if (host != null)
                        host.Dispose();
                }
                catch (Exception ex)
                {
                    UnhandledExceptionPolicy.Report("An exception occurred while disposing the test host.", ex);
                }

                statusReporter("");
            }
        }

        private sealed class Shim : MarshalByRefObject
        {
            private bool initializedRuntime;

            public void Initialize(RuntimeSetup runtimeSetup, ILogger logger)
            {
                if (!RuntimeAccessor.IsInitialized)
                {
                    RuntimeBootstrap.Initialize(runtimeSetup, logger);
                    initializedRuntime = true;
                }
            }

            public void Shutdown()
            {
                if (initializedRuntime)
                {
                    initializedRuntime = false;
                    RuntimeBootstrap.Shutdown();
                }
            }
        }
    }
}