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
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Gallio.Logging;
using Gallio.Model.Serialization;

namespace Gallio.Runner.Reports
{
    /// <summary>
    /// <para>
    /// An Xml-serializable attachment.
    /// </para>
    /// <para>
    /// The contents of the attachment are embedded in the execution log according to
    /// their encoding.  Text and Xml markup are directly embedded into the Xml whereas
    /// binary attachments are base 64 encoded.
    /// </para>
    /// </summary>
    [Serializable]
    [XmlType(Namespace = SerializationUtils.XmlNamespace)]
    public sealed class ExecutionLogAttachment : IAttachmentVisitor
    {
        private string name;
        private string contentType;
        private ExecutionLogAttachmentEncoding encoding;
        private ExecutionLogAttachmentContentDisposition contentDisposition;
        private string innerText;
        private string contentPath;
        private Attachment contents;

        /// <summary>
        /// Creates an uninitialized instance for Xml deserialization.
        /// </summary>
        private ExecutionLogAttachment()
        {
        }

        /// <summary>
        /// Creates a fully initialized attachment.
        /// </summary>
        /// <param name="attachment">The attachment</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="attachment"/> is null</exception>
        public ExecutionLogAttachment(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            this.contents = attachment;

            name = attachment.Name;
            contentType = attachment.ContentType;
        }

        /// <summary>
        /// Creates a fully initialized attachment.
        /// </summary>
        /// <param name="name">The attachment name</param>
        /// <param name="contentType">The content type</param>
        /// <param name="encoding">The content encoding</param>
        /// <param name="innerText">The inner text or "" if none</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="contentType"/> is null</exception>
        public ExecutionLogAttachment(string name, string contentType,
            ExecutionLogAttachmentEncoding encoding, string innerText)
        {
            if (name == null)
                throw new ArgumentNullException(@"name");
            if (contentType == null)
                throw new ArgumentNullException(@"contentType");

            this.name = name;
            this.contentType = contentType;
            this.encoding = encoding;
            this.innerText = innerText;
        }

        /// <summary>
        /// Gets or sets the deserialized attachment contents.
        /// </summary>
        [XmlIgnore]
        public Attachment Contents
        {
            get
            {
                EnsureAttachmentDeserialized();
                return contents;
            }
            set { contents = value; }
        }

        /// <summary>
        /// Gets or sets the name of the attachment, not null.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the content type of the attachment as a MIME type, not null.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        [XmlAttribute("contentType")]
        public string ContentType
        {
            get { return contentType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoding of the attachment.
        /// This value specifies how the attachment is represented in Xml.
        /// </summary>
        [XmlAttribute("encoding")]
        public ExecutionLogAttachmentEncoding Encoding
        {
            get
            {
                EnsureAttachmentSerialized();
                return encoding;
            }
            set { encoding = value; }
        }

        /// <summary>
        /// Gets or sets the path of the attachment contents relative to
        /// the directory that contains the Xml serialized report,
        /// or null if the content is embedded.
        /// </summary>
        [XmlAttribute("contentPath")]
        public string ContentPath
        {
            get { return contentPath; }
            set { contentPath = value; }
        }

        /// <summary>
        /// Gets or sets the content disposition of the attachment which
        /// indicates how the attachment has been stored.
        /// </summary>
        [XmlAttribute("contentDisposition")]
        public ExecutionLogAttachmentContentDisposition ContentDisposition
        {
            get { return contentDisposition; }
            set { contentDisposition = value; }
        }

        /// <summary>
        /// Gets or sets the attachment content serialized as text (including Base64 attachments), possibly null if none.
        /// </summary>
        [XmlText]
        public string InnerText
        {
            get
            {
                EnsureAttachmentSerialized();
                return innerText;
            }
            set { innerText = value; }
        }

        private void EnsureAttachmentSerialized()
        {
            if (innerText == null)
            {
                if (contents == null)
                    throw new InvalidOperationException("The attachment property is not initialized so its contents cannot be serialized.");

                contents.Accept(this);
            }
        }

        private void EnsureAttachmentDeserialized()
        {
            if (contents == null)
            {
                if (name == null)
                    throw new InvalidOperationException("The attachment is missing its name attribute.");
                if (contentType == null)
                    throw new InvalidOperationException("The attachment is missing its contentType attribute.");
                if (innerText == null)
                    throw new InvalidOperationException("The attachment is missing its text contents.");

                switch (encoding)
                {
                    case ExecutionLogAttachmentEncoding.Text:
                        if (innerText == null)
                            throw new XmlException("The text encoded attachment is missing its text contents.");

                        contents = new TextAttachment(name, contentType, innerText);
                        break;

                    case ExecutionLogAttachmentEncoding.Base64:
                        if (innerText == null)
                            throw new XmlException("The base64 encoded attachment is missing its text contents.");

                        contents = new BinaryAttachment(name, contentType, Convert.FromBase64String(innerText));
                        break;

                    default:
                        throw new XmlException(String.Format(CultureInfo.CurrentCulture,
                            "Unrecognized Xml content encoding '{0}'.", encoding));
                }
            }
        }

        void IAttachmentVisitor.VisitTextAttachment(TextAttachment attachment)
        {
            encoding = ExecutionLogAttachmentEncoding.Text;
            innerText = attachment.Text;
        }

        void IAttachmentVisitor.VisitBinaryAttachment(BinaryAttachment attachment)
        {
            encoding = ExecutionLogAttachmentEncoding.Base64;
            innerText = Convert.ToBase64String(attachment.Data, Base64FormattingOptions.None);
        }
    }
}
