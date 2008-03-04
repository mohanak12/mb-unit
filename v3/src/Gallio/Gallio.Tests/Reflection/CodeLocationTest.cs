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
using Gallio.Reflection;
using MbUnit.Framework.Xml;
using MbUnit.Framework;

namespace Gallio.Tests.Reflection
{
    [TestFixture]
    [TestsOn(typeof(CodeLocation))]
    public class CodeLocationTest
    {
        [Test]
        [Row("file", 1, 1)]
        [Row("file", 1, 0)]
        [Row("file", 0, 1)]
        [Row("file", 0, 0)]
        [Row("file", -1, 1, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row("file", 1, -1, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(null, 1, 1, ExpectedException = typeof(ArgumentNullException))]
        public void Constructor(string filename, int line, int column)
        {
            CodeLocation location = new CodeLocation(filename, line, column);
            Assert.AreEqual(filename, location.Path);
            Assert.AreEqual(line, location.Line);
            Assert.AreEqual(column, location.Column);
        }

        [Test]
        [Row("file", 1, 1)]
        [Row("file", 1, 0)]
        [Row("file", 0, 1)]
        [Row("file", 0, 0)]
        [Row("file", -1, 1, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row("file", 1, -1, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(null, 1, 1, ExpectedException = typeof(ArgumentNullException))]
        public void Setters(string filename, int line, int column)
        {
            CodeLocation location = new CodeLocation("", 0, 0);

            location.Path = filename;
            location.Line = line;
            location.Column = column;

            Assert.AreEqual(filename, location.Path);
            Assert.AreEqual(line, location.Line);
            Assert.AreEqual(column, location.Column);
        }

        [Test]
        public void TypeIsXmlSerializable()
        {
            XmlSerializationAssert.IsXmlSerializable(typeof(CodeLocation));
        }

        [Test]
        new public void ToString()
        {
            Assert.AreEqual("file", new CodeLocation("file", 0, 0).ToString());
            Assert.AreEqual("file(11)", new CodeLocation("file", 11, 0).ToString());
            Assert.AreEqual("file(11,33)", new CodeLocation("file", 11, 33).ToString());
        }
    }
}
