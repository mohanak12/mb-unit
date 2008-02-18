// Copyright 2008 MbUnit Project - http://www.mbunit.com/
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
using System.Collections.Generic;
using Gallio.Collections;
using Gallio.Reflection;

namespace Gallio.Framework.Data.Binders
{
    /// <summary>
    /// An object data binder creates objects and binds values to its slots such as
    /// generic type parameters, constructor parameters, fields and properties.
    /// </summary>
    public class ObjectDataBinder : BaseDataBinder
    {
        private readonly ITypeInfo type;
        private readonly Dictionary<ISlotInfo, IDataBinder> slotBinders;

        /// <summary>
        /// Creates an object data binder initially without slot binders.
        /// </summary>
        /// <param name="type">The type</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null</exception>
        public ObjectDataBinder(ITypeInfo type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.type = type;

            slotBinders = new Dictionary<ISlotInfo, IDataBinder>();
        }

        /// <summary>
        /// Sets the binder for a slot.
        /// </summary>
        /// <param name="slot">The slot</param>
        /// <param name="binder">The binder</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="slot"/>
        /// or <paramref name="binder"/> is null</exception>
        public void SetSlotBinder(ISlotInfo slot, IDataBinder binder)
        {
            if (slot == null)
                throw new ArgumentNullException("slot");
            if (binder == null)
                throw new ArgumentNullException("binder");

            slotBinders[slot] = binder;
        }

        /// <inheritdoc />
        protected override IDataBindingAccessor RegisterInternal(DataBindingContext context, IDataSourceResolver resolver)
        {
            List<KeyValuePair<ISlotInfo, IDataBindingAccessor>> slotAccessors = new List<KeyValuePair<ISlotInfo,IDataBindingAccessor>>(slotBinders.Count);

            foreach (KeyValuePair<ISlotInfo, IDataBinder> slotBinder in slotBinders)
            {
                IDataBindingAccessor accessor = slotBinder.Value.Register(context, resolver);
                slotAccessors.Add(new KeyValuePair<ISlotInfo, IDataBindingAccessor>(slotBinder.Key, accessor));
            }

            return new Accessor(type, slotAccessors);
        }

        private sealed class Accessor : IDataBindingAccessor
        {
            private readonly ITypeInfo type;
            private readonly List<KeyValuePair<ISlotInfo, IDataBindingAccessor>> slotAccessors;

            public Accessor(ITypeInfo type, List<KeyValuePair<ISlotInfo, IDataBindingAccessor>> slotAccessors)
            {
                this.type = type;
                this.slotAccessors = slotAccessors;
            }

            public object GetValue(DataBindingItem item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                KeyValuePair<ISlotInfo, object>[] slotValues = GenericUtils.ConvertAllToArray<
                    KeyValuePair<ISlotInfo, IDataBindingAccessor>, KeyValuePair<ISlotInfo, object>>(slotAccessors,
                    delegate(KeyValuePair<ISlotInfo, IDataBindingAccessor> slotAccessor)
                    {
                        object value = slotAccessor.Value.GetValue(item);
                        return new KeyValuePair<ISlotInfo, object>(slotAccessor.Key, value);
                    });

                return SlotBinder.CreateInstance(type, slotValues);
            }
        }
    }
}
