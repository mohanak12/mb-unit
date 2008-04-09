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
using System.Runtime.Remoting;
using Gallio.Runtime.Logging;
using Gallio.Runtime;

namespace Gallio.Runtime.Hosting
{
    /// <summary>
    /// <para>
    /// A host service enables a local client to interact with a remotely
    /// executing hosting process.
    /// </para>
    /// <para>
    /// A host service implementation may choose to implement a keep-alive
    /// mechanism to automatically shut itself down when the service is disposed or
    /// when it has not received a ping within a set period of time.
    /// </para>
    /// </summary>
    public interface IHostService : IDisposable
    {
        /// <summary>
        /// Pings the host to verify and maintain connectivity.
        /// </summary>
        /// <exception cref="HostException">Thrown if the remote host is unreachable</exception>
        void Ping();

        /// <summary>
        /// <para>
        /// Asks the host to invoke the specified callback.
        /// </para>
        /// <para>
        /// The callback must be a serializable delegate so that it can be sent
        /// to the host and executed.
        /// </para>
        /// </summary>
        /// <param name="callback">The callback</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null</exception>
        void DoCallback(CrossAppDomainDelegate callback);

        /// <summary>
        /// Creates an instance of a remote object given an assembly name and type name.
        /// </summary>
        /// <param name="assemblyName">The name of assembly that contains the type</param>
        /// <param name="typeName">The full name of the type</param>
        /// <returns>The object handle of the instance</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyName"/> or
        /// <paramref name="typeName"/> is null</exception>
        ObjectHandle CreateInstance(string assemblyName, string typeName);

        /// <summary>
        /// Creates an instance of a remote object given an assembly path and type name.
        /// </summary>
        /// <param name="assemblyPath">The path of assembly that contains the type</param>
        /// <param name="typeName">The full name of the type</param>
        /// <returns>The object handle of the instance</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyPath"/> or
        /// <paramref name="typeName"/> is null</exception>
        ObjectHandle CreateInstanceFrom(string assemblyPath, string typeName);

        /// <summary>
        /// Initializes the runtime.
        /// </summary>
        /// <param name="runtimeFactory">The runtime factory</param>
        /// <param name="runtimeSetup">The runtime setup</param>
        /// <param name="logger">The logger</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="runtimeFactory"/>,
        /// <paramref name="runtimeSetup"/> or <paramref name="logger"/> is null</exception>
        void InitializeRuntime(RuntimeFactory runtimeFactory, RuntimeSetup runtimeSetup, ILogger logger);

        /// <summary>
        /// Shuts down the runtime.
        /// </summary>
        void ShutdownRuntime();
    }
}
