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
using System.Reflection;
using System.Text;
using Gallio.Collections;
using Gallio.Framework.Pattern;
using Gallio.Model;
using Gallio.Tests.Model;
using MbUnit.Framework;
using MbUnit.TestResources;

namespace Gallio.Tests.Framework.Pattern
{
    [TestFixture]
    [TestsOn(typeof(PatternTestFramework))]
    [Author("Jeff", "jeff@ingenio.com")]
    public class PatternTestFrameworkTest : BaseTestFrameworkTest
    {
        protected override Assembly GetSampleAssembly()
        {
            return typeof(SimpleTest).Assembly;
        }

        protected override ITestFramework CreateFramework()
        {
            return new PatternTestFramework(EmptyArray<IPatternTestFrameworkExtension>.Instance);
        }

        [Test]
        public void NameIsPattern()
        {
            Assert.AreEqual("Gallio Pattern Test Framework", framework.Name);
        }

        [Test]
        public void PopulateTestTree_WhenAssemblyDoesNotReferenceFramework_IsEmpty()
        {
            sampleAssembly = typeof(Int32).Assembly;
            PopulateTestTree();

            Assert.AreEqual(0, testModel.RootTest.Children.Count);
        }
    }
}
