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
using System.Reflection;
using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.MSTestAdapter.Model;
using Gallio.MSTestAdapter.TestResources;
using Gallio.MSTestAdapter.TestResources.Metadata;
using Gallio.Runtime;
using Gallio.Runtime.Extensibility;
using Gallio.Tests.Model;
using MbUnit.Framework;

namespace Gallio.MSTestAdapter.Tests.Model
{
    [TestFixture]
    [TestsOn(typeof(MSTestFramework))]
    [Author("Julian", "julian.hidalgo@gallio.org")]
    public class MSTestFrameworkTest : BaseTestFrameworkTest
    {
        protected override Assembly GetSampleAssembly()
        {
            return typeof(SimpleTest).Assembly;
        }

        protected override ComponentHandle<ITestFramework, TestFrameworkTraits> GetFrameworkHandle()
        {
            return (ComponentHandle<ITestFramework, TestFrameworkTraits>)
                RuntimeAccessor.ServiceLocator.ResolveHandleByComponentId("MSTestAdapter.TestFramework");
        }
    }
}
