﻿using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework.Pattern;
using Gallio.Model;
using Gallio.Framework.Data;
using Gallio.Reflection;
using System.Collections;
using Gallio.Framework.Assertions;
using System.Reflection;

namespace MbUnit.Framework.ContractVerifiers.Patterns
{
    /// <summary>
    /// Abstract test pattern for contract verifiers.
    /// </summary>
    public abstract class ContractVerifierPattern
    {
        /// <summary>
        /// Gets the name of the test pattern.
        /// </summary>
        protected abstract string Name
        {
            get;
        }

        /// <summary>
        /// Runs the test pattern action.
        /// </summary>
        /// <param name="state"></param>
        protected internal abstract void Run(IContractVerifierPatternInstanceState state);

        /// <summary>
        /// Builds the test pattern, then adds it 
        /// to the evaluation scope as a new child test.
        /// </summary>
        /// <param name="scope">The scope of the test pattern.</param>
        public void Build(PatternEvaluationScope scope)
        {
            var test = new PatternTest(Name, null, scope.TestDataContext.CreateChild());
            test.Metadata.SetValue(MetadataKeys.TestKind, TestKinds.Test);
            test.IsTestCase = true;
            scope.AddChildTest(test);

            test.TestInstanceActions.BeforeTestInstanceChain.After(
                state =>
                {
                    ObjectCreationSpec spec = state.GetFixtureObjectCreationSpec(scope.Parent.CodeElement as ITypeInfo);
                    state.FixtureType = spec.ResolvedType;
                    state.FixtureInstance = spec.CreateInstance();
                });

            test.TestInstanceActions.ExecuteTestInstanceChain.After(
                state =>
                {
                    Run(new ContractVerifierPatternInstanceState(
                        state.FixtureType, state.FixtureInstance));
                });
        }

        /// <summary>
        /// Helper methods that builds a friendly displayable constructor signature.
        /// </summary>
        /// <param name="types">The parameter types of the constructor.</param>
        /// <returns></returns>
        protected static string GetConstructorSignature(IEnumerable<Type> types)
        {
            StringBuilder output = new StringBuilder(".ctor(");
            bool first = true;

            foreach (var type in types)
            {
                if (!first)
                {
                    output.Append(", ");
                }

                output.Append(type.Name);
                first = false;
            }

            return output.Append(")").ToString();
        }

        /// <summary>
        /// Casts the instance of the test fixture into a provider of equivalence classes, 
        /// then returns the resulting collection as an enumeration.
        /// </summary>
        /// <param name="targetType">The target evaluated type.</param>
        /// <param name="fixtureType">The test fixture type.</param>
        /// <param name="fixtureInstance">The test fixture instance.</param>
        /// <returns></returns>
        protected static IEnumerable GetEquivalentClasses(Type targetType, Type fixtureType, object fixtureInstance)
        {
            Type interfaceType = GetIEquivalenceClassProviderInterface(targetType, fixtureType);

            AssertionHelper.Verify(() =>
            {
                if (interfaceType != null)
                    return null;

                return new AssertionFailureBuilder("Expected the contract verifier to implement a particular interface.")
                    .AddLabeledValue("Contract Verifier", "Equality")
                    .AddLabeledValue("Expected Interface", "IEquivalentClassProvider<" + targetType.Name + ">")
                    .ToAssertionFailure();
            });

            return (IEnumerable)interfaceType.InvokeMember("GetEquivalenceClasses",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null, fixtureInstance, null);
        }

        /// <summary>
        /// Gets the interface of a particular type if it is implemented by another type,
        /// otherwise returns null.
        /// </summary>
        /// <param name="implementationType">The implementation type</param>
        /// <param name="interfaceType">The interface type</param>
        /// <returns>The interface type or null if it is not implemented by the implementation type</returns>
        protected static Type GetInterface(Type implementationType, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(implementationType) ? interfaceType : null;
        }

        /// <summary>
        /// Returns the target type as a generic IEquatable interface, or
        /// a null reference if it does not implement such an interface.
        /// </summary>
        /// <param name="targetType">The target evaluated type.</param>
        /// <param name="fixtureType">The test fixture type.</param>
        /// <returns>The interface type or a null reference.</returns>
        protected static Type GetIEquivalenceClassProviderInterface(Type targetType, Type fixtureType)
        {
            return GetInterface(fixtureType, typeof(IEquivalenceClassProvider<>)
                .MakeGenericType(targetType));
        }
    }
}
