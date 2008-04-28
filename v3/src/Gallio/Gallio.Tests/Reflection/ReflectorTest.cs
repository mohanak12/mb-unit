// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using Gallio.Reflection;
using Gallio.Utilities;
using MbUnit.Framework;

namespace Gallio.Tests.Reflection
{
    [TestFixture]
    [TestsOn(typeof(Reflector))]
    public class ReflectorTest
    {
        [Test]
        public void Resolve_Unknown_ReturnsNull()
        {
            Assert.IsNull(Reflector.Resolve(CodeReference.Unknown, false));
            Assert.IsNull(Reflector.Resolve(CodeReference.Unknown, true));
        }

        [Test]
        public void Wrap_Null_ReturnsNull()
        {
            Assert.IsNull(Reflector.Wrap((Assembly) null));
            Assert.IsNull(Reflector.WrapNamespace(null));
            Assert.IsNull(Reflector.Wrap((Type)null));
            Assert.IsNull(Reflector.Wrap((MemberInfo)null));
            Assert.IsNull(Reflector.Wrap((MethodBase)null));
            Assert.IsNull(Reflector.Wrap((MethodInfo)null));
            Assert.IsNull(Reflector.Wrap((ConstructorInfo)null));
            Assert.IsNull(Reflector.Wrap((PropertyInfo)null));
            Assert.IsNull(Reflector.Wrap((FieldInfo)null));
            Assert.IsNull(Reflector.Wrap((EventInfo)null));
            Assert.IsNull(Reflector.Wrap((ParameterInfo)null));
            Assert.IsNull(Reflector.Wrap((Attribute)null));
        }

        [Test, ExpectedArgumentOutOfRangeException]
        public void GetFunctionFromStackFrame_ShouldThrowIfLessThanZero()
        {
            Reflector.GetFunctionFromStackFrame(-1);
        }

        [Test]
        public void GetFunctionFromStackFrame()
        {
            IFunctionInfo r = GetFunctionFromStackFrame_Helper();
            Assert.AreEqual("GetFunctionFromStackFrame", r.Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IFunctionInfo GetFunctionFromStackFrame_Helper()
        {
            return Reflector.GetFunctionFromStackFrame(1);
        }

        [Test]
        public void GetCallingFunction()
        {
            IFunctionInfo r = GetCallingFunction_Helper();
            Assert.AreEqual("GetCallingFunction", r.Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IFunctionInfo GetCallingFunction_Helper()
        {
            return Reflector.GetCallingFunction();
        }

        [Test]
        public void GetExecutingFunction()
        {
            IFunctionInfo r = Reflector.GetExecutingFunction();
            Assert.AreEqual("GetExecutingFunction", r.Name);
        }

        [Test]
        public void GetSourceLocation_ReturnsUnknownForNamespaces()
        {
            Assert.AreEqual(CodeLocation.Unknown, Reflector.WrapNamespace("foo").GetCodeLocation());
        }

        [Test]
        public void GetSourceLocation_ReturnsMethodSourceFile()
        {
            CodeLocation location = Reflector.GetExecutingFunction().GetCodeLocation();
            StringAssert.EndsWith(location.Path, GetType().Name + ".cs");
        }
    }
}
