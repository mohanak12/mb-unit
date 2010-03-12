// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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
using Gallio.Model.Schema;
using Gallio.Common.Reflection;
using Gallio.Runner.Reports.Schema;
using Gallio.Tests;
using MbUnit.Framework;
using Gallio.Common.Xml;
using Rhino.Mocks;

namespace Gallio.Tests.Common.Xml
{
    [TestFixture]
    [TestsOn(typeof(MarkupComment))]
    public class MarkupCommentTest : DiffableTestBase
    {
        [Test]
        [ExpectedArgumentOutOfRangeException]
        public void Constructs_with_negative_index_should_throw_exception()
        {
            new MarkupComment(-1, "blah blah");
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Constructs_with_null_value_should_throw_exception()
        {
            new MarkupComment(0, null);
        }

        [Test]
        public void Constructs_ok([Column(0, 123)] int index)
        {
            var comment = new MarkupComment(index, " Jack Burton ");
            Assert.AreEqual(index, comment.Index);
            Assert.IsEmpty(comment.Children);
            Assert.AreEqual(" Jack Burton ", comment.Text);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Diff_with_null_expected_value_should_throw_exception()
        {
            var actual = new MarkupComment(123, "Text");
            actual.Diff(null, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Diff_with_null_path_should_throw_exception()
        {
            var actual = new MarkupComment(123, "Text");
            var expected = new MarkupComment(123, "Text");
            actual.Diff(expected, null, XmlOptions.Strict.Value);
        }

        [Test]
        public void Diff_equal_with_different_index()
        {
            var actual = new MarkupComment(123, "Text");
            var expected = new MarkupComment(456, "Text");
            var diff = actual.Diff(expected, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
            Assert.IsTrue(diff.IsEmpty);
        }

        [Test]
        public void Diff_equal_with_same_index()
        {
            var actual = new MarkupComment(123, "Text");
            var expected = new MarkupComment(123, "Text");
            var diff = actual.Diff(expected, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
            Assert.IsTrue(diff.IsEmpty);
        }

        [Test]
        public void Diff_with_value_differing_by_value()
        {
            var actual = new MarkupComment(123, "Text");
            var expected = new MarkupComment(123, "SomeOtherText");
            var diff = actual.Diff(expected, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
            AssertDiff(diff, new Diff("Unexpected comment found.", XmlPathRoot.Strict.Empty.Element(123), DiffTargets.Both));
        }

        [Test]
        public void Diff_with_value_differing_by_case()
        {
            var actual = new MarkupComment(123, "Text");
            var expected = new MarkupComment(123, "TEXT");
            var diff = actual.Diff(expected, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
            AssertDiff(diff, new Diff("Unexpected comment found.", XmlPathRoot.Strict.Empty.Element(123), DiffTargets.Both));
        }

        [Test]
        public void Diff_with_items_differing_by_type()
        {
            var actual = new MarkupComment(123, "TEXT");
            var expected = MockRepository.GenerateStub<IMarkup>();
            var diff = actual.Diff(expected, XmlPathRoot.Strict.Empty, XmlOptions.Strict.Value);
            AssertDiff(diff, new Diff("Unexpected comment markup.", XmlPathRoot.Strict.Empty.Element(123), DiffTargets.Actual));
        }
    }
}
