// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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
using Gallio.Model.Schema;
using Gallio.Common.Reflection;
using Gallio.Runner.Reports.Schema;
using Gallio.Tests;
using MbUnit.Framework;
using Gallio.Common.Xml;
using System.Collections.Generic;

namespace Gallio.Tests.Common.Xml
{
    [TestFixture]
    [TestsOn(typeof(XmlPathClosed))]
    public class XmlPathTest
    {
        [Test]
        [ExpectedArgumentNullException]
        public void Element_with_null_name_should_throw_exception()
        {
            XmlPathRoot.Element(null);
        }

        [Test]
        [ExpectedArgumentException]
        public void Element_with_empty_name_should_throw_exception()
        {
            XmlPathRoot.Element(String.Empty);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Attribute_with_null_name_should_throw_exception()
        {
            XmlPathRoot.Element("Element").Attribute(null);
        }

        [Test]
        [ExpectedArgumentException]
        public void Attribute_with_empty_name_should_throw_exception()
        {
            XmlPathRoot.Element("Element").Attribute(String.Empty);
        }

        [Test]
        public void Element_without_attribute()
        {
            var path = (XmlPathClosed)XmlPathRoot.Element("Ancestor").Element("Parent").Element("Child");
            Assert.AreElementsEqual(new[] { "Ancestor", "Parent", "Child" }, path.ElementNames);
            Assert.IsNull(path.AttributeName);
            Assert.AreEqual("<Ancestor><Parent><Child>", path.ToString());
        }

        [Test]
        public void Element_with_attribute()
        {
            var path = (XmlPathClosed)XmlPathRoot.Element("Ancestor").Element("Parent").Element("Child").Attribute("value");
            Assert.AreElementsEqual(new[] { "Ancestor", "Parent", "Child" }, path.ElementNames);
            Assert.AreEqual("value", path.AttributeName);
            Assert.AreEqual("<Ancestor><Parent><Child value='...'>", path.ToString());
        }

        [Test]
        public void Trail_empty_path()
        {
            var trail = ((XmlPathClosed)XmlPathRoot.Empty).Trail();
            Assert.IsEmpty(trail.ToString());
        }

        [Test]
        public void Trail_path_with_one_element()
        {
            var path = (XmlPathClosed)XmlPathRoot.Element("Root");
            var trail = path.Trail();
            Assert.AreEqual("<Root>", path.ToString());
            Assert.IsEmpty(trail.ToString());
        }

        [Test]
        public void Trail_deep_path()
        {
            var path = (XmlPathClosed)XmlPathRoot.Element("Ancestor").Element("Parent").Element("Child");
            var trail = path.Trail();
            Assert.AreEqual("<Ancestor><Parent><Child>", path.ToString());
            Assert.AreEqual("<Parent><Child>", trail.ToString());
        }

        [Test]
        public void Trail_deep_path_with_attribute()
        {
            var path = (XmlPathClosed)XmlPathRoot.Element("Ancestor").Element("Parent").Element("Child").Attribute("value");
            var trail = path.Trail();
            Assert.AreEqual("<Ancestor><Parent><Child value='...'>", path.ToString());
            Assert.AreEqual("<Parent><Child value='...'>", trail.ToString());
        }
    }
}
