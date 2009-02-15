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
using System.Collections.Generic;
using Gallio.Model;
using Gallio.Framework.Pattern;

namespace MbUnit.Framework
{
    /// <summary>
    /// Associates a category name with a test fixture, test method, test parameter
    /// or other test component.  The category name can be used to classify tests
    /// and build test suites of related tests.
    /// </summary>
    public class CategoryAttribute : MetadataPatternAttribute
    {
        private readonly string categoryName;

        /// <summary>
        /// Associates a category name with the test component annotated by this attribute.
        /// </summary>
        /// <param name="categoryName">The category name to associate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="categoryName"/> is null</exception>
        public CategoryAttribute(string categoryName)
        {
            if (categoryName == null)
                throw new ArgumentNullException(@"categoryName");

            this.categoryName = categoryName;
        }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string CategoryName
        {
            get { return categoryName; }
        }

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<string, string>> GetMetadata()
        {
            yield return new KeyValuePair<string, string>(MetadataKeys.CategoryName, categoryName);
        }
    }
}