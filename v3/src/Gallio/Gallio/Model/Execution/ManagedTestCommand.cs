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
using Gallio.Collections;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// A <see cref="ITestCommand"/> implementation based on a <see cref="ITestContextManager"/>.
    /// </summary>
    public class ManagedTestCommand : ITestCommand
    {
        private readonly ITestContextManager contextManager;
        private readonly ITest test;
        private readonly bool isExplicit;
        private readonly List<ITestCommand> children;

        /// <summary>
        /// Creates a test command.
        /// </summary>
        /// <param name="contextManager">The test context manager</param>
        /// <param name="test">The test</param>
        /// <param name="isExplicit">True if the test is being executed explicitly</param>
        public ManagedTestCommand(ITestContextManager contextManager, ITest test, bool isExplicit)
        {
            if (contextManager == null)
                throw new ArgumentNullException("contextManager");
            if (test == null)
                throw new ArgumentNullException("test");

            this.contextManager = contextManager;
            this.test = test;
            this.isExplicit = isExplicit;

            children = new List<ITestCommand>();
        }

        /// <inheritdoc />
        public ITest Test
        {
            get { return test; }
        }

        /// <inheritdoc />
        public bool IsExplicit
        {
            get { return isExplicit; }
        }

        /// <inheritdoc />
        public int TestCount
        {
            get
            {
                int count = 0;
                foreach (ITestCommand monitor in PreOrderTraversal)
                    count += 1;
                return count;
            }
        }

        /// <inheritdoc />
        public IEnumerable<ITestCommand> Children
        {
            get { return children; }
        }

        /// <inheritdoc />
        public IEnumerable<ITestCommand> PreOrderTraversal
        {
            get
            {
                return TreeUtils.GetPreOrderTraversal<ITestCommand>(this, GetChildren);
            }
        }

        private static IEnumerable<ITestCommand> GetChildren(ITestCommand node)
        {
            return node.Children;
        }

        /// <inheritdoc />
        public IList<ITestCommand> GetAllCommands()
        {
            return new List<ITestCommand>(PreOrderTraversal);
        }

        /// <inheritdoc />
        public ITestContext StartRootStep(ITestStep rootStep)
        {
            if (rootStep == null)
                throw new ArgumentNullException("rootStep");
            if (rootStep.Parent != null || rootStep.TestInstance.Test != test)
                throw new ArgumentException("Expected the root step of an instance of this test.", "rootStep");

            return contextManager.StartStep(rootStep);
        }

        /// <inheritdoc />
        public ITestContext StartRootStep(ITestInstance parentTestInstance)
        {
            return StartRootStep(new BaseTestStep(new BaseTestInstance(test, parentTestInstance)));
        }

        /// <summary>
        /// Adds a child test command.
        /// </summary>
        /// <param name="child">The child to add</param>
        public void AddChild(ManagedTestCommand child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            children.Add(child);
        }
    }
}
