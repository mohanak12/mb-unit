// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using Gallio.Model;

namespace Gallio.Framework.Data
{
    /// <summary>
    /// Extension methods for <see cref="IDataRow" />.
    /// </summary>
    public static class DataRowExtensions
    {
        /// <summary>
        /// Gets the metadata associated with a data row.
        /// </summary>
        /// <param name="dataRow">The data row</param>
        /// <returns>The associated metadata</returns>
        public static MetadataMap GetMetadata(this IDataRow dataRow)
        {
            MetadataMap map = new MetadataMap();
            dataRow.PopulateMetadata(map);
            return map;
        }
    }
}
