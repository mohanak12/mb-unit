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

using System.Collections.Generic;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Runner.Projects.Schema;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Model.Filters;
using Gallio.UI.ProgressMonitoring;

namespace Gallio.Icarus.Commands
{
    internal class ReloadCommand : ICommand
    {
        private readonly ITestController testController;
        private readonly IProjectController projectController;

        public ReloadCommand(ITestController testController, IProjectController projectController)
        {
            this.testController = testController;
            this.projectController = projectController;
        }

        public void Execute(IProgressMonitor progressMonitor)
        {
            using (progressMonitor.BeginTask("Reloading", 100))
            {
                using (var subProgressMonitor = progressMonitor.CreateSubProgressMonitor(95))
                    testController.Explore(subProgressMonitor, projectController.TestRunnerExtensions);

                var testFilters = projectController.TestFilters.Value;

                using (var subProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                    RestoreFilter(subProgressMonitor, testFilters);
            }
        }

        private void RestoreFilter(IProgressMonitor progressMonitor, ICollection<FilterInfo> testFilters)
        {
            using (progressMonitor.BeginTask("Restoring test filter", testFilters.Count))
            {
                foreach (var filterInfo in testFilters)
                {
                    if (filterInfo.FilterName != "AutoSave")
                    {
                        progressMonitor.Worked(1);
                        continue;
                    }

                    var filterSet = FilterUtils.ParseTestFilterSet(filterInfo.FilterExpr);
                    testController.ApplyFilterSet(filterSet);
                    return;
                }
            }
        }
    }
}
