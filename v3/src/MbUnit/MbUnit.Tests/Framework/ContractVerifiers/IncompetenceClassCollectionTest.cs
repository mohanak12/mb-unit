﻿// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
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
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Gallio.Collections;

namespace MbUnit.Tests.Framework.ContractVerifiers
{
    [TestFixture]
    public class IncompetenceClassCollectionTest
    {
        [Test]
        public void Constructs_empty()
        {
            var collection = new IncompetenceClassCollection<int>();
            Assert.IsEmpty(collection);
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Adds_null_invalid_class_should_throw_exception()
        {
            var collection = new IncompetenceClassCollection<int>();
            collection.Add((IncompetenceClass<int>)null);
        }

        [Test]
        [ExpectedArgumentException]
        public void Adds_several_times_classes_associated_with_same_exception_type_should_throw_exception()
        {
            var collection = new IncompetenceClassCollection<int>();
            collection.Add(new IncompetenceClass<int>(typeof(ArgumentOutOfRangeException)));
            collection.Add(new IncompetenceClass<int>(typeof(ArgumentOutOfRangeException)));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void Adds_null_exception_type_should_throw_exception()
        {
            var collection = new IncompetenceClassCollection<int>();
            collection.Add(null, 1, 2, 3);
        }

        [Test]
        [ExpectedArgumentException]
        public void Adds_several_times_classes_arguments_associated_with_same_exception_type_should_throw_exception()
        {
            var collection = new IncompetenceClassCollection<int>();
            collection.Add(typeof(ArgumentOutOfRangeException), 1, 2, 3);
            collection.Add(typeof(ArgumentOutOfRangeException), 4, 5, 6);
        }

        [Test]
        public void Adds_classes_Ok()
        {
            var collection = new IncompetenceClassCollection<int>();
            collection.Add(typeof(ArgumentOutOfRangeException), 1, 2, 3);
            collection.Add(new IncompetenceClass<int>(typeof(ArgumentException)) { 4, 5, 6 });
            collection.Add(typeof(InvalidOperationException), 7, 8, 9);
            Assert.AreEqual(3, collection.Count);
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(ArgumentOutOfRangeException), typeof(ArgumentException), typeof(InvalidOperationException) }, collection.Select(x => x.ExpectedExceptionType));        
        }
    }
}
