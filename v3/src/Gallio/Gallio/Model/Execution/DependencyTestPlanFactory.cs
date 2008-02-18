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
using Gallio.Model.Execution;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// Creates instances of <see cref="DependencyTestPlan" />.
    /// </summary>
    public class DependencyTestPlanFactory : ITestPlanFactory
    {
        private readonly ITestContextTracker contextTracker;

        /// <summary>
        /// Initializes a test plan factory.
        /// </summary>
        /// <param name="contextTracker">The context manager</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contextTracker"/> is null</exception>
        public DependencyTestPlanFactory(ITestContextTracker contextTracker)
        {
            if (contextTracker == null)
                throw new ArgumentNullException("contextTracker");

            this.contextTracker = contextTracker;
        }

        /// <inheritdoc />
        public ITestPlan CreateTestPlan(ITestListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(@"listener");

            return new DependencyTestPlan(new ObservableTestContextManager(contextTracker, listener));
        }
    }
}
