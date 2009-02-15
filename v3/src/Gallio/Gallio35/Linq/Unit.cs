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

namespace Gallio.Linq
{
    /// <summary>
    /// A value type that represents the result of evaluating an
    /// expression of type <see cref="System.Void" />.
    /// </summary>
    public struct Unit
    {
        /// <summary>
        /// Gets the singular value of the <see cref="Unit" /> type.
        /// </summary>
        public static readonly Unit Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return "Void";
        }
    }
}
