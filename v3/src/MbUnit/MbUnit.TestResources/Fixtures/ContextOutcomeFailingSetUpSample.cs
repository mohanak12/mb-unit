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
using Gallio.Framework;
using MbUnit.Framework;
using Gallio.Logging;

namespace MbUnit.TestResources.Fixtures
{
    /// <summary>
    /// A sample test fixture with failing setup.
    /// </summary>
    [TestFixture]
    public class ContextOutcomeFailingSetUpSample
    {
        private Context previousContext;

        [SetUp]
        public void SetUp()
        {
            previousContext = Context.CurrentContext;
            Log.WriteLine(Context.CurrentContext.Outcome);
            Assert.Fail("Boom");
        }

        [Test]
        public void Test()
        {
            Assert.Fail("Should not get here because the setup failed.");
        }

        [TearDown]
        public void TearDown()
        {
            Log.WriteLine(Context.CurrentContext.Outcome);
        }

        [FixtureTearDown]
        public void TestFixtureTearDown()
        {
            Log.WriteLine(previousContext.Outcome);
        }
    }
}
