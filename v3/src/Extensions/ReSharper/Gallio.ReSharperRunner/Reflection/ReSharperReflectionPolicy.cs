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
using Gallio.Reflection.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;

namespace Gallio.ReSharperRunner.Reflection
{
    /// <summary>
    /// Base class for ReSharper reflection policies.
    /// Provides additional support for getting the <see cref="IProject" /> and <see cref="IDeclaredElement" />
    /// associated with a code element, if available.
    /// </summary>
    public abstract class ReSharperReflectionPolicy : StaticReflectionPolicy
    {
        /// <summary>
        /// Gets the declared element associated with the code element, or null if none.
        /// </summary>
        public static IDeclaredElement GetDeclaredElement(ICodeElementInfo codeElement)
        {
            StaticWrapper element = codeElement as StaticWrapper;
            if (element == null)
                return null;

            ReSharperReflectionPolicy policy = element.Policy as ReSharperReflectionPolicy;
            return policy != null ? policy.GetDeclaredElement(element) : null;
        }

        /// <summary>
        /// Gets the project to which a code element belongs, or null if none.
        /// </summary>
        public static IProject GetProject(ICodeElementInfo codeElement)
        {
            StaticWrapper element = codeElement as StaticWrapper;
            if (element == null)
                return null;

            ReSharperReflectionPolicy policy = element.Policy as ReSharperReflectionPolicy;
            return policy != null ? policy.GetProject(element) : null;
        }

        /// <summary>
        /// Gets the declared element associated with the code element, or null if none.
        /// </summary>
        protected abstract IDeclaredElement GetDeclaredElement(StaticWrapper element);

        /// <summary>
        /// Gets the project to which a code element belongs, or null if none.
        /// </summary>
        protected abstract IProject GetProject(StaticWrapper element);
    }
}
