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
using System.Xml.Serialization;
using Gallio.Collections;
using Gallio.Model.Serialization;

namespace Gallio.Runner.Reports
{
    /// <summary>
    /// Summarizes the execution of a test package for reporting purposes.
    /// </summary>
    [Serializable]
    [XmlType(Namespace = SerializationUtils.XmlNamespace)]
    public sealed class PackageRun
    {
        private PackageRunStatistics statistics;
        private TestStepRun rootTestInstanceRun;
        private DateTime startTime;
        private DateTime endTime;

        /// <summary>
        /// Creates an empty package run.
        /// </summary>
        public PackageRun()
        {
        }

        /// <summary>
        /// Gets or sets the time when the package run started.
        /// </summary>
        [XmlAttribute("startTime")]
        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        /// <summary>
        /// Gets or sets the time when the package run ended.
        /// </summary>
        [XmlAttribute("endTime")]
        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        /// <summary>
        /// Gets or sets the root test step run, or null if the root test has not run.
        /// </summary>
        [XmlElement("testStepRun", IsNullable = false)]
        public TestStepRun RootTestStepRun
        {
            get { return rootTestInstanceRun; }
            set { rootTestInstanceRun = value; }
        }

        /// <summary>
        /// Gets or sets the statistics for the package run.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlElement("statistics", Namespace = SerializationUtils.XmlNamespace, IsNullable = false)]
        public PackageRunStatistics Statistics
        {
            get
            {
                if (statistics == null)
                    statistics = new PackageRunStatistics();
                return statistics;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                statistics = value;
            }
        }

        /// <summary>
        /// Recursively enumerates all test step runs.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<TestStepRun> TestStepRuns
        {
            get
            {
                if (rootTestInstanceRun == null)
                    return EmptyArray<TestStepRun>.Instance;

                return TreeUtils.GetPreOrderTraversal(rootTestInstanceRun, GetChildren);
            }
        }

        private static IEnumerable<TestStepRun> GetChildren(TestStepRun node)
        {
            return node.Children;
        }
    }
}
