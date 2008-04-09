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
using Gallio.Runtime;
using Gallio.Utilities;
using Gallio.Runtime.Logging;

namespace Gallio.Runtime
{
    /// <summary>
    /// <para>
    /// Provides functions for obtaining runtime services.
    /// </para>
    /// </summary>
    public static class RuntimeAccessor
    {
        private static IRuntime instance;

        /// <summary>
        /// Gets the runtime instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the runtime has not been initialized</exception>
        public static IRuntime Instance
        {
            get
            {
                if (instance == null)
                    throw new InvalidOperationException("The runtime has not been initialized.");

                return instance;
            }
        }

        /// <summary>
        /// Gets the runtime's logger, or a <see cref="NullLogger" /> if the runtime is not initialized.
        /// </summary>
        public static ILogger Logger
        {
            get
            {
                IRuntime cachedInstance = instance;
                return cachedInstance != null ? cachedInstance.Resolve<ILogger>() : NullLogger.Instance;
            }
        }

        /// <summary>
        /// Returns true if the runtime has been initialized.
        /// </summary>
        public static bool IsInitialized
        {
            get { return instance != null; }
        }

        /// <summary>
        /// The event dispatched when the value of the current runtime
        /// <see cref="Instance" /> changes.
        /// </summary>
        public static event EventHandler InstanceChanged;

        /// <summary>
        /// Returns true if the application is running within the Mono runtime.
        /// </summary>
        /// <remarks>
        /// It is occasionally necessary to tailor the execution of the test runner
        /// depending on whether Mono is running.  However, the number of such
        /// customizations should be very limited.
        /// </remarks>
        public static bool IsUsingMono
        {
            get { return Type.GetType(@"Mono.Runtime") != null; }
        }

        /// <summary>
        /// Gets the local path of the Gallio installation.
        /// </summary>
        /// <returns>The installation path</returns>
        public static string InstallationPath
        {
            get
            {
                return Instance.GetRuntimeSetup().InstallationPath;
            }
        }

        /// <summary>
        /// <para>
        /// Sets the runtime instance.
        /// </para>
        /// <para>
        /// This method should only be used by applications that host Gallio
        /// and not generally by client code.
        /// </para>
        /// </summary>
        /// <param name="runtime">The runtime instance, or null if none</param>
        public static void SetRuntime(IRuntime runtime)
        {
            EventHandler instanceChangedHandlers = InstanceChanged;
            instance = runtime;

            EventHandlerUtils.SafeInvoke(instanceChangedHandlers, null, EventArgs.Empty);
        }
    }
}
