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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Gallio.Utilities;

namespace Gallio.Model
{
    /// <summary>
    /// <para>
    /// Describes the outcome of a test.
    /// </para>
    /// <para>
    /// An outcome consists of two parts.  First, a required <see cref="Status" /> that describes
    /// whether test passed, failed or was inconclusive.  Second, an optional <see cref="Category"/>
    /// that enables different outcomes with the same status to be distinguished.  The category
    /// also provides an opportunity to extend the definition of a test outcome to include
    /// custom semantic details.
    /// </para>
    /// </summary>
    [Serializable]
    [XmlRoot("outcome", Namespace = XmlSerializationUtils.GallioNamespace)]
    public struct TestOutcome : IXmlSerializable, IEquatable<TestOutcome>
    {
        private TestStatus status;
        private string category;

        /// <summary>
        /// Creates a test outcome with no category.
        /// </summary>
        /// <param name="status">The test <see cref="Status"/></param>
        public TestOutcome(TestStatus status)
        {
            this.status = status;
            category = null;
        }

        /// <summary>
        /// Creates a test outcome with an optional category.
        /// </summary>
        /// <param name="status">The test <see cref="Status"/></param>
        /// <param name="category">The test <see cref="Category"/>, or null or an empty string if none</param>
        public TestOutcome(TestStatus status, string category)
        {
            this.status = status;
            this.category = string.IsNullOrEmpty(category) ? null : category;
        }

        /// <summary>
        /// <para>
        /// Gets the test status.
        /// </para>
        /// <para>
        /// The test status describes whether a test passed, failed or produced an inconclusive
        /// result.  This information may be reported to the user with icons and textual
        /// labels to explain the overall significance of the outcome.
        /// </para>
        /// </summary>
        public TestStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// <para>
        /// Gets the test outcome category, or null if none.  Never an empty string.
        /// </para>
        /// <para>
        /// The category, when provided, provides additional information to describe what happened
        /// to result in this particular outcome.
        /// </para>
        /// <para>
        /// Examples: "ignored", "skipped", "pending", "canceled", "aborted", "timeout".
        /// </para>
        /// <para>
        /// Naming guidelines:
        /// <list type="bullet">
        /// <item>A category should be a single lower-case word.</item>
        /// <item>It should be a word that can appear on its own or following a number.  Consequently, nouns are poor choices because they may need to be pluralized.</item>
        /// <item>It should not repeat the information already provided by the <see cref="Status"/>.  Consequently, "passed", "failed" and "inconclusive" are poor choices.</item>
        /// <item>It should be a standard category, if possible.</item>
        /// <item>It should not be too granular.  If too many categories are in common usage, test result summaries by category may become unwieldly.</item>
        /// </list>
        /// </para>
        /// </summary>
        public string Category
        {
            get { return category; }
        }

        /// <summary>
        /// <para>
        /// Gets the name of the outcome as it should be displayed.
        /// </para>
        /// <para>
        /// The display name is the outcome's <see cref="Category" />, if available.
        /// Otherwise it is a lowercase rendition of the outcome's <see cref="Status" />.
        /// </para>
        /// </summary>
        public string DisplayName
        {
            get { return category ?? StatusToString(status); }
        }

        /// <summary>
        /// If the other outcome is more severe than this one, returns it.
        /// Otherwise returns this outcome.
        /// </summary>
        /// <remarks>
        /// This combination rule has the nice property of preserving the first
        /// failure encountered even if subsequent failures occur and are combined.
        /// </remarks>
        /// <param name="other">The other outcome</param>
        /// <returns>The combined outcome</returns>
        /// <seealso cref="TestStatus"/> for test status severity ranking information.
        public TestOutcome CombineWith(TestOutcome other)
        {
            if (other.status > status)
                return other;
            return this;
        }

        /// <summary>
        /// Returns the <see cref="DisplayName" /> of the outcome.
        /// </summary>
        /// <returns>The display name</returns>
        public override string ToString()
        {
            return DisplayName;
        }

