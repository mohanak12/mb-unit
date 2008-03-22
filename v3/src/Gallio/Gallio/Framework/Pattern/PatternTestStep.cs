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
using Gallio.Model;
using Gallio.Reflection;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// A step that belongs to a <see cref="PatternTest" />.
    /// </summary>
    /// <seealso cref="PatternTestFramework"/>
    public class PatternTestStep : BaseTestStep
    {
        /// <summary>
        /// Creates a step.
        /// </summary>
        /// <param name="test">The test to which the step belongs</param>
        /// <param name="parent">The parent step, or null if creating a root step</param>
        /// <param name="name">The step name</param>
        /// <param name="codeElement">The point of definition of the step, or null if unknown</param>
        /// <param name="isPrimary">True if the test step is primary</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>
        /// or <paramref name="test"/> is null</exception>
        public PatternTestStep(PatternTest test, ITestStep parent, string name, ICodeElementInfo codeElement, bool isPrimary)
            : base(test, parent, name, codeElement, isPrimary)
        {
        }

        /// <summary>
        /// Gets the associated test.
        /// </summary>
        public new PatternTest Test
        {
            get { return (PatternTest)base.Test; }
        }
    }
}
