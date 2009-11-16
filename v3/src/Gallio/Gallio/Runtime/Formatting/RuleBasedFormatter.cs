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
using System.Collections.Generic;
using Gallio.Common;
using Gallio.Runtime.Conversions;

namespace Gallio.Runtime.Formatting
{
    /// <summary>
    /// A rule-based formatter uses a set of <see cref="IFormattingRule" />s to
    /// format values appropriately.
    /// </summary>
    public class RuleBasedFormatter : IFormatter
    {
        private readonly List<IFormattingRule> rules;
        private readonly Dictionary<Type, IFormattingRule> preferredRules;

        /// <summary>
        /// Creates a rule-based formatter.
        /// </summary>
        /// <param name="rules">The rules to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rules"/> is null.</exception>
        public RuleBasedFormatter(IFormattingRule[] rules)
        {
            if (rules == null)
                throw new ArgumentNullException("rules");

            this.rules = new List<IFormattingRule>(rules);
            preferredRules = new Dictionary<Type, IFormattingRule>();
        }

        /// <inheritdoc />
        public string Format(object obj)
        {
            if (obj == null)
                return @"null";

            Type type = obj.GetType();
            try
            {
                IFormattingRule rule = GetPreferredRule(type);

                if (rule != null)
                {
                    string result = rule.Format(obj, this);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }
            catch
            {
                // Ignore exceptions.
            }

            return string.Concat("{", type.ToString(), "}");
        }

        private IFormattingRule GetPreferredRule(Type type)
        {
            lock (preferredRules)
            {
                IFormattingRule preferredRule;
                if (! preferredRules.TryGetValue(type, out preferredRule))
                {
                    // Try to get a custom user converter.
                    FormattingFunc operation = CustomFormatters.Find(type);

                    if (operation != null)
                    {
                        preferredRule = new CustomFormattingRule(operation);
                    }
                    else
                    {
                        preferredRule = GetPreferredRuleWithoutCache(type);
                    }

                    preferredRules.Add(type, preferredRule);
                }

                return preferredRule;
            }
        }

        private IFormattingRule GetPreferredRuleWithoutCache(Type type)
        {
            int bestPriority = int.MinValue;
            IFormattingRule bestRule = null;

            foreach (IFormattingRule rule in rules)
            {
                int? priority = rule.GetPriority(type);
                if (priority.HasValue && priority.Value >= bestPriority)
                {
                    bestPriority = priority.Value;
                    bestRule = rule;
                }
            }

            return bestRule;
        }
    }
}
