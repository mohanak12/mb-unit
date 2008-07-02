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
using System.Text;

namespace Gallio.Runner.Reports
{
    /// <summary>
    /// <para>
    /// Formats <see cref="ExecutionLogStreamTag" /> instances to plain text.
    /// </para>
    /// <para>
    /// Text tags are written as-is.  Sections introduce paragraph breaks with
    /// the header written out as the first line.  Embedded attachments are
    /// described by name.
    /// </para>
    /// </summary>
    public sealed class ExecutionLogStreamTextFormatter : IExecutionLogStreamTagVisitor
    {
        private readonly StringBuilder textBuilder = new StringBuilder();
        private int pendingSpacing;
        private int actualSpacing;

        /// <summary>
        /// Gets the text that has been built.
        /// </summary>
        public string Text
        {
            get { return textBuilder.ToString(); }
        }

        /// <inheritdoc />
        public void VisitBodyTag(ExecutionLogStreamBodyTag tag)
        {
            RequestMinimumSpacing(2);
            tag.AcceptContents(this);
            RequestMinimumSpacing(2);
        }

        /// <inheritdoc />
        public void VisitSectionTag(ExecutionLogStreamSectionTag tag)
        {
            RequestMinimumSpacing(2);
            Append(tag.Name);
            RequestMinimumSpacing(1);
            tag.AcceptContents(this);
            RequestMinimumSpacing(2);
        }

        /// <inheritdoc />
        public void VisitEmbedTag(ExecutionLogStreamEmbedTag tag)
        {
            RequestMinimumSpacing(1);
            Append(String.Format("[Attachment: {0}]", tag.AttachmentName));
            RequestMinimumSpacing(1);
        }

        /// <inheritdoc />
        public void VisitTextTag(ExecutionLogStreamTextTag tag)
        {
            Append(tag.Text);
        }

        private void RequestMinimumSpacing(int spacing)
        {
            pendingSpacing = Math.Max(pendingSpacing, spacing);
        }

        private void Append(string text)
        {
            int length = text.Length;
            if (length == 0)
                return;

            if (pendingSpacing != 0)
            {
                if (textBuilder.Length != 0 && pendingSpacing > actualSpacing)
                    textBuilder.Append('\n', pendingSpacing - actualSpacing);

                pendingSpacing = 0;
            }

            textBuilder.EnsureCapacity(textBuilder.Length + length);

            for (int i = 0; i < length; i++)
            {
                char c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                    actualSpacing += 1;
                else
                    actualSpacing = 0;

                textBuilder.Append(c);
            }
        }
    }
}
