// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using System.Collections.Generic;
using System.Threading;
using Gallio;
using Gallio.Collections;
using Gallio.Framework;
using Gallio.Framework.Utilities;

namespace MbUnit.Framework
{
    /// <summary>
    /// Describes the context in which an assertion is being evaluated.
    /// </summary>
    public sealed class AssertionContext
    {
        private static readonly Key<AssertionContext> AssertionContextKey = new Key<AssertionContext>("MbUnit.Framework.AssertionContext");

        private readonly TestContext testContext;
        private Scope scope;

        private AssertionContext(TestContext testContext)
        {
            this.testContext = testContext;
            scope = new Scope(this, AssertionFailureBehavior.LogAndThrow);
        }

        /// <summary>
        /// Gets the current assertion context.
        /// </summary>
        public static AssertionContext CurrentContext
        {
            get
            {
                TestContext context = Gallio.Framework.TestContext.CurrentContext;
                lock (context.Data)
                {
                    AssertionContext assertionContext;
                    if (!context.Data.TryGetValue(AssertionContextKey, out assertionContext))
                    {
                        assertionContext = new AssertionContext(context);
                        context.Data.SetValue(AssertionContextKey, assertionContext);
                    }

                    return assertionContext;
                }
            }
        }

        /// <summary>
        /// Gets the associated test context.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContext; }
        }

        /// <summary>
        /// <para>
        /// Gets the current assertion failure behavior.
        /// </para>
        /// <para>
        /// The value of this property may change during invocations of <see cref="CaptureFailures" />.
        /// </para>
        /// </summary>
        public AssertionFailureBehavior AssertionFailureBehavior
        {
            get { return scope.AssertionFailureBehavior; }
        }

        /// <summary>
        /// <para>
        /// Submits an assertion failure.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The behavior of this method depends upon the current setting of
        /// <see cref="AssertionFailureBehavior" />.  Typically this method will
        /// throw an <see cref="AssertionFailureException" /> but it might do
        /// other things.  Do not assume that it will throw an exception!
        /// </para>
        /// </remarks>
        /// <param name="failure">The assertion failure</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="failure"/>
        /// is null</exception>
        public void SubmitFailure(AssertionFailure failure)
        {
            if (failure == null)
                throw new ArgumentNullException("failure");

            scope.SubmitFailure(failure);
        }

        /// <summary>
        /// <para>
        /// Performs an action and returns an array containing the assertion failures
        /// that were observed within the block.
        /// </para>
        /// <para>
        /// The set of failures captured will depend on the setting of <paramref name="assertionFailureBehavior"/>.
        /// <list type="bullet">
        /// <item>If set to <see cref="Framework.AssertionFailureBehavior.LogAndThrow"/>, then only
        /// the first failure will be captured since execution will be immediately aborted when it happens.</item>
        /// <item>If set to <see cref="Framework.AssertionFailureBehavior.Log"/> or
        /// <see cref="Framework.AssertionFailureBehavior.Defer" />, then all
        /// failures will be captured until the block terminates or throws an exception for some other reason.</item>
        /// <item>If set to <see cref="Framework.AssertionFailureBehavior.Ignore"/>, then no failures
        /// will be captured since they will all be ignored!</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="action">The action to invoke</param>
        /// <param name="assertionFailureBehavior">The assertion failure behavior to use while
        /// executing the block</param>
        /// <param name="captureExceptionAsAssertionFailure">Specifies whether to represent an exception
        /// as an assertion failure, otherwise it is rethrown</param>
        /// <returns>The array of failures, may be empty if none</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        [TestFrameworkInternal]
        public AssertionFailure[] CaptureFailures(Action action, AssertionFailureBehavior assertionFailureBehavior,
            bool captureExceptionAsAssertionFailure)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Scope newScope = new Scope(this, assertionFailureBehavior);
            Scope oldScope = null;
            try
            {
                oldScope = Interlocked.Exchange(ref scope, newScope);

                return newScope.CaptureFailures(action, captureExceptionAsAssertionFailure);
            }
            finally
            {
                if (oldScope != null && Interlocked.CompareExchange(ref scope, oldScope, newScope) != newScope)
                    throw new NotSupportedException("The current implementation does not support capturing failures concurrently.");
            }
        }

        private sealed class Scope
        {
            private readonly AssertionContext context;
            private readonly AssertionFailureBehavior assertionFailureBehavior;
            private List<AssertionFailure> savedFailures;

            public Scope(AssertionContext context, AssertionFailureBehavior assertionFailureBehavior)
            {
                this.context = context;
                this.assertionFailureBehavior = assertionFailureBehavior;
            }

            public AssertionFailureBehavior AssertionFailureBehavior
            {
                get { return assertionFailureBehavior; }
            }

            public void SubmitFailure(AssertionFailure failure)
            {
                SubmitFailure(failure, false);
            }

            [TestFrameworkInternal]
            public AssertionFailure[] CaptureFailures(Action action, bool captureExceptionAsAssertionFailure)
            {
                try
                {
                    action();
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (AssertionFailureException ex)
                {
                    if (!ex.IsSilent)
                        SubmitFailure(ex.Failure, true);
                }
                catch (Exception ex)
                {
                    if (!captureExceptionAsAssertionFailure)
                        throw;

                    SubmitFailure(new AssertionFailureBuilder("An exception occurred.")
                        .AddException(ex)
                        .ToAssertionFailure(), true);
                }

                return GetSavedFailuresAsArray();
            }

            private void SubmitFailure(AssertionFailure failure, bool noThrow)
            {
                SaveFailureAccordingToBehavior(failure);
                LogFailureAccordingToBehavior(failure);

                if (! noThrow)
                    ThrowFailureAccordingToBehavior(failure);
            }

            private void AddSavedFailure(AssertionFailure failure)
            {
                lock (this)
                {
                    if (savedFailures == null)
                        savedFailures = new List<AssertionFailure>();
                    savedFailures.Add(failure);
                }
            }

            private AssertionFailure[] GetSavedFailuresAsArray()
            {
                lock (this)
                {
                    if (savedFailures == null)
                        return EmptyArray<AssertionFailure>.Instance;
                    return savedFailures.ToArray();
                }
            }

            private void LogFailure(AssertionFailure failure)
            {
                failure.Log(context.testContext.LogWriter);
            }

            private void LogFailureAccordingToBehavior(AssertionFailure failure)
            {
                switch (assertionFailureBehavior)
                {
                    case AssertionFailureBehavior.LogAndThrow:
                    case AssertionFailureBehavior.Log:
                        LogFailure(failure);
                        break;
                }
            }

            private void SaveFailureAccordingToBehavior(AssertionFailure failure)
            {
                if (assertionFailureBehavior != AssertionFailureBehavior.Ignore)
                    AddSavedFailure(failure);
            }

            private void ThrowFailureAccordingToBehavior(AssertionFailure failure)
            {
                if (assertionFailureBehavior == AssertionFailureBehavior.LogAndThrow)
                    throw new AssertionFailureException(failure, true);
            }
        }
    }
}
