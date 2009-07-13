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
using System.Xml.Serialization;
using Gallio.Common;
using Gallio.Common.Xml;

namespace Gallio.Model
{
    /// <summary>
    /// A test result describes the final result of having executed a test.
    /// </summary>
    [Serializable]
    [XmlType(Namespace=SchemaConstants.XmlNamespace)]
    public sealed class TestResult
    {
        private TestOutcome outcome = TestOutcome.Inconclusive;
        private int assertCount;
        private double duration;

        /// <summary>
        /// Gets or sets the test outcome, including its children (unless they were skipped
        /// or are otherwise irrelevant to the outcome of their parent).
        /// </summary>
        /// <value>
        /// Defaults to <see cref="TestOutcome.Inconclusive" />.
        /// </value>
        [XmlElement("outcome")]
        public TestOutcome Outcome
        {
            get { return outcome; }
            set { outcome = value; }
        }

        /// <summary>
        /// Gets or sets the number of assertions evaluated by the test, including its children.
        /// </summary>
        [XmlAttribute("assertCount")]
        public int AssertCount
        {
            get { return assertCount; }
            set { assertCount = value; }
        }

        /// <summary>
        /// Gets or sets the test duration in seconds, including its children.
        /// </summary>
        [XmlAttribute("duration")]
        public double Duration
        {
            get { return duration; }
            set { duration = value; }
        }
    }
}