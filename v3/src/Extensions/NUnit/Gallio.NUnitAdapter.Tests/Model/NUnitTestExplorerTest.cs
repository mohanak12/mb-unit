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

using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.NUnitAdapter.TestResources;
using Gallio.NUnitAdapter.TestResources.Metadata;
using Gallio.Runtime;
using Gallio.Runtime.Extensibility;
using Gallio.Tests.Model;
using Gallio.NUnitAdapter.Model;
using MbUnit.Framework;

namespace Gallio.NUnitAdapter.Tests.Model
{
    [TestFixture]
    [TestsOn(typeof(NUnitTestExplorer))]
    [Author("Julian", "julian.hidalgo@gallio.org")]
    public class NUnitTestExplorerTest : BaseTestExplorerTest<SimpleTest>
    {
        protected override ComponentHandle<ITestFramework, TestFrameworkTraits> GetFrameworkHandle()
        {
            return (ComponentHandle<ITestFramework, TestFrameworkTraits>)
#if NUNIT248
                RuntimeAccessor.ServiceLocator.ResolveHandleByComponentId("NUnitAdapter248.TestFramework");
#elif NUNIT25
                RuntimeAccessor.ServiceLocator.ResolveHandleByComponentId("NUnitAdapter25.TestFramework");
#else
#error "Unrecognized NUnit framework version."
#endif
        }

        protected override string FrameworkKind
        {
            get { return NUnitTestExplorer.FrameworkKind; }
        }
    }
}
