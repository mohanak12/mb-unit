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
using System.Reflection;
using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.Model.Helpers;
using Gallio.Model.Tree;
using Gallio.NUnitAdapter.Properties;
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
        internal const string AssemblyKind = "NUnit v2.4.8 Assembly";
#elif NUNIT25
        internal const string AssemblyKind = "NUnit v2.5 Assembly";
#else
#error "Unrecognized NUnit framework version."
#endif

        private readonly Dictionary<IAssemblyInfo, NUnitTestExplorerEngine> assemblyTestExplorerEngines;

        public NUnitTestExplorer()
        {
            assemblyTestExplorerEngines = new Dictionary<IAssemblyInfo, NUnitTestExplorerEngine>();
        }

        protected override void ExploreImpl(IReflectionPolicy reflectionPolicy, ICodeElementInfo codeElement)
        {
            IAssemblyInfo assembly = ReflectionUtils.GetAssembly(codeElement);
            if (assembly == null)
                return;

            try
            {
                NUnitTestExplorerEngine engine;
                if (!assemblyTestExplorerEngines.TryGetValue(assembly, out engine))
                {
                    Assembly loadedAssembly = assembly.Resolve(false);

                    if (loadedAssembly != null)
                        engine = new NUnitNativeTestExplorerEngine(TestModel, loadedAssembly);
                    else
                        engine = new NUnitReflectiveTestExplorerEngine(TestModel, assembly);

                    bool skipChildren = !(codeElement is IAssemblyInfo);
                    engine.ExploreAssembly(skipChildren);
                }

                ITypeInfo type = ReflectionUtils.GetType(codeElement);
                if (type != null)
                {
                    engine.ExploreType(type);
                }
            }
            catch (Exception ex)
            {
                TestModel.AddAnnotation(new Annotation(AnnotationType.Error, assembly,
                    "An exception was thrown while exploring an NUnit test assembly.", ex));
            }
        }
    }
}
