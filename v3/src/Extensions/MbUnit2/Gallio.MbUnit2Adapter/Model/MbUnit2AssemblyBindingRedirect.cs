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

extern alias MbUnit2;

using System.Reflection;
using Gallio.Hosting;
using Gallio.Model;
using Gallio.Runner.Domains;

namespace Gallio.MbUnit2Adapter.Model
{
    /// <summary>
    /// Contributes a binding redirect for the MbUnit v2 assemblies.
    /// This ensures that we can run MbUnit v2 tests even if the version of MbUnit
    /// they were built against differs from the plugin so long as no breaking API
    /// changes are encountered.
    /// </summary>
    internal class MbUnit2AssemblyBindingRedirect : IHostTestDomainContributor
    {
        /// <inheritdoc />
        public void ConfigureHost(HostSetup hostSetup, TestPackageConfig packageConfig)
        {
            Assembly frameworkAssembly = typeof(MbUnit2::MbUnit.Framework.Assert).Assembly;
            hostSetup.Configuration.AddAssemblyBinding(frameworkAssembly, true);
        }
    }
}
