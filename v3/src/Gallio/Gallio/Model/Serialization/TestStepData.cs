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
using System.Xml.Serialization;
using Gallio.Model;
using Gallio.Model.Serialization;
using Gallio.Utilities;

namespace Gallio.Model.Serialization
{
    /// <summary>
    /// Describes a test step in a portable manner for serialization.
    /// </summary>
    /// <seealso cref="ITestStep"/>
    [Serializable]
    [XmlRoot("testStep", Namespace = XmlSerializationUtils.GallioNamespace)]
    [XmlType(Namespace = XmlSerializationUtils.GallioNamespace)]
    public sealed class TestStepData : TestComponentData
    {
        private string fullName;
        private string parentId;
        private string testId;
        private bool isPrimary;
        private bool isTestCase;
        private bool isDynamic;

        /// <summary>
        /// Creates an uninitialized instance for Xml deserialization.
        /// </summary>
        private TestStepData()
        {
        }

        /// <summary>
        /// Creates a step.
        /// </summary>
        /// <param name="id">The step id</param>
        /// <param name="name">The step name</param>
        /// <param name="fullName">The full name of the step</param>
        /// <param name="testId">The test id</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/>, <paramref name="name"/>,
        /// <paramref name="fullName"/> or <paramref name="testId"/> is null</exception>
        public TestStepData(string id, string name, string fullName, string testId)
            : base(id, name)
        {
            if (fullName == null)
                throw new ArgumentNullException(@"fullName");
            if (testId == null)
                throw new ArgumentNullException("testId");

            this.fullName = fullName;
            this.testId = testId;
        }

        /// <summary>
        /// Copies the contents of a test step.
        /// </summary>
        /// <param name="source">The source test step</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        public TestStepData(ITestStep source)
            : base(source)
        {
            fullName = source.FullName;
            testId = source.Test.Id;
            isPrimary = source.IsPrimary;
            isTestCase = source.IsTestCase;

            if (source.Parent != null)
                parentId = source.Parent.Id;
        }

        /// <summary>
        /// Gets or sets the full name of the step.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlAttribute("fullName")]
        public string FullName
        {
            get { return fullName; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                fullName = value;
            }
        }

        /// <summary>
        /// Gets or sets the id of the parent step.
        /// </summary>
        [XmlAttribute("parentId")]
        public string ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        /// <summary>
        /// Gets or sets the id of the test to which the step belongs.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlAttribute("testId")]
        public string TestId
        {
            get { return testId; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                testId = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the test step is primary.
        /// </summary>
        /// <seealso cref="ITestStep.IsPrimary"/>
        [XmlAttribute("isPrimary")]
        public bool IsPrimary
        {
            get { return isPrimary; }
            set { isPrimary = value; }
        }

        /// <summary>
        /// Gets or sets whether the test step represents a test case.
        /// </summary>
        /// <seealso cref="ITestStep.IsTestCase"/>
        [XmlAttribute("isTestCase")]
        public bool IsTestCase
        {
            get { return isTestCase; }
            set { isTestCase = value; }
        }

        /// <summary>
        /// Gets or sets whether the test step is dynamic.
        /// </summary>
        /// <seealso cref="ITestStep.IsDynamic"/>
        [XmlAttribute("isDynamic")]
        public bool IsDynamic
        {
            get { return isDynamic; }
            set { isDynamic = value; }
        }
    }
}