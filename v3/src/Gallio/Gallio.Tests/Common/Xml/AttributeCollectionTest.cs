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

using Gallio.Model.Schema;
using Gallio.Common.Reflection;
using Gallio.Runner.Reports.Schema;
using Gallio.Tests;
using MbUnit.Framework;
using Gallio.Common.Xml;
using Gallio.Common;
using System.Collections.Generic;
using System;

namespace Gallio.Tests.Common.Xml
{
    [TestFixture]
    [TestsOn(typeof(AttributeCollection))]
    public class AttributeCollectionTest : DiffableTestBase
    {
        [Test]
        [ExpectedArgumentNullException]
        public void Constructs_with_null_initializer_should_throw_exception()
        {
            new AttributeCollection(null);
        }

        [Test]
        public void Default_empty()
        {
            var collection = AttributeCollection.Empty;
            Assert.IsEmpty(collection);
            Assert.IsEmpty(collection.ToXml());
        }

        [Test]
        public void Constructs_ok()
        {
            var attribute1 = new Gallio.Common.Xml.Attribute("name1", "value1");
            var attribute2 = new Gallio.Common.Xml.Attribute("name2", "value2");
            var attribute3 = new Gallio.Common.Xml.Attribute("name3", "value3");
            var array = new[] { attribute1, attribute2, attribute3 };
            var collection = new AttributeCollection(array);
            Assert.AreElementsEqual(array, collection,
                new StructuralEqualityComparer<Gallio.Common.Xml.Attribute> 
                { 
                    { x => x.Name }, 
                    { x => x.Value } 
                });
            Assert.AreEqual(" name1=\"value1\" name2=\"value2\" name3=\"value3\"", collection.ToXml());
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Diff_with_null_expected_value_should_throw_exception()
        {
            var collection = AttributeCollection.Empty;
            collection.Diff(null, XmlPath.Empty, XmlOptions.Strict.Value);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Diff_with_null_path_should_throw_exception()
        {
            var collection = AttributeCollection.Empty;
            collection.Diff(AttributeCollection.Empty, null, XmlOptions.Strict.Value);
        }

        private AttributeCollection MakeCollection(params string[] namesValues)
        {
            var list = new List<Gallio.Common.Xml.Attribute>();

            for (int i = 0; i < namesValues.Length / 2; i++)
            {
                list.Add(new Gallio.Common.Xml.Attribute(namesValues[2 * i], namesValues[2 * i + 1]));
            }

            return new AttributeCollection(list);
        }

        [Test]
        public void Contains_with_null_name_should_throw_exception()
        {
            var attributes = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            Assert.Throws<ArgumentNullException>(() => attributes.Contains(null));
        }

        [Test]
        public void Contains_yes()
        {
            var attributes = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            bool found = attributes.Contains("name2");
            Assert.IsTrue(found);
        }

        [Test]
        public void Contains_no()
        {
            var attributes = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            bool found = attributes.Contains("name123");
            Assert.IsFalse(found);
        }

        #region Diffing ordered attributes
        
        [Test]
        public void Diff_equal_collections()
        {
            var actual = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            var expected = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            Assert.IsEmpty(diffSet);
        }

        [Test]
        public void Diff_collections_with_missing_attribute_at_the_end()
        {
            var actual = MakeCollection("name1", "value1", "name2", "value2");
            var expected = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Missing attribute.", "name3", String.Empty) });
        }

        [Test]
        public void Diff_collections_with_missing_attribute_in_the_middle()
        {
            var actual = MakeCollection("name1", "value1", "name3", "value3");
            var expected = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Unexpected attribute found.", "name2", "name3") });
        }

        [Test]
        public void Diff_collections_with_exceeding_attribute_at_the_end()
        {
            var actual = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            var expected = MakeCollection("name1", "value1", "name2", "value2");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Unexpected attribute found.", String.Empty, "name3") });
        }

        [Test]
        public void Diff_collections_with_exceeding_attribute_in_the_middle()
        {
            var actual = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            var expected = MakeCollection("name1", "value1", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Unexpected attribute found.", "name3", "name2") });
        }

        [Test]
        public void Diff_collections_with_one_unexpected_value()
        {
            var actual = MakeCollection("name1", "value1", "name2", "ERROR!", "name3", "value3");
            var expected = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { new Diff("<Root name2='...'>", "Unexpected attribute value found.", "value2", "ERROR!") });
        }

        [Test]
        public void Diff_collections_with_several_unexpected_values()
        {
            var actual = MakeCollection("name1", "ERROR1!", "name2", "value2", "name3", "ERROR3!");
            var expected = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), XmlOptions.Strict.Value);
            AssertDiff(diffSet, new[] { 
                new Diff("<Root name1='...'>", "Unexpected attribute value found.", "value1", "ERROR1!"),
                new Diff("<Root name3='...'>", "Unexpected attribute value found.", "value3", "ERROR3!") });
        }

        #endregion

        #region Diffing unordered attributes

        [Test]
        public void Diff_equal_unordered_collections()
        {
            var actual = MakeCollection("name1", "value1", "name2", "value2", "name3", "value3");
            var expected = MakeCollection("name2", "value2", "name3", "value3", "name1", "value1");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), Options.IgnoreAttributesOrder);
            Assert.IsEmpty(diffSet);
        }

        [Test]
        public void Diff_equal_unordered_collections_with_missing_attribute()
        {
            var actual = MakeCollection("name1", "value1", "name3", "value3");
            var expected = MakeCollection("name2", "value2", "name3", "value3", "name1", "value1");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), Options.IgnoreAttributesOrder);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Missing attribute.", "name2", String.Empty) });
        }

        [Test]
        public void Diff_equal_unordered_collections_with_excess_attribute()
        {
            var actual = MakeCollection("name1", "value1", "name3", "value3", "name2", "value2");
            var expected = MakeCollection("name2", "value2", "name1", "value1");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), Options.IgnoreAttributesOrder);
            AssertDiff(diffSet, new[] { new Diff("<Root>", "Unexpected attribute found.", String.Empty, "name3") });
        }

        [Test]
        public void Diff_equal_unordered_collections_with_unexpected_attribute_value()
        {
            var actual = MakeCollection("name1", "value1", "name3", "ERROR!", "name2", "value2");
            var expected = MakeCollection("name2", "value2", "name1", "value1", "name3", "value3");
            DiffSet diffSet = actual.Diff(expected, XmlPath.Element("Root"), Options.IgnoreAttributesOrder);
            AssertDiff(diffSet, new[] { new Diff("<Root name3='...'>", "Unexpected attribute value found.", "value3", "ERROR!") });
        }

        #endregion
    }
}
