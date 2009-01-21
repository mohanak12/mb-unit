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
using System.Reflection;
using Gallio.Framework;
using Gallio.Framework.Assertions;
using Gallio.Framework.Pattern;
using Gallio.Reflection;
using MbUnit.Framework;
using MbUnit.Pex;
using Microsoft.ExtendedReflection.Collections;
using Microsoft.ExtendedReflection.Emit;
using Microsoft.ExtendedReflection.Logging;
using Microsoft.ExtendedReflection.Metadata;
using Microsoft.ExtendedReflection.Metadata.Builders;
using Microsoft.ExtendedReflection.Metadata.Interfaces;
using Microsoft.ExtendedReflection.Metadata.Names;
using Microsoft.ExtendedReflection.Symbols;
using Microsoft.Pex.Engine;
using Microsoft.Pex.Engine.ComponentModel;
using Microsoft.Pex.Engine.TestFrameworks;
using Microsoft.Pex.Framework.Instrumentation;
using Microsoft.Pex.Framework.Suppression;

[assembly: PexSuppressStackFrameFromAssembly("Gallio")]
[assembly: PexSuppressStackFrameFromAssembly("Gallio35")]
[assembly: PexSuppressStackFrameFromAssembly("MbUnit")]
[assembly: PexSuppressStackFrameFromAssembly("MbUnit35")]
[assembly: PexSuppressUninstrumentedMethodFromAssembly("Gallio")]
[assembly: PexSuppressUninstrumentedMethodFromAssembly("Gallio35")]
[assembly: PexSuppressUninstrumentedMethodFromAssembly("MbUnit")]
[assembly: PexSuppressUninstrumentedMethodFromAssembly("MbUnit35")]
[assembly: MbUnitTestFramework(SetAsDefault = true)]

namespace MbUnit.Pex
{
    /// <summary>
    /// Adds support for MbUnit to Pex.
    /// </summary>
    /// <remarks author="jeff">
    /// This implementation only supports the very basic features of MbUnit.
    /// In the future, I hope Pex will adopt a more powerful extensibility interface the puts
    /// more control over test execution into the hands of the test framework.  There's
    /// really no reason Pex should have to know about stuff like how to create a fixture
    /// or which setup and teardown methods to invoke.
    /// It should just delegate back to the framework for pre-test and post-test actions
    /// and test execution.
    /// </remarks>
    [Serializable]
    internal class MbUnitTestFramework : TestFrameworkBase
    {
        private static readonly Assembly GallioAssembly = typeof(PatternTestFramework).Assembly;
        private static readonly Assembly MbUnitAssembly = typeof(Assert).Assembly;

        private static readonly Method TestFixtureAttributeConstructor = Metadata<TestFixtureAttribute>.Type.DefaultConstructor;
        private static readonly Method TestAttributeConstructor = Metadata<TestAttribute>.Type.DefaultConstructor;
        private static readonly Method IgnoreAttributeConstructorWithReason = Metadata<IgnoreAttribute>.Type.GetMethod(".ctor", Metadata<string>.Type);
        private static readonly Method ExpectedExceptionAttributeConstructorWithType = Metadata<ExpectedExceptionAttribute>.Type.GetMethod(".ctor", Metadata<Type>.Type);
        private static readonly Method AssertInconclusiveMethod = MetadataFromReflection.GetType(typeof(Assert)).GetMethod("Inconclusive", new TypeEx[] { SystemTypes.String });
        private static readonly GenericMethod AssertAreEqualMethod = MetadataFromReflection.GetGenericMethod(Array.Find(typeof(Assert).GetMethods(),
            method => method.Name == "AreEqual" && method.GetParameters().Length == 2 && method.IsGenericMethod));

        public MbUnitTestFramework(IPexComponent host)
            : base(host)
        {
        }

