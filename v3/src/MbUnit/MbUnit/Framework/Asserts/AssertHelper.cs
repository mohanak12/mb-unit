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
using System.Diagnostics;
using System.Threading;
using Gallio;
using Gallio.Framework;
using Gallio.Framework.Utilities;

namespace MbUnit.Framework
{
    /// <summary>
    /// Provides utilities to assist with the implementation of new asserts.
    /// </summary>
    public abstract class AssertHelper
    {
        /// <summary>
        /// <para>
        /// Verifies that an assertion succeeded.
        /// </para>
        /// <para>
        /// If the assertion function returns null then the assertion is deemed to have passed.
        /// If it returns an <see cref="AssertionFailure" /> or throws an exception,
        /// then is is deemed to have failed.
        /// </para>
        /// <para>
        /// When an assertion failure is detected, it is submitted to <see cref="AssertionContext.SubmitFailure"/>
        /// which may choose to throw a <see cref="AssertionFailureException" /> or do something else.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Using this method enables the system to track statistics about assertions
        /// and to ensure that assertion failures are reported uniformly.
        /// </para>
        /// <para>
        /// It is important to note that not all failures will result in a <see cref="AssertionFailureException"/>
        /// being thrown.  Refer to <see cref="AssertionContext.SubmitFailure"/> for details.
        /// </para>
        /// </remarks>
        /// <param name="assertionFunc">The assertion function to evaluate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assertionFunc"/> is null</exception>
        [TestFrameworkInternal]
        public static void Verify(Func<AssertionFailure> assertionFunc)
        {
            if (assertionFunc == null)
                throw new ArgumentNullException("assertionFunc");

            TestContext.CurrentContext.IncrementAssertCount();

            AssertionFailure failure;
            try
            {
                failure = assertionFunc();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failure = new AssertionFailureBuilder("An exception occurred while verifying an assertion.")
                    .AddException(ex)
                    .ToAssertionFailure();
            }

            if (failure != null)
                AssertionContext.CurrentContext.SubmitFailure(failure);
        }

        /// <summary>
        /// Performs an action and returns an array containing the assertion failures
        /// that were observed within the block.  If the block throws an exception, it
        /// is reified as an assertion failure.
        /// </summary>
        /// <param name="action">The action to invoke</param>
        /// <returns>The array of failures, may be empty if none</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        [TestFrameworkInternal]
        public static AssertionFailure[] Eval(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            AssertionFailureBehavior behavior = AssertionContext.CurrentContext.AssertionFailureBehavior;
            return AssertionContext.CurrentContext.CaptureFailures(action, behavior, true);
        }
    }
}
