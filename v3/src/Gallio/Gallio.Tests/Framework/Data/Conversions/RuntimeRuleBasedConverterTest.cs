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
using System.Collections.Generic;
using System.Text;
using Gallio.Framework.Data.Conversions;
using Gallio.Hosting;
using MbUnit.Framework;
using Rhino.Mocks;

namespace Gallio.Tests.Framework.Data.Conversions
{
    [TestFixture]
    [TestsOn(typeof(RuntimeRuleBasedConverter))]
    public class RuntimeRuleBasedConverterTest : BaseUnitTest
    {
        [Test]
        public void PopulatesRulesFromTheRuntime()
        {
            IRuntime runtime = Mocks.CreateMock<IRuntime>();

            using (Mocks.Record())
            {
                SetupResult.For(runtime.ResolveAll<IConversionRule>()).Return(new IConversionRule[]
                {
                    new ConvertibleToConvertibleConversionRule()
                });
            }

            using (Mocks.Playback())
            {
                IConverter converter = new RuntimeRuleBasedConverter(runtime);
                Assert.IsTrue(converter.CanConvert(typeof(int), typeof(string)));
            }
        }
    }
}
