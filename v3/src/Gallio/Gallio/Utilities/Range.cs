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
using System.Collections.Generic;

namespace Gallio.Utilities
{
    /// <summary>
    /// A range denotes a segment of a string or other indexed data structure.
    /// </summary>
    [Serializable]
    public struct Range
    {
        private readonly int startIndex;
        private readonly int length;

        /// <summary>
        /// Creates a range of indices.
        /// </summary>
        /// <param name="startIndex">The starting index of the range</param>
        /// <param name="length">The number of characters within the range, may be 0</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startIndex"/>
        /// or <paramref name="length"/> is negative</exception>
        public Range(int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", startIndex, "Index must be non-negative.");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", startIndex, "Length must be non-negative.");

            this.startIndex = startIndex;
            this.length = length;
        }

        /// <summary>
        /// Gets the starting index of the range.
        /// </summary>
        public int StartIndex
        {
            get { return startIndex; }
        }

        /// <summary>
        /// Gets the ending index of the range, which is the start index plus the length.
        /// The ending index is one past the last character within the range.
        /// </summary>
        public int EndIndex
        {
            get { return startIndex + length; }
        }

        /// <summary>
        /// Gets the length of the range.
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Gets a substring of the specified text using this range.
        /// </summary>
        /// <param name="text">The source text</param>
        /// <returns>The substring of the source text that represents this range</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null</exception>
        public string SubstringOf(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            return text.Substring(startIndex, length);
        }

        /// <summary>
        /// Extends a range into an adjacent range and returns the combined range.
        /// </summary>
        /// <param name="range">The adjacent range</param>
        /// <returns>The extended range</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="range"/> is not an adjacent range</exception>
        public Range ExtendWith(Range range)
        {
            if (EndIndex == range.startIndex)
                return new Range(startIndex, length + range.length);
            if (startIndex == range.EndIndex)
                return new Range(range.startIndex, length + range.length);

            throw new ArgumentException("The ranges must be adjacent.", "range");
        }
    }
}
