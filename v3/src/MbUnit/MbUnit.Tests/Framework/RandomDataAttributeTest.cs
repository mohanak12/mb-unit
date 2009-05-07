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

using System;
using System.Transactions;
using Gallio.Framework;
using Gallio.Reflection;
using Gallio.Runner.Reports;
using Gallio.Tests;
using MbUnit.Framework;
using System.Linq;
using Gallio.Model.Logging;
using System.Text.RegularExpressions;

namespace MbUnit.Tests.Framework
{
    [TestsOn(typeof(RandomDataAttribute))]
    [RunSample(typeof(RandomDataSample))]
    public class RandomDataAttributeTest : BaseTestWithSampleRunner
    {
        [Test]
        [Row("Single", 100, -10, 10)]
        public void EnumData(string testMethod, int expectedRepeat, double expectedMinimum, double expectedMaximum)
        {
            var run = Runner.GetPrimaryTestStepRun(CodeReference.CreateFromMember(typeof(RandomDataSample).GetMethod(testMethod)));
            string[] lines = run.Children.Select(x => x.TestLog.GetStream(TestLogStreamNames.Default).ToString()).ToArray();
            Assert.AreEqual(expectedRepeat, lines.Length);

            foreach(string line in lines)
            {
                var match = Regex.Match(line, @"\[(?<value>-?(\d*\.)?\d+)\]");
                Assert.IsTrue(match.Success);
                double value = Double.Parse(match.Groups["value"].Value);
                Assert.Between(value, expectedMinimum, expectedMaximum);
            }
        }

        [TestFixture, Explicit("Sample")]
        public class RandomDataSample
        {
            [Test]
            public void Single([RandomData(100, -10, 10)] double value)
            {
                TestLog.WriteLine("[{0}]", value);
            }
        }
    }
}
