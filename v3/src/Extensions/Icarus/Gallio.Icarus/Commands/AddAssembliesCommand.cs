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
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.Icarus.Commands
{
    internal class AddAssembliesCommand : ICommand
    {
        private readonly IProjectController projectController;
        private readonly ITestController testController;
        private readonly IList<string> assemblyFiles;

        public AddAssembliesCommand(IProjectController projectController, ITestController testController, 
            IList<string> assemblyFiles)
        {
            this.projectController = projectController;
            this.testController = testController;
            this.assemblyFiles = assemblyFiles;
        }

        public void Execute(IProgressMonitor progressMonitor)
        {
            using (progressMonitor.BeginTask("Adding assemblies", 100))
            {
                // add assemblies to test package
                using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(10))
                    projectController.AddAssemblies(assemblyFiles, subProgressMonitor);

                if (progressMonitor.IsCanceled)
                    throw new OperationCanceledException();

                // reload tests
                using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(90))
                {
                    testController.SetTestPackageConfig(projectController.TestPackageConfig);
                    testController.Explore(subProgressMonitor, projectController.TestRunnerExtensions);
                }
            }
        }
    }
}
