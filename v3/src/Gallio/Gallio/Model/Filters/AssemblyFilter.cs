// Copyright 2007 MbUnit Project - http://www.mbunit.com/
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
using System.Reflection;
using Gallio.Model;

namespace Gallio.Model.Filters
{
    /// <summary>
    /// A filter that matches objects whose <see cref="IModelComponent.CodeReference" />
    /// matches the specified assembly name filter.
    /// </summary>
    [Serializable]
    public class AssemblyFilter<T> : BasePropertyFilter<T> where T : IModelComponent
    {
        /// <summary>
        /// Creates an assembly filter.
        /// </summary>
        /// <param name="assemblyNameFilter">A filter for the assembly name that is used
        /// to match one of the following values:
        /// <list type="bullet">
        /// <item>Full name as returned by <see cref="Assembly.FullName" /></item>
        /// <item>Simple name (aka. partial name) as returned by <see cref="AssemblyName.Name" /></item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyNameFilter"/> is null</exception>
        public AssemblyFilter(Filter<string> assemblyNameFilter)
            :  base(assemblyNameFilter)
        {
        }

        /// <inheritdoc />
        public override bool IsMatch(T value)
        {
            string assemblyName = value.CodeReference.AssemblyName;
            if (assemblyName == null)
                return false;

            return ValueFilter.IsMatch(assemblyName) ||
                ValueFilter.IsMatch(GetSimpleName(assemblyName));
        }

        private static string GetSimpleName(string assemblyName)
        {
            AssemblyName name = new AssemblyName(assemblyName);
            return name.Name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Assembly(" + ValueFilter + ")";
        }
    }
}