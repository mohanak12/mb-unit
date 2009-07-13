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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.Model.Helpers;
using Gallio.Model.Tree;
using Gallio.NUnitAdapter.Properties;
using NUnit.Util;
using NUnitCoreExtensions = NUnit.Core.CoreExtensions;
using NUnitTestRunner = NUnit.Core.TestRunner;
using NUnitTestPackage = NUnit.Core.TestPackage;
using NUnitRemoteTestRunner = NUnit.Core.RemoteTestRunner;
using NUnitITest = NUnit.Core.ITest;

namespace Gallio.NUnitAdapter.Model
{
    /// <summary>
    /// Explores tests in NUnit assemblies.
    /// </summary>
    internal class NUnitTestExplorer : TestExplorer
    {
#if NUNIT248
        internal const string FrameworkKind = "NUnit v2.4.8 Framework";
        private const string FrameworkName = "NUnit v2.4.8";
#elif NUNIT25
        internal const string FrameworkKind = "NUnit v2.5 Framework";
        private const string FrameworkName = "NUnit v2.5";
#else
#error "Unrecognized NUnit framework version."
#endif
        private const string NUnitFrameworkAssemblyDisplayName = @"nunit.framework";

        private readonly Dictionary<Version, Test> frameworkTests;
        private readonly Dictionary<IAssemblyInfo, Test> assemblyTests;

        public NUnitTestExplorer()
        {
            frameworkTests = new Dictionary<Version, Test>();
            assemblyTests = new Dictionary<IAssemblyInfo, Test>();
        }

        protected override void ExploreImpl(IReflectionPolicy reflectionPolicy, ICodeElementInfo codeElement)
        {
            IAssemblyInfo assembly = ReflectionUtils.GetAssembly(codeElement);
            if (assembly != null)
            {
                Version frameworkVersion = GetFrameworkVersion(assembly);

                if (frameworkVersion != null)
                {
                    Test frameworkTest = GetFrameworkTest(frameworkVersion, TestModel.RootTest);
                    GetAssemblyTest(assembly, frameworkTest);
                }
            }
        }

        private static Version GetFrameworkVersion(IAssemblyInfo assembly)
        {
            AssemblyName frameworkAssemblyName = ReflectionUtils.FindAssemblyReference(assembly, NUnitFrameworkAssemblyDisplayName);
            return frameworkAssemblyName != null ? frameworkAssemblyName.Version : null;
        }

        private Test GetFrameworkTest(Version frameworkVersion, Test rootTest)
        {
            Test frameworkTest;
            if (!frameworkTests.TryGetValue(frameworkVersion, out frameworkTest))
            {
                frameworkTest = CreateFrameworkTest(frameworkVersion);
                rootTest.AddChild(frameworkTest);

                frameworkTests.Add(frameworkVersion, frameworkTest);
            }

            return frameworkTest;
        }

        private static Test CreateFrameworkTest(Version frameworkVersion)
        {
            Test frameworkTest = new Test(String.Format(Resources.NUnitTestExplorer_FrameworkNameWithVersionFormat, frameworkVersion), null);
            frameworkTest.LocalIdHint = FrameworkName;
            frameworkTest.Kind = FrameworkKind;
            frameworkTest.Metadata.Add(MetadataKeys.Framework, FrameworkName);

            return frameworkTest;
        }

        private Test GetAssemblyTest(IAssemblyInfo assembly, Test frameworkTest)
        {
            Test assemblyTest;
            if (!assemblyTests.TryGetValue(assembly, out assemblyTest))
            {
                assemblyTest = CreateAssemblyTest(assembly);
                if (assemblyTest != null)
                {
                    frameworkTest.AddChild(assemblyTest);

                    assemblyTests.Add(assembly, assemblyTest);
                }
            }

            return assemblyTest;
        }

        private Test CreateAssemblyTest(IAssemblyInfo assembly)
        {
            // Resolve test assembly.
            string location;
            try
            {
                location = assembly.Resolve(true).Location;
            }
            catch (Exception ex)
            {
                TestModel.AddAnnotation(new Annotation(AnnotationType.Error, assembly, "Could not resolve the location of an NUnit test assembly.", ex));
                return null;
            }

            try
            {
                NUnitTestRunner runner = CreateTestRunner(location);

                NUnitAssemblyTest assemblyTest = new NUnitAssemblyTest(assembly, runner);
                PopulateMetadata(assemblyTest);

                foreach (NUnitITest assemblyTestSuite in runner.Test.Tests)
                    BuildTestChildren(assemblyTest, assemblyTestSuite);

                return assemblyTest;
            }
            catch (Exception ex)
            {
                TestModel.AddAnnotation(new Annotation(AnnotationType.Error, assembly, "An exception was thrown while exploring an NUnit test assembly.", ex));
                return null;
            }
        }

        private static NUnitTestRunner CreateTestRunner(string assemblyLocation)
        {
            NUnitTestPackage package = new NUnitTestPackage(@"Tests");

            // Note: Don't build nodes for namespaces.  Grouping by namespace is a
            //       presentation concern of the test runner, not strictly a structural one
            //       so we turn this feature off.
            package.Settings.Add(@"AutoNamespaceSuites", false);
            package.Assemblies.Add(assemblyLocation);

            NUnitTestRunner runner = new NUnitRemoteTestRunner();
            if (!runner.Load(package))
                throw new ModelException(Resources.NUnitTestExplorer_CannotLoadNUnitTestAssemblies);

            return runner;
        }