        public override bool AttributeBased
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "MbUnit"; }
        }

        public override ShortAssemblyName AssemblyName
        {
            get { return ShortAssemblyName.FromAssembly(MbUnitAssembly); }
        }

        public override bool SupportsPartialClasses
        {
            get { return true; }
        }

        public override bool SupportsTestReflection
        {
            get { return true; }
        }

        public override TypeName AssertionExceptionType
        {
            get { return Metadata<AssertionFailureException>.SerializableName; }
        }

        public override TypeName InconclusiveExceptionType
        {
            get { return Metadata<TestInconclusiveException>.SerializableName; }
        }

        public override IIndexable<ShortAssemblyName> References
        {
            get
            {
                return Indexable.Array(
                    ShortAssemblyName.FromAssembly(GallioAssembly),
                    ShortAssemblyName.FromAssembly(MbUnitAssembly)
                    );
            }
        }

        public override void MarkTestClass(TypeDefinitionBuilder type)
        {
            type.CustomAttributes.Add(new CustomAttributeBuilder(TestFixtureAttributeConstructor));
        }

        public override void MarkTestMethod(PexGeneratedTest test, MethodDefinitionBuilder method)
        {
            method.CustomAttributes.Add(new CustomAttributeBuilder(TestAttributeConstructor));

            CopyRelevantAttributesFromOriginalTest(test.Exploration.Method.Definition, method);
        }

        private static void CopyRelevantAttributesFromOriginalTest(ICustomAttributeProviderEx attributeProvider, MethodDefinitionBuilder method)
        {
            foreach (ICustomAttribute attribute in attributeProvider.GetAttributes(Metadata<MetadataPatternAttribute>.Type, true))
                method.CustomAttributes.Add(attribute);
        }

        public override void MarkIgnored(MethodDefinitionBuilder method, string message)
        {
            method.CustomAttributes.Add(new CustomAttributeBuilder(IgnoreAttributeConstructorWithReason,
                MetadataExpression.String(message)));
        }

        public override void MarkExpectedException(VisibilityContext visibility, MethodDefinitionBuilder method, Exception exception)
        {
            method.CustomAttributes.Add(new CustomAttributeBuilder(ExpectedExceptionAttributeConstructorWithType,
                MetadataExpression.TypeOf(exception.GetType())));
        }

        public override void MakeInconclusive(MethodDefinitionBuilder method, string message)
        {
            method.MethodBodyBuilder.Push(message);
            method.MethodBodyBuilder.Callstatic(AssertInconclusiveMethod, new IType[0]);
        }

        public override bool IsFixture(TypeDefinition target)
        {
            // FIXME: Does not handle custom fixture types.
            return target.IsDefined(Metadata<TestFixtureAttribute>.Type, true);
        }

        public override bool IsTest(MethodDefinition method)
        {
            // FIXME: Does not handle custom test types.
            return method.IsDefined(Metadata<TestAttribute>.Type, true);
        }

        public override bool TryGetAssertAreEqual(IType type, out IMethod method)
        {
            TypeEx argType = type as TypeEx;
            if (argType == null)
                return base.TryGetAssertAreEqual(type, out method);

            method = AssertAreEqualMethod.Instantiate(argType);
            return true;
        }

        public override bool TryReadExpectedException(ICustomAttributeProviderEx target, out TypeEx exceptionType)
        {
            ExpectedExceptionAttribute attribute = GetFirstAttributeOfType<ExpectedExceptionAttribute>(target);
            if (attribute != null)
            {
                exceptionType = MetadataFromReflection.GetType(attribute.ExceptionType);
                return true;
            }

            exceptionType = null;
            return false;
        }

        public override bool IsFixtureIgnored(TestFrameworkTestSelection testSelection, TypeEx fixtureType,
            out string ignoreMessage)
        {
            return IsIgnored(testSelection, fixtureType, out ignoreMessage);
        }

        public override bool IsTestIgnored(TestFrameworkTestSelection testSelection, MethodDefinition method,
            out string ignoreMessage)
        {
            return IsIgnored(testSelection, method, out ignoreMessage);
        }

        private static bool IsIgnored(TestFrameworkTestSelection testSelection, ICustomAttributeProviderEx attributeProvider,
            out string ignoreMessage)
        {
            IgnoreAttribute attribute = GetFirstAttributeOfType<IgnoreAttribute>(attributeProvider);
            if (attribute != null)
            {
                ignoreMessage = attribute.Reason;
                return true;
            }

            ignoreMessage = null;
            return !IsInTestSelection(testSelection, attributeProvider);
        }

        private static bool IsInTestSelection(TestFrameworkTestSelection testSelection, ICustomAttributeProviderEx attributeProvider)
        {
            if (testSelection.HasCategories)
            {
                IEnumerable<CategoryAttribute> categoryAttributes = GetAllAttributesOfType<CategoryAttribute>(attributeProvider);

                if (!string.IsNullOrEmpty(testSelection.ExcludedCategories)
                    && ContainsCategory(testSelection.ExcludedCategories, categoryAttributes))
                    return false;

                if (!string.IsNullOrEmpty(testSelection.IncludedCategories)
                    && ! ContainsCategory(testSelection.IncludedCategories, categoryAttributes))
                    return false;
            }

            return true;
        }

        private static bool ContainsCategory(string delimitedCategories, IEnumerable<CategoryAttribute> categoryAttributes)
        {
            foreach (CategoryAttribute categoryAttribute in categoryAttributes)
            {
                if (delimitedCategories.Contains(categoryAttribute.CategoryName))
                    return true;
            }

            return false;
        }

        public override bool TryGetAssemblySetupTeardownMethods(AssemblyEx assembly, out Method setup,
            out Method teardown)
        {
            // FIXME: Does not support anything but simple non-generic static AssemblyFixtures.
            foreach (TypeDefinition typeDefinition in assembly.TypeDefinitions)
            {
                if (typeDefinition.IsDefined(Metadata<AssemblyFixtureAttribute>.Type, true))
                {
                    setup = InstantiateMethodIfNotNull(GetAnnotatedMethod(typeDefinition, Metadata<FixtureSetUpAttribute>.Type), null);
                    teardown = InstantiateMethodIfNotNull(GetAnnotatedMethod(typeDefinition, Metadata<FixtureTearDownAttribute>.Type), null);
                    return true;
                }
            }

            setup = null;
            teardown = null;
            return false;
        }

        public override bool TryGetFixtureSetupTeardownMethods(TypeEx type, out Method fixtureSetup,
            out Method fixtureTeardown, out Method testSetup, out Method testTeardown)
        {
            // FIXME: Does not support anything but the basics.
            fixtureSetup = InstantiateMethodIfNotNull(GetAnnotatedMethod(type.Definition, Metadata<FixtureSetUpAttribute>.Type), type);
            fixtureTeardown = InstantiateMethodIfNotNull(GetAnnotatedMethod(type.Definition, Metadata<FixtureTearDownAttribute>.Type), type);
            testSetup = InstantiateMethodIfNotNull(GetAnnotatedMethod(type.Definition, Metadata<SetUpAttribute>.Type), type);
            testTeardown = InstantiateMethodIfNotNull(GetAnnotatedMethod(type.Definition, Metadata<TearDownAttribute>.Type), type);
            return true;
        }

        private static T GetFirstAttributeOfType<T>(ICustomAttributeProviderEx attributeProvider)
            where T : Attribute
        {
            ICustomAttribute[] attributes = attributeProvider.GetAttributes(Metadata<T>.Type, true);
            if (attributes.Length != 0)
                return (T)attributes[0].GetValue();

            return null;
        }

        private static T[] GetAllAttributesOfType<T>(ICustomAttributeProviderEx attributeProvider)
            where T : Attribute
        {
            return Enumerable.ConvertAllToArray<ICustomAttribute, T>(attributeProvider.GetAttributes(Metadata<T>.Type, true),
                delegate(ICustomAttribute attribute)
                {
                    return (T)attribute.GetValue();
                });
        }

        private static Method InstantiateMethodIfNotNull(MethodDefinition method, TypeEx reflectedType)
        {
            if (method == null)
                return null;

            return reflectedType != null ? GetMethod(reflectedType, method)
                : method.Instantiate(TypeEx.NoTypes, TypeEx.NoTypes);
        }

        private static MethodDefinition GetAnnotatedMethod(TypeDefinition typeDefinition, TypeEx attributeType)
        {
            foreach (MethodDefinition methodDefinition in GetAllMethodsInFlattenedHierarchy(typeDefinition))
            {
                if (methodDefinition.IsDefined(attributeType, true))
                    return methodDefinition;
            }

            return null;
        }

        private static IEnumerable<MethodDefinition> GetAllMethodsInFlattenedHierarchy(TypeDefinition typeDefinition)
        {
            foreach (MethodDefinition methodDefinition in typeDefinition.InstanceMethods)
                yield return methodDefinition;

            do
            {
                foreach (MethodDefinition methodDefinition in typeDefinition.DeclaredStaticMethods)
                    yield return methodDefinition;

                typeDefinition = typeDefinition.BaseTypeDefinition;
            }
            while (typeDefinition != null);
        }
    }
}