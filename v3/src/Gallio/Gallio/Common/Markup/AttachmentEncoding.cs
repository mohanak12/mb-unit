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

using System;
using System.Xml.Serialization;

namespace Gallio.Common.Markup
{
    /// <summary>
    /// Specifies the Xml-encoding of the contents of an embedded attachment.
    /// </summary>
    public enum AttachmentEncoding
    {
        /// <summary>
        /// The attachment is encoded as a text string.
        /// </summary>
        [XmlEnum("text")]
        Text,

        /// <summary>
        /// The attachment is encoded as base 64 text string.
        /// </summary>
        [XmlEnum("base64")]
        Base64
    }
}