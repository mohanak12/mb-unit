// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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

using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Events;
using Gallio.Icarus.Services;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.UI.Events;
using Gallio.UI.ProgressMonitoring;

namespace Gallio.Icarus.Commands
{
    public class ReloadCommand : ICommand
    {
        private readonly ITestController testController;
        private readonly IProjectController projectController;
        private readonly IEventAggregator eventAggregator;
        private readonly IFilterService filterService;

        public ReloadCommand(ITestController testController, IProjectController projectController, 
            IEventAggregator eventAggregator, IFilterService filterService)
        {
            this.testController = testController;
            this.projectController = projectController;
            this.eventAggregator = eventAggregator;
            this.filterService = filterService;
        }

        public void Execute(IProgressMonitor progressMonitor)
        {
            using (progressMonitor.BeginTask("Reloading", 100))
            {
                eventAggregator.Send(this, new Reloading());

                using (var subProgressMonitor = progressMonitor.CreateSubProgressMonitor(95))
                {
                    testController.Explore(subProgressMonitor, projectController.TestRunnerExtensionSpecifications);
                }

                using (var subProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                {
                    var restoreFilterCommand = new RestoreFilterCommand(filterService, projectController);
                    restoreFilterCommand.Execute(subProgressMonitor);
                }
            }
        }
    }
}
