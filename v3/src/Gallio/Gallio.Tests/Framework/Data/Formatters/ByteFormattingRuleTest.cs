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

using System;
using Gallio.Framework.Data.Formatters;
using MbUnit.Framework;

namespace Gallio.Tests.Framework.Data.Formatters
{
    [TestFixture]
    [TestsOn(typeof(ByteFormattingRule))]
    public class ByteFormattingRuleTest : BaseFormattingRuleTest<ByteFormattingRule>
    {
        [Test]
        [Row(typeof(byte), 0x00, "0x00")]
        [Row(typeof(byte), 0x05, "0x05")]
        [Row(typeof(byte), 0xa5, "0xa5")]
        [Row(typeof(sbyte), 0x00, "0x00")]
        [Row(typeof(sbyte), 0x05, "0x05")]
        [Row(typeof(sbyte), 0xa5, "0xa5")]
        public void Format<T>(T value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, Formatter.Format(value));
        }

        [Test]
        [Row(typeof(byte), FormattingRulePriority.Best)]
        [Row(typeof(sbyte), FormattingRulePriority.Best)]
        [Row(typeof(string), null)]
        public void GetPriority(Type type, int? expectedPriority)
        {
            Assert.AreEqual(expectedPriority, FormattingRule.GetPriority(type));
        }
    }
}