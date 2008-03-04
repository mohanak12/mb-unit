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
using System.Threading;

namespace Gallio.Model
{
    /// <summary>
    /// A read-only implementation of <see cref="ITestStep" /> for reflection.
    /// </summary>
    public sealed class TestStepInfo : TestComponentInfo, ITestStep
    {
        private TestStepInfo cachedParent;
        private TestInstanceInfo cachedTestInstance;

        /// <summary>
        /// Creates a read-only wrapper of the specified test step.
        /// </summary>
        /// <param name="source">The source test step</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        public TestStepInfo(ITestStep source)
            : base(source)
        {
        }

        /// <inheritdoc />
        public string FullName
        {
            get { return Source.FullName; }
        }

        /// <inheritdoc />
        public TestStepInfo Parent
        {
            get
            {
                if (cachedParent == null && Source.Parent != null)
                    Interlocked.CompareExchange(ref cachedParent, new TestStepInfo(Source.Parent), null);
                return cachedParent;
            }
        }
        ITestStep ITestStep.Parent
        {
            get { return Parent; }
        }

        /// <inheritdoc />
        public TestInstanceInfo TestInstance
        {
            get
            {
                if (cachedTestInstance == null)
                    Interlocked.CompareExchange(ref cachedTestInstance, new TestInstanceInfo(Source.TestInstance), null);
                return cachedTestInstance;
            }
        }
        ITestInstance ITestStep.TestInstance
        {
            get { return TestInstance; }
        }

        /// <inheritdoc />
        new internal ITestStep Source
        {
            get { return (ITestStep)base.Source; }
        }
    }
}