        private static void BuildTest(NUnitTest parentTest, NUnitITest nunitTest)
        {
            string kind;
            ICodeElementInfo codeElement;

            switch (nunitTest.TestType)
            {
#if NUNIT248
                case @"Test Case":
#else
                case @"NUnitTestMethod":
#endif
                    kind = TestKinds.Test;
                    codeElement = ParseTestCaseName(parentTest.CodeElement, nunitTest.TestName.FullName);
                    break;

#if NUNIT248
                case @"Test Fixture":
#else
                case @"NUnitTestFixture":
#endif
                    kind = TestKinds.Fixture;
                    codeElement = ParseTestFixtureName(parentTest.CodeElement, nunitTest.TestName.FullName);
                    break;

                default:
                    kind = nunitTest.IsSuite ? TestKinds.Suite : TestKinds.Test;
                    codeElement = parentTest.CodeElement;
                    break;
            }

            // Build the test.
            NUnitTest test = new NUnitTest(nunitTest.TestName.Name, codeElement, nunitTest);
            test.Kind = kind;
            test.IsTestCase = !nunitTest.IsSuite;

            PopulateMetadata(test);

            parentTest.AddChild(test);
            BuildTestChildren(test, nunitTest);
        }

        private static void BuildTestChildren(NUnitTest parentTest, NUnitITest parentNUnitTest)
        {
            if (parentNUnitTest.Tests != null)
            {
                foreach (NUnitITest childNUnitTest in parentNUnitTest.Tests)
                    BuildTest(parentTest, childNUnitTest);
            }
        }

        private static void PopulateMetadata(NUnitTest test)
        {
            NUnitITest nunitTest = test.Test;

            if (!String.IsNullOrEmpty(nunitTest.Description))
                test.Metadata.Add(MetadataKeys.Description, nunitTest.Description);

            if (!String.IsNullOrEmpty(nunitTest.IgnoreReason))
                test.Metadata.Add(MetadataKeys.IgnoreReason, nunitTest.IgnoreReason);

            foreach (string category in nunitTest.Categories)
                test.Metadata.Add(MetadataKeys.Category, category);

            foreach (DictionaryEntry entry in nunitTest.Properties)
            {
                string keyString = entry.Key.ToString();
                if (!keyString.StartsWith("_"))
                {
                    ICollection values = entry.Value as ICollection;
                    if (values != null)
                    {
                        foreach (object value in values)
                            test.Metadata.Add(keyString, value != null ? value.ToString() : null);
                    }
                    else
                    {
                        test.Metadata.Add(keyString, entry.Value != null ? entry.Value.ToString() : null);
                    }
                }
            }

            ICodeElementInfo codeElement = test.CodeElement;
            if (codeElement != null)
            {
                // Add documentation.
                string xmlDocumentation = codeElement.GetXmlDocumentation();
                if (xmlDocumentation != null)
                    test.Metadata.Add(MetadataKeys.XmlDocumentation, xmlDocumentation);

                // Add assembly-level metadata.
                IAssemblyInfo assemblyInfo = codeElement as IAssemblyInfo;
                if (assemblyInfo != null)
                    ModelUtils.PopulateMetadataFromAssembly(assemblyInfo, test.Metadata);
            }
        }

        /// <summary>
        /// Parses a code element from an NUnit test case name.
        /// The name generally consists of the fixture type full-name followed by
        /// a dot and the test method name.
        /// </summary>
        private static ICodeElementInfo ParseTestCaseName(ICodeElementInfo parent, string name)
        {
            // Handle row-test naming scheme.
            int firstParen = name.IndexOf('(');
            if (firstParen >= 0)
                name = name.Substring(0, firstParen);

            // Parse the identifier.
            if (IsProbableIdentifier(name))
            {
                IAssemblyInfo assembly = ReflectionUtils.GetAssembly(parent);
                if (assembly != null)
                {
                    int lastDot = name.LastIndexOf('.');
                    if (lastDot > 0 && lastDot < name.Length - 1)
                    {
                        string typeName = name.Substring(0, lastDot);
                        string methodName = name.Substring(lastDot + 1);

                        ITypeInfo type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            try
                            {
                                return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                            }
                            catch (AmbiguousMatchException)
                            {
                                // We may have insufficient information to distinguish overloaded
                                // test methods.  In this case we give up trying to find the code element.
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a code reference from an NUnit test fixture name.
        /// The name generally consists of the fixture type full-name.
        /// </summary>
        private static ICodeElementInfo ParseTestFixtureName(ICodeElementInfo parent, string name)
        {
            if (IsProbableIdentifier(name))
            {
                IAssemblyInfo assembly = ReflectionUtils.GetAssembly(parent);
                if (assembly != null)
                    return assembly.GetType(name);
            }

            return null;
        }

        private static bool IsProbableIdentifier(string name)
        {
            return name.Length != 0
                && !name.Contains(@" ")
                && !name.StartsWith(@".")
                && !name.EndsWith(@".");
        }
    }
}
