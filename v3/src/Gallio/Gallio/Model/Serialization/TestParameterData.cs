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

namespace Gallio.Model.Serialization
{
    /// <summary>
    /// Describes a test parameter in a portable manner for serialization.
    /// </summary>
    /// <seealso cref="ITestParameter"/>
    [Serializable]
    [XmlType(Namespace=SerializationUtils.XmlNamespace)]
    public sealed class TestParameterData : TestComponentData
    {
        private string typeName;
        private int index;

        /// <summary>
        /// Creates an uninitialized instance for Xml deserialization.
        /// </summary>
        private TestParameterData()
        {
        }

        /// <summary>
        /// Creates a parameter data object.
        /// </summary>
        /// <param name="id">The component id</param>
        /// <param name="name">The component name</param>
        /// <param name="typeName">The parameter type name</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/>.
        /// <paramref name="name"/> or <paramref name="typeName"/> is null</exception>
        public TestParameterData(string id, string name, string typeName)
            : base(id, name)
        {
            if (typeName == null)
                throw new ArgumentNullException(@"typeName");

            this.typeName = typeName;
        }

        /// <summary>
        /// Copies the contents of a test parameter.
        /// </summary>
        /// <param name="source">The source test parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        public TestParameterData(ITestParameter source)
            : base(source)
        {
            typeName = source.Type.ToString();
            index = source.Index;
        }

        /// <summary>
        /// Gets or sets the fully-qualified type name of the parameter's value type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <seealso cref="ITestParameter.Type"/>
        [XmlAttribute("type")]
        public string TypeName
        {
            get { return typeName; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                typeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the parameter.
        /// </summary>
        /// <seealso cref="ITestParameter.Index"/>
        [XmlAttribute("index")]
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }
}