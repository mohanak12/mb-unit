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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gallio.Concurrency;
using Gallio.Runtime.Hosting;
using Gallio.Utilities;
using Microsoft.Win32;

namespace Gallio.NCoverIntegration
{
#if NCOVER2
    internal static class NCoverTool
    {
        public static ProcessTask CreateProcessTask(string executablePath, string arguments, string workingDirectory)
        {
            string installDir = GetNCoverInstallDir();
            string ncoverConsolePath = Path.Combine(installDir, "NCover.Console.exe");

            StringBuilder ncoverArguments = new StringBuilder();
            ncoverArguments.Append('"').Append(executablePath).Append('"');
            ncoverArguments.Append(' ').Append(arguments);
            ncoverArguments.Append(" //w \"").Append(RemoveTrailingBackslash(workingDirectory)).Append('"');
            ncoverArguments.Append(" //x \"").Append(Path.Combine(workingDirectory, "Coverage.xml")).Append('"');
            //ncoverArguments.Append(" //l \"C:\\Temp\\Coverage.log\"");

            return new ProcessTask(ncoverConsolePath, ncoverArguments.ToString(), workingDirectory);
        }

        private static string GetNCoverInstallDir()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Gnoso\NCover")
                ?? Registry.LocalMachine.OpenSubKey(@"WOW6432Node\Software\Gnoso\NCover");
            if (key != null)
            {
                string installDir = (string) key.GetValue("InstallDir");
                if (installDir != null)
                    return installDir;
            }

            throw new HostException("NCover v2 does not appear to be installed.");
        }

        private static string RemoveTrailingBackslash(string path)
        {
            if (path.EndsWith(@"\"))
                return path.Substring(0, path.Length - 1);
            return path;
        }
    }
#endif
}
