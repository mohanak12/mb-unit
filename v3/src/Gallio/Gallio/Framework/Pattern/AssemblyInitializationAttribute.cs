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
using Gallio.Framework.Pattern;
using Gallio.Reflection;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// <para>
    /// An assembly initialization attribute gets a chance to perform early initialization
    /// of the system before enumerating the tests within the assembly.
    /// </para>
    /// <para>
    /// For examples, a subclass of this attribute may be used to register an assembly
    /// resolver before test enumeration occurs to ensure that all referenced assemblies
    /// can be loaded.
    /// </para>
    /// </summary>
    public abstract class AssemblyInitializationAttribute : Attribute
    {
        /// <summary>
        /// Performs early initialization for the specified assembly.
        /// </summary>
        /// <param name="topLevelTestBuilder">The top level test builder that will contain
        /// the assembly-level test</param>
        /// <param name="assembly">The assembly to process</param>
        public abstract void Initialize(IPatternTestBuilder topLevelTestBuilder, IAssemblyInfo assembly);
    }
}
