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

namespace MbUnit.Framework.Core.Attributes
{
    /// <summary>
    /// The data pattern attribute applies a data source to a fixture or test
    /// parameter declaratively.  It can be attached to a fixture class, a public property
    /// or field of a fixture, a test method or a test method parameter.  When attached
    /// to a property or field of a fixture, implies that the property or field is
    /// a fixture parameter (so the <see cref="ParameterPatternAttribute" />
    /// may be omitted).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
        | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
        | AttributeTargets.Parameter, AllowMultiple=true, Inherited=true)]
    public abstract class DataPatternAttribute : PatternAttribute
    {
        private string condition;

        /// <summary>
        /// Gets or sets the name of the condition token associated with a condition to
        /// evaluate to decide whether to use the data specified by this attribute.
        /// <seealso cref="ConditionPatternAttribute" />.
        /// </summary>
        public string Condition
        {
            get { return condition; }
            set { condition = value; }
        }
    }
}