        #region Built-in Outcomes
        /// <summary>
        /// Gets a standard outcome for a test that passed.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Passed"/>.
        /// Category: null.
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Passed
        {
            get { return new TestOutcome(TestStatus.Passed); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that failed.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Failed"/>.
        /// Category: null.
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Failed
        {
            get { return new TestOutcome(TestStatus.Failed); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that failed due to an error.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Failed"/>.
        /// Category: "error".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Error
        {
            get { return new TestOutcome(TestStatus.Failed, "error"); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that failed because it ran out of time.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Failed"/>.
        /// Category: "timeout".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Timeout
        {
            get { return new TestOutcome(TestStatus.Failed, "timeout"); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that was inconclusive.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Inconclusive"/>.
        /// Category: null.
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Inconclusive
        {
            get { return new TestOutcome(TestStatus.Inconclusive); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that has an inconclusive outcome because it was canceled.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Inconclusive"/>.
        /// Category: "canceled".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Canceled
        {
            get { return new TestOutcome(TestStatus.Inconclusive, "canceled"); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that did not run.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Skipped"/>.
        /// Category: null.
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Skipped
        {
            get { return new TestOutcome(TestStatus.Skipped); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that did not run because the user is choosing
        /// to ignore it.  Perhaps the test is broken or non-functional.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Skipped"/>.
        /// Category: "ignored".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Ignored
        {
            get { return new TestOutcome(TestStatus.Skipped, "ignored"); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that did not run because it has unsatisfied
        /// prerequisites.  The test may depend on functionality that has not yet been implemented
        /// or perhaps the test itself has yet to be implemented.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Skipped"/>.
        /// Category: "pending".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Pending
        {
            get { return new TestOutcome(TestStatus.Skipped, "pending"); }
        }

        /// <summary>
        /// Gets a standard outcome for a test that did not run because it must be selected explicitly.
        /// The test may be particularly expensive or require manual supervision by an operator.
        /// </summary>
        /// <remarks>
        /// Status: <see cref="TestStatus.Skipped"/>.
        /// Category: "explicit".
        /// </remarks>
        /// <returns>The outcome</returns>
        public static TestOutcome Explicit
        {
            get { return new TestOutcome(TestStatus.Skipped, "explicit"); }
        }
        #endregion

        #region Xml Serialization
        /* Note: We implement out own Xml serialization so that the outcome object can still appear to be immutable.
                 since we don't need any property setters unlike if we were using [XmlAttribute] attributes. */
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            status = StatusFromString(reader.GetAttribute(@"status"));
            category = reader.GetAttribute(@"category");

            if (category != null && category.Length == 0)
                category = null;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(@"status", StatusToString(status));

            if (category != null)
                writer.WriteAttributeString(@"category", category);
        }

        private static string StatusToString(TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Skipped:
                    return "skipped";
                case TestStatus.Passed:
                    return "passed";
                case TestStatus.Failed:
                    return "failed";
                case TestStatus.Inconclusive:
                    return "inconclusive";
                default:
                    throw new ArgumentException("Invalid status code.", "status");
            }
        }

        private static TestStatus StatusFromString(string status)
        {
            switch (status)
            {
                case "skipped":
                    return TestStatus.Skipped;
                case "passed":
                    return TestStatus.Passed;
                case "failed":
                    return TestStatus.Failed;
                case "inconclusive":
                    return TestStatus.Inconclusive;
                default:
                    throw new ArgumentException("Invalid status code.", "status");
            }
        }
        #endregion

        #region Equality
        /// <summary>
        /// Compares two outcomes for equality.
        /// </summary>
        /// <param name="a">The first outcome</param>
        /// <param name="b">The second outcome</param>
        /// <returns>True if the outcomes are equal</returns>
        public static bool operator ==(TestOutcome a, TestOutcome b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two outcomes for inequality.
        /// </summary>
        /// <param name="a">The first outcome</param>
        /// <param name="b">The second outcome</param>
        /// <returns>True if the outcomes are not equal</returns>
        public static bool operator !=(TestOutcome a, TestOutcome b)
        {
            return ! a.Equals(b);
        }

        /// <inheritdoc />
        public bool Equals(TestOutcome other)
        {
            return status == other.status
                && category == other.category;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TestOutcome && Equals((TestOutcome)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return status.GetHashCode()
                ^ (category != null ? category.GetHashCode() : 0);
        }
        #endregion
    }
}