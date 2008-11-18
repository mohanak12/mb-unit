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

using System.Collections.Generic;
using System.IO;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Mediator.Interfaces;
using Gallio.Icarus.ProgressMonitoring;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Runner.Projects;
using Gallio.Runtime.ProgressMonitoring;
using System;

namespace Gallio.Icarus.Mediator
{
    public class Mediator : IMediator
    {
        private readonly TaskManager taskManager = new TaskManager();
        private readonly ProgressMonitorProvider progressMonitorProvider = new ProgressMonitorProvider();

        public IProjectController ProjectController { get; set; }

        public ITestController TestController { get; set; }

        public IReportController ReportController { get; set; }

        public IExecutionLogController ExecutionLogController { get; set; }

        public IAnnotationsController AnnotationsController { get; set; }

        public IRuntimeLogController RuntimeLogController { get; set; }

        public IOptionsController OptionsController { get; set; }

        public ProgressMonitorProvider ProgressMonitorProvider
        {
            get { return progressMonitorProvider; }
        }

        public TaskManager TaskManager
        {
            get { return taskManager; }
        }

        private Mediator()
        { }

        internal static Mediator Instance
        {
            get { return Nested.instance; }
        }

        private class Nested
        {
            static Nested()
            { }

            internal static readonly Mediator instance = new Mediator();
        }

        public void AddAssemblies(IList<string> assemblyFiles)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Adding assemblies", 100))
                {
                    // add assemblies to test package
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(10))
                        ProjectController.AddAssemblies(assemblyFiles, subProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    // reload tests
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(90))
                        TestController.Reload(ProjectController.TestPackageConfig, subProgressMonitor);
                }
            }));
        }

        public void ApplyFilter(string filter)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                TestController.ApplyFilter(filter, progressMonitor)));
        }

        public void DeleteFilter(FilterInfo filterInfo)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                ProjectController.DeleteFilter(filterInfo, progressMonitor)));
        }

        public void GenerateReport()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                var reportFolder = Path.Combine(Path.GetDirectoryName(ProjectController.ProjectFileName),
                    "Reports");

                // good luck cancelling that fast!
                if (progressMonitor.IsCanceled)
                    throw new OperationCanceledException();

                TestController.Report.Read(
                    report => ReportController.GenerateReport(report, reportFolder, progressMonitor));
            }));
        }

        public void NewProject()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Creating new project.", 100))
                {
                    // create a new project
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(10))
                        ProjectController.NewProject(subProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    // reload
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(90))
                        TestController.Reload(ProjectController.TestPackageConfig, subProgressMonitor);
                }
            }));
        }

        public void OpenProject(string fileName)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(
                delegate(IProgressMonitor progressMonitor)
                {
                    using (progressMonitor.BeginTask("Opening project.", 100))
                    {
                        using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(10))
                            ProjectController.OpenProject(fileName, subProgressMonitor);

                        if (progressMonitor.IsCanceled)
                            throw new OperationCanceledException();

                        using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(80))
                            TestController.Reload(ProjectController.TestPackageConfig, subProgressMonitor);

                        if (progressMonitor.IsCanceled)
                            throw new OperationCanceledException();

                        using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(10))
                            RestoreFilter(subProgressMonitor);
                    }
                }));
        }

        public void Reload()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Reloading", 100))
                {
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(95))
                        TestController.Reload(subProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                        RestoreFilter(subProgressMonitor);
                }
            }));
        }

        public void RefreshTestTree()
        {
            progressMonitorProvider.Run(progressMonitor => 
                TestController.RefreshTestTree(progressMonitor));
        }

        private void RestoreFilter(IProgressMonitor progressMonitor)
        {
            foreach (FilterInfo filterInfo in ProjectController.TestFilters)
            {
                if (progressMonitor.IsCanceled)
                    throw new OperationCanceledException();

                if (filterInfo.FilterName != "AutoSave")
                    continue;
                TestController.ApplyFilter(filterInfo.Filter, progressMonitor);
                return;
            }
        }

        public void RemoveAllAssemblies()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Removing all assemblies.", 100))
                {
                    // remove all assemblies from test package
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(50))
                        ProjectController.RemoveAllAssemblies(subProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    // reload
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(50))
                        TestController.Reload(ProjectController.TestPackageConfig, subProgressMonitor);
                }
            }));
        }

        public void RemoveAssembly(string fileName)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                ProjectController.RemoveAssembly(fileName, progressMonitor)));
        }

        public void ResetTests()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                TestController.ResetTests(progressMonitor)));
        }

        public void RunTests()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Running tests.", 100))
                {
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                    using (IProgressMonitor subSubProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                        ProjectController.SaveFilter("LastRun", TestController.GetCurrentFilter(subProgressMonitor),
                            subSubProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(90))
                        TestController.RunTests(subProgressMonitor);
                }
            }));    
        }

        public void SaveFilter(string filterName)
        {
            progressMonitorProvider.Run(delegate(IProgressMonitor progressMonitor)
            {
                using (progressMonitor.BeginTask("Saving filter", 2))
                {
                    Filter<ITest> filter;
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(50))
                        filter = TestController.GetCurrentFilter(subProgressMonitor);

                    if (progressMonitor.IsCanceled)
                        throw new OperationCanceledException();

                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(50))
                        ProjectController.SaveFilter(filterName, filter, subProgressMonitor);
                }
            });
        }

        public void SaveProject(string projectFileName)
        {
            // don't run as task, or the project won't get saved at shutdown
            progressMonitorProvider.Run(progressMonitor => ProjectController.SaveProject(projectFileName, progressMonitor));
        }

        public void ShowReport(string reportFormat)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => TestController.Report.Read(report => 
                ReportController.ShowReport(report, reportFormat, progressMonitor))));
        }

        public void Unload()
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                TestController.UnloadTestPackage(progressMonitor)));
        }

        public void ViewSourceCode(string testId)
        {
            taskManager.StartTask(() => progressMonitorProvider.Run(progressMonitor => 
                TestController.ViewSourceCode(testId, progressMonitor)));
        }

        public void Cancel()
        {
            if (progressMonitorProvider.ProgressMonitor != null)
                progressMonitorProvider.ProgressMonitor.Cancel();
            taskManager.Stop();
        }
    }
}
