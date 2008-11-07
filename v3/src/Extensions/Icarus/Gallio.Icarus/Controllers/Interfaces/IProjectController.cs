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
using System.ComponentModel;
using Gallio.Icarus.Controllers.EventArgs;
using Gallio.Icarus.Models.Interfaces;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Runner.Projects;
using Gallio.Runtime.ProgressMonitoring;
using System.Threading;

namespace Gallio.Icarus.Controllers.Interfaces
{
    public interface IProjectController
    {
        IProjectTreeModel Model { get; }
        TestPackageConfig TestPackageConfig { get; }
        BindingList<FilterInfo> TestFilters { get; }
        BindingList<string> HintDirectories { get; }
        string ProjectFileName { get; }
        SynchronizationContext SynchronizationContext { get; set; }

        event EventHandler<AssemblyChangedEventArgs> AssemblyChanged;

        void AddAssemblies(IList<string> assemblies, IProgressMonitor progressMonitor);
        void DeleteFilter(FilterInfo filterInfo, IProgressMonitor progressMonitor);
        Filter<ITest> GetFilter(string filterName, IProgressMonitor progressMonitor);
        void NewProject(IProgressMonitor progressMonitor);
        void OpenProject(string projectName, IProgressMonitor progressMonitor);
        void RemoveAllAssemblies(IProgressMonitor progressMonitor);
        void RemoveAssembly(string fileName, IProgressMonitor progressMonitor);
        void SaveFilter(string filterName, Filter<ITest> filter, IProgressMonitor progressMonitor);
        void SaveProject(string projectName, IProgressMonitor progressMonitor);
    }
}
