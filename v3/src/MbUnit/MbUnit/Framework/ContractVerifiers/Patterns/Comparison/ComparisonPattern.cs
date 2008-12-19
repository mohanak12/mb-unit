﻿// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using System.Text;
using Gallio.Framework.Pattern;
using Gallio.Framework.Assertions;
using System.Collections;
using System.Reflection;
using Gallio;

namespace MbUnit.Framework.ContractVerifiers.Patterns.Comparison
{
    /// <summary>
    /// General purpose test pattern for contract verifiers.
    /// It verifies that a given method evaluating comparison between
    /// two instances, returns the expected result according to
    /// the equivalence class the objects belongs to.
    /// </summary>
    /// <typeparam name="TTarget">The target comparable type.</typeparam>
    /// <typeparam name="TResult">The type of the results provided by
    /// the comparison method. Usually a Int32 or a Boolean.</typeparam>
    internal class ComparisonPattern<TTarget, TResult> : ContractVerifierPattern
        where TTarget : IComparable<TTarget>
        where TResult : struct
    {
        private ComparisonPatternSettings<TResult> settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings.</param>
        internal ComparisonPattern(ComparisonPatternSettings<TResult> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.settings = settings;
        }

        /// <inheritdoc />
        protected override string Name
        {
            get
            {
                return settings.Name;
            }
        }

        /// <inheritdoc />
        protected internal override void Run(IContractVerifierPatternInstanceState state)
        {
            AssertionHelper.Verify(() =>
            {
                if (settings.ComparisonMethodInfo != null)
                    return null;

                return new AssertionFailureBuilder("Comparison method/operator expected to be implemented.")
                    .AddLabeledValue("Expected Method", settings.SignatureDescription)
                    .ToAssertionFailure();
            });

            VerifyComparisonContract(state,
                (a, b) => settings.PostProcesses(settings.ComparisonMethodInfo.IsStatic ?
                        (TResult)settings.ComparisonMethodInfo.Invoke(null, new object[] { a, b }) :
                        (TResult)settings.ComparisonMethodInfo.Invoke(a, new object[] { b })));
        }

        private void VerifyComparisonContract(IContractVerifierPatternInstanceState state, Func<object, object, TResult> compares)
        {
            // Get the equivalence classes before entering the multiple assertion block in
            // order to catch any missing implementation of IEquivalentClassProvider<T> before.
            IEnumerable equivalenceClasses = GetEquivalentClasses(settings.EquivalenceClassSource, state);

            Assert.Multiple(() =>
            {
                VerifyEqualityBetweenTwoNullReferences(compares);
                int i = 0;

                foreach (object a in equivalenceClasses)
                {
                    int j = 0;

                    foreach (object b in GetEquivalentClasses(settings.EquivalenceClassSource, state))
                    {
                        CompareEquivalentInstances((IEnumerable)a, (IEnumerable)b, settings.PostProcesses(settings.Refers(i, j)), compares);
                        j++;
                    }

                    i++;
                }
            });
        }

        private void CompareEquivalentInstances(IEnumerable a, IEnumerable b, TResult expectedResult, Func<object, object, TResult> compares)
        {
            foreach (object x in a)
            {
                VerifyNullReferenceComparison(x, compares);

                if (settings.ComparisonMethodInfo.IsStatic)
                {
                    foreach (object y in b)
                    {
                        AssertionHelper.Verify(() =>
                        {
                            TResult actualResult = compares(x, y);
                            if (expectedResult.Equals(actualResult))
                                return null;

                            return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                                .AddRawLabeledValue("Left Value", x)
                                .AddRawLabeledValue("Right Value", y)
                                .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                                .AddLabeledValue("Actual Result", settings.Formats(actualResult))
                                .ToAssertionFailure();
                        });
                    }
                }
            }
        }

        private void VerifyEqualityBetweenTwoNullReferences(Func<object, object, TResult> compares)
        {
            if (!typeof(TTarget).IsValueType && settings.ComparisonMethodInfo.IsStatic)
            {
                AssertionHelper.Verify(() =>
                {
                    TResult actualResult;
                    TResult expectedResult = settings.PostProcesses(settings.Refers(0, 0));

                    try
                    {
                        actualResult = compares(null, null);
                        if (expectedResult.Equals(actualResult))
                            return null;
                    }
                    catch (TargetInvocationException exception)
                    {
                        return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                            .AddRawLabeledValue("Left Value", null)
                            .AddRawLabeledValue("Right Value", null)
                            .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                            .AddRawLabeledValue("Actual Result", exception.InnerException)
                            .ToAssertionFailure();
                    }
                    catch (NullReferenceException exception)
                    {
                        return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                            .AddRawLabeledValue("Left Value", null)
                            .AddRawLabeledValue("Right Value", null)
                            .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                            .AddRawLabeledValue("Actual Result", exception)
                            .ToAssertionFailure();
                    }

                    return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                        .AddRawLabeledValue("Left Value", null)
                        .AddRawLabeledValue("Right Value", null)
                        .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                        .AddLabeledValue("Actual Result", settings.Formats(actualResult))
                        .ToAssertionFailure();
                });
            }
        }

        private void VerifyNullReferenceComparison(object x, Func<object, object, TResult> compares)
        {
            if (!typeof(TTarget).IsValueType)
            {
                AssertionHelper.Verify(() =>
                {
                    TResult actualResult = compares(x, null);
                    TResult expectedResult = settings.PostProcesses(settings.Refers(0, Int32.MinValue));
                    if (expectedResult.Equals(actualResult))
                        return null;

                    return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                        .AddRawLabeledValue("Left Value", x)
                        .AddRawLabeledValue("Right Value", null)
                        .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                        .AddLabeledValue("Actual Result", settings.Formats(actualResult))
                        .ToAssertionFailure();
                });

                if (settings.ComparisonMethodInfo.IsStatic)
                {
                    AssertionHelper.Verify(() =>
                    {
                        TResult actualResult;
                        TResult expectedResult = settings.PostProcesses(settings.Refers(Int32.MinValue, 0));

                        try
                        {
                            actualResult = compares(null, x);
                            if (expectedResult.Equals(actualResult))
                                return null;
                        }
                        catch (TargetInvocationException exception)
                        {
                            return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                                .AddRawLabeledValue("Left Value", null)
                                .AddRawLabeledValue("Right Value", x)
                                .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                                .AddRawLabeledValue("Actual Result", exception.InnerException)
                                .ToAssertionFailure();
                        }
                        catch (NullReferenceException exception)
                        {
                            return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                                .AddRawLabeledValue("Left Value", null)
                                .AddRawLabeledValue("Right Value", x)
                                .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                                .AddRawLabeledValue("Actual Result", exception)
                                .ToAssertionFailure();
                        }

                        return new AssertionFailureBuilder("The comparison result between left and right values does not meet expectations.")
                            .AddRawLabeledValue("Left Value", null)
                            .AddRawLabeledValue("Right Value", x)
                            .AddLabeledValue("Expected Result", settings.Formats(expectedResult))
                            .AddLabeledValue("Actual Result", settings.Formats(actualResult))
                            .ToAssertionFailure();
                    });
                }
            }
        }
    }
}
