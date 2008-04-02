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
using System.Text;
using System.Xml.Serialization;
using Gallio.Utilities;

namespace Gallio.Runner.Reports
{
    /// <summary>
    /// An Xml-serializable container for a section with
    /// an identifying section name.  This tag is used to delineate
    /// sections within an execution log stream.
    /// </summary>
    [Serializable]
    [XmlType(Namespace = XmlSerializationUtils.GallioNamespace)]
    public sealed class ExecutionLogStreamSectionTag : ExecutionLogStreamContainerTag
    {
        private string name;

        /// <summary>
        /// Creates an uninitialized instance for Xml deserialization.
        /// </summary>
        private ExecutionLogStreamSectionTag()
        {
        }

        /// <summary>
        /// Creates an initialized tag.
        /// </summary>
        /// <param name="name">The section name</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null</exception>
        public ExecutionLogStreamSectionTag(string name)
        {
            if (name == null)
                throw new ArgumentNullException(@"name");
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the section name, not null.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                name = value;
            }
        }

        /// <inheritdoc />
        public override void Accept(IExecutionLogStreamTagVisitor visitor)
        {
            visitor.VisitSectionTag(this);
        }
    }
}
