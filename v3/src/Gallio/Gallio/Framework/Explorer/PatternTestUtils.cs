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
using System.Reflection;
using Gallio.Reflection;
using Gallio.Logging;
using Gallio.Model;

namespace Gallio.Framework.Explorer
{
    /// <summary>
    /// Utility functions for populating <see cref="PatternTest" /> objects.
    /// </summary>
    /// <seealso cref="PatternTestFramework"/>
    public static class PatternTestUtils
    {
        /// <summary>
        /// Creates an action that invokes a method on the fixture without parameters.
        /// </summary>
        /// <remarks>
        /// If the method throws an exception when the action runs, it is wrapped up 
        /// and rethrown as a <see cref="TargetInvocationException" />.
        /// </remarks>
        /// <param name="method">The method to invoke</param>
        /// <returns>The action</returns>
        public static Action<PatternTestState> CreateFixtureMethodInvoker(IMethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(@"method");

            return delegate(PatternTestState state)
            {
                MethodInfo resolvedMethod = method.Resolve(true);
                if (resolvedMethod.IsStatic)
                {
                    resolvedMethod.Invoke(null, null);
                }
                else
                {
                    object instance = state.FixtureInstance;
                    if (instance == null)
                        throw new ModelException("Expected to invoke an instance method of a fixture but the fixture instance is not available.");

                    resolvedMethod.Invoke(instance, null);
                }
            };
        }
    }
}