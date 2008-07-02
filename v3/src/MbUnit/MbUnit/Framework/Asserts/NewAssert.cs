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
using System.ComponentModel;
using Gallio;
using Gallio.Framework.Utilities;

namespace MbUnit.Framework
{
    /// <summary>
    /// Defines a set of assertions.
    /// </summary>
    [TestFrameworkInternal]
    public abstract class NewAssert
    {
        #region Private stuff
        /// <summary>
        /// Always throws a <see cref="InvalidOperationException" />.
        /// Use <see cref="NewAssert.AreEqual{T}(T, T)" /> instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new void Equals(object a, object b)
        {
            throw new InvalidOperationException("Assert.Equals should not be used for assertions.  Use Assert.AreEqual instead.");
        }

        /// <summary>
        /// Always throws a <see cref="InvalidOperationException" />.
        /// Use <see cref="NewAssert.AreSame{T}(T, T)" /> instead.
        /// </summary>
        public static new void ReferenceEquals(object a, object b)
        {
            throw new InvalidOperationException("Assert.ReferenceEquals should not be used for assertions.  Use Assert.AreSame instead.");
        }
        #endregion

        #region Syntax Extensions
        /// <summary>
        /// Provides methods for composing assertions to map over complex data structures.
        /// </summary>
        public static AssertOverSyntax Over
        {
            get { return AssertOverSyntax.Instance; }
        }
        #endregion

        #region AreEqual
        /// <summary>
        /// Verifies that an actual value equals some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue)
        {
            AreEqual<T>(expectedValue, actualValue, (string) null, null);
        }

        /// <summary>
        /// Verifies that an actual value equals some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue, string messageFormat, params object[] messageArgs)
        {
            AreEqual<T>(expectedValue, actualValue, (Func<T, T, bool>) null, messageFormat, messageArgs);
        }

        /// <summary>
        /// Verifies that an actual value equals some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue, IEqualityComparer<T> comparer)
        {
            AreEqual<T>(expectedValue, actualValue, comparer, null, null);
        }

        /// <summary>
        /// Verifies that an actual value equals some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue, IEqualityComparer<T> comparer, string messageFormat, params object[] messageArgs)
        {
            AreEqual<T>(expectedValue, actualValue, comparer != null ? comparer.Equals : (Func<T, T, bool>) null, messageFormat, messageArgs);
        }

        /// <summary>
        /// Verifies that an actual value equals some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue, Func<T, T, bool> comparer)
        {
            AreEqual<T>(expectedValue, actualValue, comparer, null, null);
        }

        /// <summary>
        /// Verifies that an actual value equals some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreEqual<T>(T expectedValue, T actualValue, Func<T, T, bool> comparer, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (comparer == null)
                    comparer = DefaultEqualityComparer;

                if (comparer(expectedValue, actualValue))
                    return null;

