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

extern alias MbUnit2;

using System;
using System.Collections.Generic;
using System.Reflection;
using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.MbUnit2Adapter.Properties;
using TestFixturePatternAttribute2 = MbUnit2::MbUnit.Core.Framework.TestFixturePatternAttribute;
using TestPatternAttribute2 = MbUnit2::MbUnit.Core.Framework.TestPatternAttribute;
using FixtureCategoryAttribute2 = MbUnit2::MbUnit.Framework.FixtureCategoryAttribute;
using TestCategoryAttribute2 = MbUnit2::MbUnit.Framework.TestCategoryAttribute;
using TestImportance2 = MbUnit2::MbUnit.Framework.TestImportance;
using AuthorAttribute2 = MbUnit2::MbUnit.Framework.AuthorAttribute;
using TestsOnAttribute2 = MbUnit2::MbUnit.Framework.TestsOnAttribute;
using ImportanceAttribute2 = MbUnit2::MbUnit.Framework.ImportanceAttribute;

namespace Gallio.MbUnit2Adapter.Model
{
    /// <summary>
    /// Explores tests in MbUnit v2 assemblies.
    /// </summary>
    internal class MbUnit2TestExplorer : BaseTestExplorer
    {
        private const string FrameworkKind = "MbUnit v2 Framework";
        private const string FrameworkName = "MbUnit v2";
        private const string MbUnitFrameworkAssemblyDisplayName = @"MbUnit.Framework";

        public override void Explore(TestModel testModel, TestSource testSource, Action<ITest> consumer)
        {
            var state = new ExplorerState(testModel);

            foreach (IAssemblyInfo assembly in testSource.Assemblies)
                state.ExploreAssembly(assembly, consumer);

            foreach (ITypeInfo type in testSource.Types)
                state.ExploreType(type, consumer);
        }

        private sealed class ExplorerState
        {
            private readonly TestModel testModel;
            private readonly Dictionary<Version, ITest> frameworkTests;
            private readonly Dictionary<IAssemblyInfo, ITest> assemblyTests;
            private readonly List<KeyValuePair<ITest, string>> unresolvedDependencies;

            public ExplorerState(TestModel testModel)
            {
                this.testModel = testModel;
                frameworkTests = new Dictionary<Version, ITest>();
                assemblyTests = new Dictionary<IAssemblyInfo, ITest>();
                unresolvedDependencies = new List<KeyValuePair<ITest, string>>();
            }

            public void ExploreAssembly(IAssemblyInfo assembly, Action<ITest> consumer)
            {
                Version frameworkVersion = GetFrameworkVersion(assembly);

                if (frameworkVersion != null)
                {
                    ITest frameworkTest = GetFrameworkTest(frameworkVersion, testModel.RootTest);
                    ITest assemblyTest = GetAssemblyTest(assembly, frameworkTest);

                    if (assemblyTest != null && consumer != null)
                        consumer(assemblyTest);
                }
            }

            public void ExploreType(ITypeInfo type, Action<ITest> consumer)
            {
                // TODO: Optimize me to only populate what's strictly required.
                ExploreAssembly(type.Assembly, delegate(ITest assemblyTest)
                {
                    foreach (ITest test in assemblyTest.Children)
                    {
                        if (test.CodeElement.Equals(type))
                            consumer(test);
                    }
                });
            }

            private static Version GetFrameworkVersion(IAssemblyInfo assembly)
            {
                AssemblyName frameworkAssemblyName = ReflectionUtils.FindAssemblyReference(assembly, MbUnitFrameworkAssemblyDisplayName);
                return frameworkAssemblyName != null ? frameworkAssemblyName.Version : null;
            }

            private ITest GetFrameworkTest(Version frameworkVersion, ITest rootTest)
            {
                ITest frameworkTest;
                if (! frameworkTests.TryGetValue(frameworkVersion, out frameworkTest))
                {
                    frameworkTest = CreateFrameworkTest(frameworkVersion);
                    rootTest.AddChild(frameworkTest);

                    frameworkTests.Add(frameworkVersion, frameworkTest);
                }

                return frameworkTest;
            }

            private static ITest CreateFrameworkTest(Version frameworkVersion)
            {
                BaseTest frameworkTest = new BaseTest(
                    String.Format(Resources.MbUnit2TestExplorer_FrameworkNameWithVersionFormat, frameworkVersion), null);
                frameworkTest.LocalIdHint = FrameworkName;
                frameworkTest.Kind = FrameworkKind;
                frameworkTest.Metadata.Add(MetadataKeys.Framework, FrameworkName);

                return frameworkTest;
            }

            private ITest GetAssemblyTest(IAssemblyInfo assembly, ITest frameworkTest)
            {
                ITest assemblyTest;
                if (assemblyTests.TryGetValue(assembly, out assemblyTest))
                    return assemblyTest;

                try
                {
                    Assembly loadedAssembly = assembly.Resolve(false);

                    if (loadedAssembly != null)
                        assemblyTest = MbUnit2NativeTestExplorer.BuildAssemblyTest(loadedAssembly, unresolvedDependencies);
                    else
                        assemblyTest = MbUnit2ReflectiveTestExplorer.BuildAssemblyTest(testModel, assembly, unresolvedDependencies);
                }
                catch (Exception ex)
                {
                    testModel.AddAnnotation(new Annotation(AnnotationType.Error, assembly,
                        "An exception was thrown while exploring an MbUnit v2 test assembly.", ex));
                    return null;
                }

                for (int i = 0; i < unresolvedDependencies.Count; i++)
                {
                    foreach (KeyValuePair<IAssemblyInfo, ITest> entry in assemblyTests)
                    {
                        if (entry.Key.FullName == unresolvedDependencies[i].Value)
                        {
                            unresolvedDependencies[i].Key.AddDependency(entry.Value);
                            unresolvedDependencies.RemoveAt(i--);
                            break;
                        }
                    }
                }

                frameworkTest.AddChild(assemblyTest);

                assemblyTests.Add(assembly, assemblyTest);
                return assemblyTest;
            }
        }
    }
}
