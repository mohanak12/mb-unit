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
using System.IO;
using System.Reflection;
using Gallio.Model;
using Gallio.CSUnitAdapter.Properties;
using Gallio.Runtime.Hosting;

namespace Gallio.CSUnitAdapter.Model
{
    /// <summary>
    /// Builds a test object model based on reflection against CSUnit framework attributes.
    /// </summary>
    public class CSUnitTestFramework : BaseTestFramework
    {
        private static readonly string[] frameworkAssemblyFiles = new string[]
        {
            @"csUnit.dll",
            @"csUnitCore.dll",
            @"csUnit.Common.dll",
            @"csUnit.Interfaces.dll",
        };

        /// <inheritdoc />
        public override string Name
        {
            get { return Resources.CSUnitTestFramework_FrameworkName; }
        }

        /// <inheritdoc />
        public override ITestExplorer CreateTestExplorer(TestModel testModel)
        {
            return new CSUnitTestExplorer(testModel);
        }

        /// <inheritdoc />
        public override void ConfigureTestDomain(TestDomainSetup testDomainSetup)
        {
            foreach (string assembly in testDomainSetup.TestPackageConfig.AssemblyFiles)
            {
                string dir = Path.GetDirectoryName(assembly);
                if (ConfigureAssemblyRedirects(dir, testDomainSetup.TestPackageConfig.HostSetup.Configuration))
                    return;
            }
        }

        private static bool ConfigureAssemblyRedirects(string dir, HostConfiguration config)
        {
            int found = 0;
            foreach (string file in frameworkAssemblyFiles)
            {
                string path = Path.Combine(dir, file);
                if (File.Exists(path))
                {
                    try
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                        config.AddAssemblyBinding(assemblyName, new Uri(path).ToString(), true);
                        found++;
                    }
                    catch
                    {
                        // ignore loading errors
                    }
                }
            }
            return found > 0;
        }
    }
}
