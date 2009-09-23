﻿// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
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
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Gallio.Common.Xml
{
    /// <summary>
    /// An immutable collection of XML attributes.
    /// </summary>
    public class AttributeCollection : IDiffableCollection<AttributeCollection, Attribute>
    {
        private readonly List<Attribute> attributes;

        /// <summary>
        /// An empty collection singleton instance.
        /// </summary>
        public readonly static AttributeCollection Empty = new AttributeCollection();

        private AttributeCollection()
        {
            this.attributes = new List<Attribute>();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return attributes.Count;
            }
        }

        /// <inheritdoc />
        public Attribute this[int index]
        {
            get
            {
                return attributes[index];
            }
        }

        /// <summary>
        /// Constructs a collection of XML attributes from the specified enumeration.
        /// </summary>
        /// <param name="attributes">An enumeration of attributes.</param>
        public AttributeCollection(IEnumerable<Attribute> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            this.attributes = new List<Attribute>(attributes);
        }

        /// <summary>
        /// Returns the XML fragment for the attributes.
        /// </summary>
        /// <returns>The resulting XML fragment representing the attributes.</returns>
        public string ToXml()
        {
            var builder = new StringBuilder();

            foreach (var attribute in attributes)
            {
                builder.Append(" " + attribute.ToXml());
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public IEnumerator<Attribute> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var attribute in attributes)
            {
                yield return attribute;
            }
        }

        /// <inheritdoc />
        public DiffSet Diff(AttributeCollection expected, IXmlPathOpen path, Options options)
        {
            return DiffEngineFactory.ForAttributes(expected, this, path, options).Diff();
        }

        /// <inheritdoc />
        public int FindIndex(Predicate<int> predicate)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (predicate(i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Determines whether the collection contains an attribute with the specified name.
        /// </summary>
        /// <param name="searchedAttributeName">The name of the searched attribute.</param>
        /// <param name="options">Options for the search.</param>
        /// <returns>True is such an attribute exists; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="searchedAttributeName"/> is null.</exception>
        public bool Contains(string searchedAttributeName, Options options)
        {
            if (searchedAttributeName == null)
                throw new ArgumentNullException("searchedAttributeName");

            return attributes.Exists(attribute => attribute.AreNamesEqual(searchedAttributeName, options));
        }
    }
}
