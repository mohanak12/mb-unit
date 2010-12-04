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

using System;
using System.Collections.Generic;
using Gallio.Common.Policies;
using Gallio.Icarus.Commands;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Events;
using Gallio.Icarus.Models;
using Gallio.Icarus.Projects;
using Gallio.Model;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.UI.Events;
using Gallio.UI.ProgressMonitoring;

namespace Gallio.Icarus.TestExplorer
{
    public class Controller : IController, Handles<ApplicationShutdown>, Handles<Reloading>, Handles<RunStarted>,
        Handles<RunFinished>, Handles<ExploreStarted>, Handles<ExploreFinished>, Handles<UserOptionsLoaded>
    {
        private readonly IModel model;
        private readonly IEventAggregator eventAggregator;
        private readonly IUserOptionsController userOptionsController;
        private readonly ITaskManager taskManager;
        private readonly ICommandFactory commandFactory;

        public event EventHandler SaveState;
        public event EventHandler RestoreState;

        public Controller(IModel model, IEventAggregator eventAggregator, IOptionsController optionsController, 
            IUserOptionsController userOptionsController, ITaskManager taskManager, ICommandFactory commandFactory)
        {
            this.model = model;
            this.commandFactory = commandFactory;
            this.eventAggregator = eventAggregator;
            this.userOptionsController = userOptionsController;
            this.taskManager = taskManager;

            model.PassedColor.Value = optionsController.PassedColor;
            model.FailedColor.Value = optionsController.FailedColor;
            model.SkippedColor.Value = optionsController.SkippedColor;
            model.InconclusiveColor.Value = optionsController.InconclusiveColor;

            model.TreeViewCategories = optionsController.SelectedTreeViewCategories;

            model.CollapsedNodes.Value = new List<string>(userOptionsController.CollapsedNodes);
        }

        public void SortTree(SortOrder sortOrder)
        {
            eventAggregator.Send(this, new SortTreeEvent(sortOrder));
        }

        public void FilterStatus(TestStatus testStatus)
        {
            switch (testStatus)
            {
                case TestStatus.Passed:
                    model.FilterPassed.Value = !model.FilterPassed;
                    break;

                case TestStatus.Failed:
                    model.FilterFailed.Value = !model.FilterFailed;
                    break;

                case TestStatus.Inconclusive:
                    model.FilterInconclusive.Value = !model.FilterInconclusive;
                    break;
            }
            eventAggregator.Send(this, new FilterTestStatusEvent(testStatus));
        }

        public void AddFiles(string[] fileNames)
        {
            var command = commandFactory.CreateAddFilesCommand(fileNames);
            taskManager.QueueTask(command);
        }

        public void RemoveAllFiles()
        {
            var command = commandFactory.CreateRemoveAllFilesCommand();
            taskManager.QueueTask(command);
        }

        public void RemoveFile(string fileName)
        {
            var command = commandFactory.CreateRemoveFileCommand(fileName);
            taskManager.QueueTask(command);
        }

        public void ChangeTreeCategory(string newCategory, Action<IProgressMonitor> continuation)
        {
            model.CurrentTreeViewCategory.Value = newCategory;
            eventAggregator.Send(this, new TreeViewCategoryChanged(newCategory));
            var command = commandFactory.CreateRefreshTestTreeCommand();
            taskManager.QueueTask(command);
            taskManager.QueueTask(new DelegateCommand(continuation));
        }

        public void ShowSourceCode(string testId)
        {
            var command = commandFactory.CreateViewSourceCodeCommand(testId);
            taskManager.QueueTask(command);
        }

        public void SetCollapsedNodes(IEnumerable<string> collapsedNodes)
        {
            userOptionsController.SetCollapsedNodes(collapsedNodes);
        }

        public void ResetTests()
        {
            var command = commandFactory.CreateResetTestsCommand();
            taskManager.QueueTask(command);
        }

        public void SetTreeSelection(IEnumerable<TestTreeNode> nodes)
        {
            eventAggregator.Send(this, new TestSelectionChanged(nodes));
        }

        public void Handle(ApplicationShutdown @event)
        {
            EventHandlerPolicy.SafeInvoke(SaveState, 
                this, EventArgs.Empty);

            var command = commandFactory.CreateSaveFilterCommand("AutoSave");
            command.Execute(NullProgressMonitor.CreateInstance());
        }

        public void Handle(Reloading @event)
        {
            EventHandlerPolicy.SafeInvoke(SaveState,
                this, EventArgs.Empty);
        }

        public void Handle(RunStarted @event)
        {
            model.CanEditTree.Value = false;
        }

        public void Handle(RunFinished @event)
        {
            model.CanEditTree.Value = true;
        }

        public void Handle(ExploreStarted @event)
        {
            model.CanEditTree.Value = false;
        }

        public void Handle(ExploreFinished @event)
        {
            EventHandlerPolicy.SafeInvoke(RestoreState, this,
                EventArgs.Empty);
            model.CanEditTree.Value = true;
        }

        public void Handle(UserOptionsLoaded @event)
        {
            model.CollapsedNodes.Value = new List<string>(userOptionsController.CollapsedNodes);
            model.CurrentTreeViewCategory.Value = userOptionsController.TreeViewCategory;
        }
    }
}
