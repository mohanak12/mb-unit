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
using System.IO;
using Gallio.Common.Reflection;

namespace Gallio.Common.Platform
{
    /// <summary>
    /// Provides support for working with different implementations of the .Net runtime.
    /// </summary>
    public static class DotNetRuntimeSupport
    {
        private static Memoizer<DotNetRuntimeType> runtimeTypeMemoizer = new Memoizer<DotNetRuntimeType>();

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
            get { return RuntimeType == DotNetRuntimeType.Mono; }
        }

        /// <summary>
        /// Get the type of the .Net runtime currently executing this process.
        /// </summary>
        public static DotNetRuntimeType RuntimeType
        {
            get
            {
                return runtimeTypeMemoizer.Memoize(() =>
                {
                    if (Type.GetType(@"Mono.Runtime") != null)
                        return DotNetRuntimeType.Mono;

                    return DotNetRuntimeType.CLR;
                });
            }
        }

        /// <summary>
        /// When using Mono, creates <see cref="ProcessStartInfo" /> that re-enters the Mono runtime
        /// if the executable is .Net otherwise creates a standard process start info.
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="arguments">The arguments</param>
        /// <returns>The process start info</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="executablePath"/>
        /// or <paramref name="arguments"/> is null</exception>
        public static ProcessStartInfo CreateReentrantProcessStartInfo(string executablePath, string arguments)
        {
            if (executablePath == null)
                throw new ArgumentNullException("executablePath");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            if (IsUsingMono && AssemblyUtils.IsAssembly(executablePath))
            {
                string monoExe = GetCurrentMonoRuntimePath();
                if (monoExe != null)
                    return new ProcessStartInfo(monoExe, String.Concat("\"", executablePath, "\" ", arguments));
            }

            return new ProcessStartInfo(executablePath, arguments);
        }

        private static string GetCurrentMonoRuntimePath()
        {
            Process process = Process.GetCurrentProcess();
            if (process.MainModule != null)
                return process.MainModule.FileName;

            return null;
        }
    }
}