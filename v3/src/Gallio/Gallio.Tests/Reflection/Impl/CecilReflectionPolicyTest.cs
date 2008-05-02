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


using Gallio.Reflection;
using Gallio.Reflection.Impl;
using MbUnit.Framework;

namespace Gallio.Tests.Reflection.Impl
{
    [TestFixture]
    [TestsOn(typeof(CecilReflectionPolicy))]
    [Pending("There are known bugs in the underlying implementation some of which are due to missing features in Cecil.")]
    public class CecilReflectionPolicyTest : BaseReflectionPolicyTest
    {
        private CecilReflectionPolicy policy;

        public override void SetUp()
        {
            base.SetUp();
            WrapperAssert.SupportsSpecialFeatures = false;

            policy = new CecilReflectionPolicy();
        }

        protected override IReflectionPolicy ReflectionPolicy
        {
            get { return policy; }
        }
    }
}
