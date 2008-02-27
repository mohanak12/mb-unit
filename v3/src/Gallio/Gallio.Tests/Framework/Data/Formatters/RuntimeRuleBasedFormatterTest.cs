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
using Gallio.Hosting;
using MbUnit.Framework;
using Rhino.Mocks;

namespace Gallio.Tests.Framework.Data.Formatters
{
    [TestFixture]
    [TestsOn(typeof(RuntimeRuleBasedFormatter))]
    public class RuntimeRuleBasedFormatterTest : BaseUnitTest
    {
        [Test]
        public void PopulatesRulesFromTheRuntime()
        {
            IRuntime runtime = Mocks.CreateMock<IRuntime>();

            using (Mocks.Record())
            {
                SetupResult.For(runtime.ResolveAll<IFormattingRule>()).Return(new IFormattingRule[]
                {
                    new StringFormattingRule()
                });
            }

            using (Mocks.Playback())
            {
                IFormatter formatter = new RuntimeRuleBasedFormatter(runtime);
                Assert.AreEqual("\"abc\"", formatter.Format("abc"));
            }
        }
    }
}
