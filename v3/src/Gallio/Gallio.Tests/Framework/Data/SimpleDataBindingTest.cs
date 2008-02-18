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

using Gallio.Framework.Data;
using MbUnit.Framework;

namespace Gallio.Tests.Framework.Data
{
    [TestFixture]
    [TestsOn(typeof(SimpleDataBinding))]
    public class SimpleDataBindingTest
    {
        [Test]
        public void ConstructorWithValueTypeOnly()
        {
            SimpleDataBinding binding = new SimpleDataBinding(typeof(int));

            Assert.AreEqual(typeof(int), binding.Type);
            Assert.IsNull(binding.Path);
            Assert.IsNull(binding.Index);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void ConstructorWithValueTypeOnly_ThrowsIfValueTypeIsNull()
        {
            new SimpleDataBinding(null);
        }

        [Test]
        public void ConstructorWithPathAndIndex()
        {
            SimpleDataBinding binding = new SimpleDataBinding(typeof(int), "path", 42);

            Assert.AreEqual(typeof(int), binding.Type);
            Assert.AreEqual("path", binding.Path);
            Assert.AreEqual(42, binding.Index);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void ConstructorWithPathAndIndex_ThrowsIfValueTypeIsNull()
        {
            new SimpleDataBinding(null, "path", 42);
        }

        [Test]
        public void ReplaceIndexCreatesANewInstanceWithTheNewIndex()
        {
            SimpleDataBinding oldBinding = new SimpleDataBinding(typeof(int), "path", 42);
            DataBinding newBinding = oldBinding.ReplaceIndex(23);

            Assert.AreNotSame(oldBinding, newBinding);

            Assert.AreEqual(typeof(int), newBinding.Type);
            Assert.AreEqual("path", newBinding.Path);
            Assert.AreEqual(23, newBinding.Index);
        }

        [Test]
        new public void ToString()
        {
            Assert.AreEqual("Binding ValueType: System.Int32, Path: <null>, Index: <null>",
                new SimpleDataBinding(typeof(int)).ToString());
            Assert.AreEqual("Binding ValueType: System.Int32, Path: 'foo', Index: 42",
                new SimpleDataBinding(typeof(int), "foo", 42).ToString());
        }

        [Test]
        public void EqualsAndHashCodeAreEqualForEqualBindings()
        {
            Assert.AreEqual(
                new SimpleDataBinding(typeof(int), "path", 1),
                new SimpleDataBinding(typeof(int), "path", 1));
            Assert.AreEqual(
                new SimpleDataBinding(typeof(int), "path", 1).GetHashCode(),
                new SimpleDataBinding(typeof(int), "path", 1).GetHashCode());
        }

        [Test]
        public void EqualsAndHashCodeAreDistinctForDifferentBindings()
        {
            InterimAssert.AreDistinct(
                new SimpleDataBinding(typeof(string), null, null),
                new SimpleDataBinding(typeof(int), null, null),
                new SimpleDataBinding(typeof(string), null, 0),
                new SimpleDataBinding(typeof(string), null, 1),
                new SimpleDataBinding(typeof(string), "path", null),
                new SimpleDataBinding(typeof(string), "path2", null),
                new SimpleDataBinding(typeof(string), "path", 0),
                new SimpleDataBinding(typeof(string), "path2", 1),
                null
            );
        }
    }
}