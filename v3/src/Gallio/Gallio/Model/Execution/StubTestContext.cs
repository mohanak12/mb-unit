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
using Gallio.Common.Collections;
using Gallio.Model;
using Gallio.Common.Markup;
using Gallio.Common.Reflection;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// A stub implementation of <see cref="ITestContext" /> using a <see cref="TextualMarkupDocumentWriter" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Does not fully support nested test steps or other dynamic features.
    /// </para>
    /// </remarks>
    /// <seealso cref="StubTestContextTracker" />
    /// <seealso cref="TextualMarkupDocumentWriter"/>
    public class StubTestContext : ITestContext
    {
        private readonly UserDataCollection data;
        private readonly ITestStep testStep;
        private readonly TextualMarkupDocumentWriter logWriter;

        /// <summary>
        /// Creates a stub context.
        /// </summary>
        public StubTestContext()
        {
            data = new UserDataCollection();
            testStep = new TestStepInfo(new BaseTestStep(new RootTest(), null));
            logWriter = new TextualMarkupDocumentWriter(Console.Out, false);
        }

        /// <inheritdoc />
        public ITestContext Parent
        {
            get { return null; }
        }

        /// <inheritdoc />
        public ITestStep TestStep
        {
            get { return testStep; }
        }

        /// <inheritdoc />
        public MarkupDocumentWriter LogWriter
        {
            get { return logWriter; }
        }

        /// <inheritdoc />
        public string LifecyclePhase
        {
            get { return LifecyclePhases.Execute; }
            set { }
        }

        /// <inheritdoc />
        public TestOutcome Outcome
        {
            get { return TestOutcome.Passed; }
        }

        /// <inheritdoc />
        public UserDataCollection Data
        {
            get { return data; }
        }

        /// <inheritdoc />
        public int AssertCount
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public bool IsFinished
        {
            get { return false; }
        }

        /// <inheritdoc />
        public event EventHandler Finishing
        {
            add { }
            remove { }
        }

        /// <inheritdoc />
        public void AddAssertCount(int value)
        {
        }

        /// <inheritdoc />
        public ITestContext StartChildStep(ITestStep childStep)
        {
            return this;
        }

        /// <inheritdoc />
        public void FinishStep(TestOutcome outcome, TimeSpan? actualDuration)
        {
        }

        /// <inheritdoc />
        public void AddMetadata(string metadataKey, string metadataValue)
        {
        }

        /// <inheritdoc />
        public void SetInterimOutcome(TestOutcome outcome)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}