                return new AssertionFailureBuilder("Expected values to be equal.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetExpectedValue(expectedValue)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region AreNotEqual
        /// <summary>
        /// Verifies that an actual value does not equal some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue)
        {
            AreNotEqual<T>(expectedValue, actualValue, (string)null, null);
        }

        /// <summary>
        /// Verifies that an actual value does not equal some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue, string messageFormat, params object[] messageArgs)
        {
            AreNotEqual<T>(expectedValue, actualValue, (Func<T, T, bool>)null, messageFormat, messageArgs);
        }

        /// <summary>
        /// Verifies that an actual value does not equal some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue, IEqualityComparer<T> comparer)
        {
            AreNotEqual<T>(expectedValue, actualValue, comparer, null, null);
        }

        /// <summary>
        /// Verifies that an actual value does not equal some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue, IEqualityComparer<T> comparer, string messageFormat, params object[] messageArgs)
        {
            AreNotEqual<T>(expectedValue, actualValue, comparer != null ? comparer.Equals : (Func<T, T, bool>)null, messageFormat, messageArgs);
        }

        /// <summary>
        /// Verifies that an actual value does not equal some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue, Func<T, T, bool> comparer)
        {
            AreNotEqual<T>(expectedValue, actualValue, comparer, null, null);
        }

        /// <summary>
        /// Verifies that an actual value does not equal some expected value according to a particular comparer.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="comparer">The comparer to use, or null to use the default one</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotEqual<T>(T expectedValue, T actualValue, Func<T, T, bool> comparer, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (comparer == null)
                    comparer = DefaultEqualityComparer;

                if (! comparer(expectedValue, actualValue))
                    return null;

                return new AssertionFailureBuilder("Expected values to be non-equal.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetExpectedValue(expectedValue)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region AreSame
        /// <summary>
        /// Verifies that an actual value is referentially identical to some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreSame<T>(T expectedValue, T actualValue)
            where T : class
        {
            AreSame<T>(expectedValue, actualValue, (string)null, null);
        }

        /// <summary>
        /// Verifies that an actual value is referentially identical to some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreSame<T>(T expectedValue, T actualValue, string messageFormat, params object[] messageArgs)
            where T : class
        {
            AssertHelper.Verify(delegate
            {
                if (Object.ReferenceEquals(expectedValue, actualValue))
                    return null;

                return new AssertionFailureBuilder("Expected values to be referentially identical.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetExpectedValue(expectedValue)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region AreNotSame
        /// <summary>
        /// Verifies that an actual value is not referentially identical to some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotSame<T>(T expectedValue, T actualValue)
            where T : class
        {
            AreNotSame<T>(expectedValue, actualValue, (string)null, null);
        }

        /// <summary>
        /// Verifies that an actual value is not referentially identical to some expected value.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void AreNotSame<T>(T expectedValue, T actualValue, string messageFormat, params object[] messageArgs)
            where T : class
        {
            AssertHelper.Verify(delegate
            {
                if (! Object.ReferenceEquals(expectedValue, actualValue))
                    return null;

                return new AssertionFailureBuilder("Expected values to be referentially different.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetExpectedValue(expectedValue)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region IsTrue
        /// <summary>
        /// Verifies that an actual value is true.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsTrue(bool actualValue)
        {
            IsTrue(actualValue, null, null);
        }

        /// <summary>
        /// Verifies that an actual value is true.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsTrue(bool actualValue, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (actualValue)
                    return null;

                return new AssertionFailureBuilder("Expected value to be true.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region IsFalse
        /// <summary>
        /// Verifies that an actual value is false.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsFalse(bool actualValue)
        {
            IsFalse(actualValue, null, null);
        }

        /// <summary>
        /// Verifies that an actual value is false.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsFalse(bool actualValue, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (! actualValue)
                    return null;

                return new AssertionFailureBuilder("Expected value to be false.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region IsNull
        /// <summary>
        /// Verifies that an actual value is null.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsNull(object actualValue)
        {
            IsNull(actualValue, null, null);
        }

        /// <summary>
        /// Verifies that an actual value is null.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsNull(object actualValue, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (actualValue == null)
                    return null;

                return new AssertionFailureBuilder("Expected value to be null.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region IsNotNull
        /// <summary>
        /// Verifies that an actual value is not null.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsNotNull(object actualValue)
        {
            IsNotNull(actualValue, null, null);
        }

        /// <summary>
        /// Verifies that an actual value is not null.
        /// </summary>
        /// <param name="actualValue">The actual value</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void IsNotNull(object actualValue, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                if (actualValue != null)
                    return null;

                return new AssertionFailureBuilder("Expected value to be non-null.")
                    .SetMessage(messageFormat, messageArgs)
                    .SetActualValue(actualValue)
                    .ToAssertionFailure();
            });
        }
        #endregion

        #region Throws
        /// <summary>
        /// Evaluates an action delegate and verifies that it throws an exception of a particular type.
        /// </summary>
        /// <typeparam name="TExpectedException">The expected type of exception</typeparam>
        /// <param name="action">The action delegate to evaluate</param>
        /// <returns>The exception that was thrown</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static TExpectedException Throws<TExpectedException>(Action action)
            where TExpectedException : Exception
        {
            return Throws<TExpectedException>(action, null, null);
        }

        /// <summary>
        /// Evaluates an action delegate and verifies that it throws an exception of a particular type.
        /// </summary>
        /// <typeparam name="TExpectedException">The expected type of exception</typeparam>
        /// <param name="action">The action delegate to evaluate</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <returns>The exception that was thrown</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static TExpectedException Throws<TExpectedException>(Action action, string messageFormat, params object[] messageArgs)
            where TExpectedException : Exception
        {
            return (TExpectedException)Throws(typeof(TExpectedException), action, messageFormat, messageArgs);
        }

        /// <summary>
        /// Evaluates an action delegate and verifies that it throws an exception of a particular type.
        /// </summary>
        /// <param name="expectedExceptionType">The expected exception type</param>
        /// <param name="action">The action delegate to evaluate</param>
        /// <returns>The exception that was thrown</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static Exception Throws(Type expectedExceptionType, Action action)
        {
            return Throws(expectedExceptionType, action);
        }

        /// <summary>
        /// Evaluates an action delegate and verifies that it throws an exception of a particular type.
        /// </summary>
        /// <param name="expectedExceptionType">The expected exception type</param>
        /// <param name="action">The action delegate to evaluate</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <returns>The exception that was thrown</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static Exception Throws(Type expectedExceptionType, Action action, string messageFormat, params object[] messageArgs)
        {
            Exception result = null;
            AssertHelper.Verify(delegate
            {
                try
                {
                    action();
                    return new AssertionFailureBuilder("Expected the block to throw an exception.")
                        .SetMessage(messageFormat, messageArgs)
                        .SetLabeledValue("Expected Exception Type", expectedExceptionType)
                        .ToAssertionFailure();
                }
                catch (Exception actualException)
                {
                    if (expectedExceptionType.IsInstanceOfType(actualException))
                    {
                        result = actualException;
                        return null;
                    }

                    return new AssertionFailureBuilder("The block threw an exception of a different type than was expected.")
                        .SetMessage(messageFormat, messageArgs)
                        .SetLabeledValue("Expected Exception Type", expectedExceptionType)
                        .AddException(actualException)
                        .ToAssertionFailure();
                }
            });

            return result;
        }
        #endregion

        #region DoesNotThrow
        /// <summary>
        /// Evaluates an action delegate and verifies that it does not throw an exception of any type.
        /// </summary>
        /// <param name="action">The action delegate to evaluate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void DoesNotThrow(Action action)
        {
            DoesNotThrow(action, null, null);
        }

        /// <summary>
        /// Evaluates an action delegate and verifies that it does not throw an exception of any type.
        /// </summary>
        /// <param name="action">The action delegate to evaluate</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void DoesNotThrow(Action action, string messageFormat, params object[] messageArgs)
        {
            AssertHelper.Verify(delegate
            {
                try
                {
                    action();
                    return null;
                }
                catch (Exception actualException)
                {
                    return new AssertionFailureBuilder("The block threw an exception but none was expected.")
                        .SetMessage(messageFormat, messageArgs)
                        .AddException(actualException)
                        .ToAssertionFailure();
                }
            });
        }
        #endregion

#if false
        #region That
        /// <summary>
        /// Verifies that a particular condition holds true.
        /// </summary>
        /// <param name="condition">The condition to assert</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="condition"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void That(AssertionCondition condition)
        {
            That(condition, (string)null, null);
        }

        /// <summary>
        /// Verifies that a particular condition holds true.
        /// </summary>
        /// <param name="condition">The condition to assert</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="condition"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void That(AssertionCondition condition, string messageFormat, params object[] messageArgs)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            XXX
        }

        /// <summary>
        /// Verifies that an actual value satisfies a particular constraint.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="actualValue">The actual value</param>
        /// <param name="constraint">The constraint to evaluate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="constraint"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void That<T>(T actualValue, AssertionConstraint<T> constraint)
        {
            That(actualValue, constraint, null, null);
        }

        /// <summary>
        /// Verifies that an actual value satisfies a particular constraint.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="actualValue">The actual value</param>
        /// <param name="constraint">The constraint to evaluate</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="constraint"/> is null</exception>
        /// <exception cref="AssertionException">Thrown if the verification failed</exception>
        public static void That<T>(T actualValue, AssertionConstraint<T> constraint, string messageFormat, params object[] messageArgs)
        {
            if (constraint == null)
                throw new ArgumentNullException("constraint");

            That(new ConstraintCondition<T>(actualValue, constraint), messageFormat, messageArgs);
        }
        #endregion
#endif

        #region Multiple
        /// <summary>
        /// <para>
        /// Executes an action delegate that contains multiple related assertions.
        /// </para>
        /// <para>
        /// While the delegate runs, the behavior of assertions is change such that
        /// failures are captured but do not cause a <see cref="AssertionFailureException" />
        /// to be throw.  When the delegate returns, the previous assertion failure behavior
        /// is restored and any captured assertion failures are reported.  The net effect
        /// of this change is that the test can continue to run even after an assertion failure
        /// occurs which can help to provide more information about the problem.
        /// </para>
        /// </summary>
        /// <remarks>
        /// A multiple assertion block is useful for verifying the state of a single
        /// component with many parts that require several assertions to check.
        /// This feature can accelerate debugging because more diagnostic information
        /// become available at once.
        /// </remarks>
        /// <param name="action">The action to invoke</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        public static void Multiple(Action action)
        {
            Multiple(action, null, null);
        }

        /// <summary>
        /// <para>
        /// Executes an action delegate that contains multiple related assertions.
        /// </para>
        /// <para>
        /// While the delegate runs, the behavior of assertions is change such that
        /// failures are captured but do not cause a <see cref="AssertionFailureException" />
        /// to be throw.  When the delegate returns, the previous assertion failure behavior
        /// is restored and any captured assertion failures are reported.  The net effect
        /// of this change is that the test can continue to run even after an assertion failure
        /// occurs which can help to provide more information about the problem.
        /// </para>
        /// <para>
        /// If the block throws an exception other than an assertion failure, then it is
        /// similarly recorded.
        /// </para>
        /// </summary>
        /// <remarks>
        /// A multiple assertion block is useful for verifying the state of a single
        /// component with many parts that require several assertions to check.
        /// This feature can accelerate debugging because more diagnostic information
        /// become available at once.
        /// </remarks>
        /// <param name="action">The action to invoke</param>
        /// <param name="messageFormat">The custom assertion message format, or null if none</param>
        /// <param name="messageArgs">The custom assertion message arguments, or null if none</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        public static void Multiple(Action action, string messageFormat, params object[] messageArgs)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            AssertHelper.Verify(delegate
            {
                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(action,
                    AssertionFailureBehavior.Log, true);
                if (failures.Length == 0)
                    return null;

                return new AssertionFailureBuilder(String.Format("There were {0} failures within the multiple assertion block.", failures.Length))
                    .SetMessage(messageFormat, messageArgs)
                    .ToAssertionFailure();
            });
        }
        #endregion

        private static bool DefaultEqualityComparer<T>(T expectedValue, T actualValue)
        {
            if (Object.ReferenceEquals(expectedValue, actualValue))
                return true;
            if (expectedValue == null || actualValue == null)
                return false;
            return expectedValue.Equals(actualValue);
        }
    }
}
