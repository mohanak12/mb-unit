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
using System.IO;
using System.Reflection;
using Gallio.Reflection;
using MbUnit.Framework;

namespace Gallio.Tests.Reflection
{
    [TestFixture]
    [TestsOn(typeof(ReflectionOnlyAssemblyLoader))]
    public class ReflectionOnlyAssemblyLoaderTest
    {
        [Test]
        public void AddHintDirectoryThrowsWhenPathIsNull()
        {
            ReflectionOnlyAssemblyLoader loader = new ReflectionOnlyAssemblyLoader();
            InterimAssert.Throws<ArgumentNullException>(delegate { loader.AddHintDirectory(null); });
        }

        [Test]
        public void FallsBackOnNativelyLoadableAssemblies()
        {
            ReflectionOnlyAssemblyLoader loader = new ReflectionOnlyAssemblyLoader();
            Assembly nativeAssembly = typeof(ReflectionOnlyAssemblyLoader).Assembly;
            IAssemblyInfo assembly = loader.ReflectionPolicy.LoadAssembly(nativeAssembly.GetName());
            Assert.AreEqual(AssemblyUtils.GetAssemblyLocalPath(nativeAssembly), assembly.Path);
        }

        [Test]
        public void LoadsAssembliesFromHintPathPreferentially()
        {
            ReflectionOnlyAssemblyLoader loader = new ReflectionOnlyAssemblyLoader();
            string binDir = Path.GetDirectoryName(AssemblyUtils.GetAssemblyLocalPath(typeof(ReflectionOnlyAssemblyLoader).Assembly));
            loader.AddHintDirectory("non-existent-folder-is-ignored-without-sideeffects");
            loader.AddHintDirectory(binDir);

            IAssemblyInfo assembly = loader.ReflectionPolicy.LoadAssembly(new AssemblyName("Gallio")); // would ordinarily be loaded from somewhere else
            OldStringAssert.StartsWith(assembly.Path, binDir);
        }
    }
}
