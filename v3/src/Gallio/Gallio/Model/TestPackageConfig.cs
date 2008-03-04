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
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using Gallio.Hosting;
using Gallio.Model.Serialization;
using Gallio.Utilities;

namespace Gallio.Model
{
    /// <summary>
    /// A test package configuration specifies the options used by a test runner to load tests
    /// into memory for execution.  The package may contain multiple test assemblies
    /// that are to be loaded together for test execution.  It can also be serialized as
    /// XML or using .Net remoting for persistence and remote operation.
    /// </summary>
    /// <remarks author="jeff">
    /// Someday a test package might allow other kinds of resources to be specified
    /// such as individual test scripts, archives, dependent files, environmental
    /// additional configuration settings, etc...
    /// </remarks>
    [Serializable]
    [XmlRoot("testPackageConfig", Namespace=SerializationUtils.XmlNamespace)]
    [XmlType(Namespace = SerializationUtils.XmlNamespace)]
    public sealed class TestPackageConfig
    {
        private readonly List<string> hintDirectories;
        private readonly List<string> assemblyFiles;

        private HostSetup hostSetup;

        /// <summary>
        /// Creates an empty package.
        /// </summary>
        public TestPackageConfig()
        {
            hintDirectories = new List<string>();
            assemblyFiles = new List<string>();
        }

        /// <summary>
        /// Gets the list of relative or absolute paths of test assembly files.
        /// </summary>
        [XmlArray("assemblyFiles", IsNullable = false)]
        [XmlArrayItem("assemblyFile", typeof(string), IsNullable = false)]
        public List<string> AssemblyFiles
        {
            get { return assemblyFiles; }
        }

        /// <summary>
        /// Gets the list of hint directories used to resolve test assemblies and other files.
        /// </summary>
        [XmlArray("hintDirectories", IsNullable=false)]
        [XmlArrayItem("hintDirectory", typeof(string), IsNullable=false)]
        public List<string> HintDirectories
        {
            get { return hintDirectories; }
        }

        /// <summary>
        /// Gets or sets the host setup parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlElement("hostSetup", IsNullable = false)]
        public HostSetup HostSetup
        {
            get
            {
                if (hostSetup == null)
                    Interlocked.CompareExchange(ref hostSetup, new HostSetup(), null);
                return hostSetup;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                hostSetup = value;
            }
        }

        /// <summary>
        /// Creates a copy of the test package.
        /// </summary>
        /// <returns>The copy</returns>
        public TestPackageConfig Copy()
        {
            TestPackageConfig copy = new TestPackageConfig();

            copy.assemblyFiles.AddRange(assemblyFiles);
            copy.hintDirectories.AddRange(hintDirectories);

            if (hostSetup != null)
                copy.hostSetup = hostSetup.Copy();

            return copy;
        }

        /// <summary>
        /// Makes all paths in this instance absolute.
        /// </summary>
        /// <param name="baseDirectory">The base directory for resolving relative paths,
        /// or null to use the current directory</param>
        public void Canonicalize(string baseDirectory)
        {
            FileUtils.CanonicalizePaths(baseDirectory, assemblyFiles);
            FileUtils.CanonicalizePaths(baseDirectory, hintDirectories);

            HostSetup.Canonicalize(baseDirectory);
        }
    }
}