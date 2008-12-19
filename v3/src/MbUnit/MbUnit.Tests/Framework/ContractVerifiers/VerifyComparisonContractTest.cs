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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Gallio.Model;
using Gallio.Runner.Reports;
using Gallio.Tests;
using Gallio.Tests.Integration;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace MbUnit.Tests.Framework.ContractVerifiers
{
    [RunSample(typeof(FullContractOnComparableSample))]
    [RunSample(typeof(PartialContractOnComparableSample))]
    public class VerifyComparisonContractTest : AbstractContractVerifierTest
    {
        [Test]
        [Row(typeof(FullContractOnComparableSample), "ComparableCompareTo", TestStatus.Passed)]
        [Row(typeof(FullContractOnComparableSample), "OperatorGreaterThan", TestStatus.Passed)]
        [Row(typeof(FullContractOnComparableSample), "OperatorGreaterThanOrEqual", TestStatus.Passed)]
        [Row(typeof(FullContractOnComparableSample), "OperatorLessThan", TestStatus.Passed)]
        [Row(typeof(FullContractOnComparableSample), "OperatorLessThanOrEqual", TestStatus.Passed)]
        [Row(typeof(PartialContractOnComparableSample), "ComparableCompareTo", TestStatus.Passed)]
        [Row(typeof(PartialContractOnComparableSample), "OperatorGreaterThan", TestStatus.Inconclusive)]
        [Row(typeof(PartialContractOnComparableSample), "OperatorGreaterThanOrEqual", TestStatus.Inconclusive)]
        [Row(typeof(PartialContractOnComparableSample), "OperatorLessThan", TestStatus.Inconclusive)]
        [Row(typeof(PartialContractOnComparableSample), "OperatorLessThanOrEqual", TestStatus.Inconclusive)]
        public void VerifySampleEqualityContract(Type fixtureType, string testMethodName, TestStatus expectedTestStatus)
        {
            VerifySampleContract("ComparisonTests", fixtureType, testMethodName, expectedTestStatus);
        }

        [Explicit]
        internal class FullContractOnComparableSample
        {
            [ContractVerifier]
            public readonly IContractVerifier ComparisonTests = new VerifyComparisonContract<SampleComparable>()
            {
                EquivalenceClasses = new EquivalenceClassCollection<SampleComparable>(
                    new EquivalenceClass<SampleComparable>(new SampleComparable(123), new SampleComparable(123)),
                    new EquivalenceClass<SampleComparable>(new SampleComparable(456)),
                    new EquivalenceClass<SampleComparable>(new SampleComparable(789))),

                ImplementsOperatorOverloads = true
            };
        }

        [Explicit]
        private class PartialContractOnComparableSample
        {
            [ContractVerifier]
            public readonly IContractVerifier ComparisonTests = new VerifyComparisonContract<SampleComparable>()
            {
                EquivalenceClasses = new EquivalenceClassCollection<SampleComparable>(
                    new EquivalenceClass<SampleComparable>(new SampleComparable(123), new SampleComparable(123)),
                    new EquivalenceClass<SampleComparable>(new SampleComparable(456)),
                    new EquivalenceClass<SampleComparable>(new SampleComparable(789))),

                ImplementsOperatorOverloads = false
            };
        }

        /// <summary>
        /// Sample comparable type.
        /// </summary>
        internal class SampleComparable : IComparable<SampleComparable>
        {
            private int value;

            public SampleComparable(int value)
            {
                this.value = value;
            }

            public int CompareTo(SampleComparable other)
            {
                return (other == null) ? Int32.MaxValue : value.CompareTo(other.value);
            }

            public static bool operator >=(SampleComparable left, SampleComparable right)
            {
                return ((left == null) && (right == null)) || ((left != null) && (left.CompareTo(right) >= 0));
            }

            public static bool operator <=(SampleComparable left, SampleComparable right)
            {
                return (left == null) || (left.CompareTo(right) <= 0);
            }

            public static bool operator >(SampleComparable left, SampleComparable right)
            {
                return (left != null) && (left.CompareTo(right) > 0);
            }

            public static bool operator <(SampleComparable left, SampleComparable right)
            {
                return ((left != null) || (right != null)) && ((left == null) || (left.CompareTo(right) < 0));
            }
        }
    }
}
