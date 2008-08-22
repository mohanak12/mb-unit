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
using System.Xml.Serialization;
using Gallio.Collections;
using Gallio.Utilities;

namespace Gallio.Model.Logging.Tags
{
    /// <summary>
    /// The top-level container tag of structured text.
    /// </summary>
    [Serializable]
    [XmlRoot("body", Namespace = XmlSerializationUtils.GallioNamespace)]
    [XmlType(Namespace = XmlSerializationUtils.GallioNamespace)]
    public sealed class BodyTag : ContainerTag, ICloneable<BodyTag>, IEquatable<BodyTag>
    {
        /// <inheritdoc />
        new public BodyTag Clone()
        {
            BodyTag copy = new BodyTag();
            CopyTo(copy);
            return copy;
        }

        /// <inheritdoc />
        public bool Equals(BodyTag other)
        {
            return other != null
                && GenericUtils.ElementsEqual(Contents, other.Contents);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BodyTag);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 1 ^ Contents.Count;
        }

        internal override Tag CloneImpl()
        {
            return Clone();
        }

        internal override void AcceptImpl(ITagVisitor visitor)
        {
            visitor.VisitBodyTag(this);
        }
    }
}
