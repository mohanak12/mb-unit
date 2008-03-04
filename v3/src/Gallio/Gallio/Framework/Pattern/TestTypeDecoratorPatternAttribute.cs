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
using Gallio.Reflection;
using Gallio.Framework.Pattern;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// <para>
    /// A test type decorator pattern attribute applies decorations to an
    /// existing type-level <see cref="PatternTest" />.
    /// </para>
    /// </summary>
    /// <seealso cref="TestTypePatternAttribute"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
        AllowMultiple = true, Inherited = true)]
    public abstract class TestTypeDecoratorPatternAttribute : DecoratorPatternAttribute
    {
        /// <inheritdoc />
        public override void ProcessTest(IPatternTestBuilder testBuilder, ICodeElementInfo codeElement)
        {
            ITypeInfo type = (ITypeInfo)codeElement;

            testBuilder.AddDecorator(Order, delegate(IPatternTestBuilder typeTestBuilder)
            {
                DecorateTest(typeTestBuilder, type);
            });
        }

        /// <summary>
        /// <para>
        /// Applies decorations to a type-level <see cref="PatternTest" />.
        /// </para>
        /// <para>
        /// A typical use of this method is to augment the test with additional metadata
        /// or to add additional behaviors to the test.
        /// </para>
        /// </summary>
        /// <param name="builder">The test builder</param>
        /// <param name="type">The type</param>
        protected virtual void DecorateTest(IPatternTestBuilder builder, ITypeInfo type)
        {
        }
    }
}