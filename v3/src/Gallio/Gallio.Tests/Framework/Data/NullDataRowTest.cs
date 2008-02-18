// Copyright 2008 MbUnit Project - http://www.mbunit.com/
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

using System.Collections.Generic;
using Gallio.Framework.Data;
using MbUnit.Framework;

namespace Gallio.Tests.Framework.Data
{
    [TestFixture]
    [TestsOn(typeof(NullDataRow))]
    [DependsOn(typeof(BaseDataRowTest))]
    public class NullDataRowTest
    {
        [Test]
        public void HasNoMetadata()
        {
            List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>(NullDataRow.Instance.GetMetadata());
            Assert.AreEqual(0, metadata.Count);
        }

        [Test]
        public void GetValueReturnsDefaultValueForType()
        {
            Assert.AreEqual(0, NullDataRow.Instance.GetValue(new SimpleDataBinding(typeof(int))));
            Assert.AreEqual(0.0, NullDataRow.Instance.GetValue(new SimpleDataBinding(typeof(double))));
            Assert.AreEqual(null, NullDataRow.Instance.GetValue(new SimpleDataBinding(typeof(object))));
        }
    }
}