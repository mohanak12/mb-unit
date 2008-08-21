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
using System.Collections.ObjectModel;
using Gallio.Collections;
using Gallio.Model.Logging;
using Gallio.Model.Logging.Tags;

namespace Gallio.Model.Logging
{
    /// <summary>
    /// <para>
    /// A structured text object is an immutable value type that wraps a <see cref="StructuredTestLogStream" />.
    /// It encapsulates a fragment of a structured log such that it can be written back to a test log
    /// stream later on.
    /// </para>
    /// <para>
    /// Structured text is emitted by a <see cref="StructuredTextWriter" />.
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class StructuredText : IEquatable<StructuredText>
    {
        private readonly BodyTag bodyTag;
        private readonly IList<Attachment> attachments;

        /// <summary>
        /// Creates a simple structured text object over a plain text string.
        /// </summary>
        /// <param name="text">The text string</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null</exception>
        public StructuredText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            bodyTag = new BodyTag();
            bodyTag.Contents.Add(new TextTag(text));

            attachments = EmptyArray<Attachment>.Instance;
        }

        /// <summary>
        /// Creates a structured text object that wraps the body tag of a structured test log stream
        /// and no attachments.
        /// </summary>
        /// <param name="bodyTag">The body tag to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bodyTag"/> is null</exception>
        public StructuredText(BodyTag bodyTag)
            : this(bodyTag, EmptyArray<Attachment>.Instance)
        {
        }

        /// <summary>
        /// Creates a structured text object that wraps the body tag of a structured test log stream
        /// and a list of attachments.
        /// </summary>
        /// <param name="bodyTag">The body tag to wrap</param>
        /// <param name="attachments">The list of attachments</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bodyTag"/>,
        /// or <paramref name="attachments"/> is null</exception>
        public StructuredText(BodyTag bodyTag, IList<Attachment> attachments)
        {
            if (bodyTag == null)
                throw new ArgumentNullException("bodyTag");
            if (attachments == null)
                throw new ArgumentNullException("attachments");

            this.bodyTag = bodyTag;
            this.attachments = attachments;
        }

        /// <summary>
        /// Gets a copy of the body tag that described the structured text.
        /// </summary>
        public BodyTag BodyTag
        {
            get { return bodyTag.Clone(); }
        }

        /// <summary>
        /// Gets the immutable list of attachments.
        /// </summary>
        public IList<Attachment> Attachments
        {
            get { return new ReadOnlyCollection<Attachment>(attachments); }
        }

        /// <summary>
        /// Writes the structured text to a test log stream writer.
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is null</exception>
        public void WriteTo(TestLogStreamWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            foreach (Attachment attachment in attachments)
                writer.Container.Attach(attachment);

            bodyTag.WriteTo(writer);
        }

        /// <summary>
        /// Formats the structured text to a string, discarding unrepresentable formatting details.
        /// </summary>
        /// <returns>The structured text as a string</returns>
        public override string ToString()
        {
            return bodyTag.ToString();
        }

        /// <inheritdoc />
        public bool Equals(StructuredText other)
        {
            return other != null
                && GenericUtils.ElementsEqual(attachments, other.attachments)
                && bodyTag.Equals(other.bodyTag);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as StructuredText);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return attachments.Count ^ bodyTag.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(StructuredText a, StructuredText b)
        {
            return ReferenceEquals(a, b) || ! ReferenceEquals(a, null) && a.Equals(b);
        }

        /// <inheritdoc />
        public static bool operator !=(StructuredText a, StructuredText b)
        {
            return !(a == b);
        }
    }
}
