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
using System.Windows.Forms;

using Gallio.Runtime.Logging;

using Gallio.Icarus.Core.CustomEventArgs;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Model.Serialization;
using Gallio.Reflection;
using Gallio.Runner.Reports;
using Gallio.Runner.Projects;

using Aga.Controls.Tree;
using System.IO;

namespace Gallio.Icarus.Interfaces
{
    public interface IProjectAdapterView
    {
        event EventHandler<SingleEventArgs<IList<string>>> AddAssemblies;
        event EventHandler<EventArgs> RemoveAssemblies;
        event EventHandler<SingleEventArgs<string>> RemoveAssembly;
        event EventHandler<GetTestTreeEventArgs> GetTestTree;
        event EventHandler<EventArgs> RunTests;
        event EventHandler<EventArgs> GenerateReport;
        event EventHandler<EventArgs> CancelOperation;
        event EventHandler<SingleEventArgs<string>> SaveFilter;
        event EventHandler<SingleEventArgs<string>> ApplyFilter;
        event EventHandler<SingleEventArgs<string>> DeleteFilter;
        event EventHandler<EventArgs> GetReportTypes;
        event EventHandler<SaveReportAsEventArgs> SaveReportAs;
        event EventHandler<SingleEventArgs<string>> SaveProject;
        event EventHandler<OpenProjectEventArgs> OpenProject;
        event EventHandler<EventArgs> NewProject;
        event EventHandler<EventArgs> GetTestFrameworks;
        event EventHandler<SingleEventArgs<string>> GetSourceLocation;
        event EventHandler<SingleEventArgs<IList<string>>> UpdateHintDirectoriesEvent;
        event EventHandler<SingleEventArgs<string>> UpdateApplicationBaseDirectoryEvent;
        event EventHandler<SingleEventArgs<string>> UpdateWorkingDirectoryEvent;
        event EventHandler<SingleEventArgs<bool>> UpdateShadowCopyEvent;
        event EventHandler<EventArgs> ResetTestStatus;
        event EventHandler<SingleEventArgs<IList<string>>> GetExecutionLog;
        event EventHandler<EventArgs> UnloadTestPackage;

        bool ShowProgressMonitor { set; }
        ITreeModel TestTreeModel { set; }
        ITreeModel ProjectTreeModel { set; }
        string TaskName { set; }
        string SubTaskName { set; }
        string ReportPath { set; }
        IList<string> TestFilters { set; }
        IList<string> ReportTypes { set; }
        IList<string> TestFrameworks { set; }
        IList<string> HintDirectories { set; }
        string ApplicationBaseDirectory { set; }
        string WorkingDirectory { set; }
        bool ShadowCopy { set; }
        List<AnnotationData> Annotations { set; }
        Stream ExecutionLog { set; }
        double CompletedWorkUnits { set; }
        double TotalWorkUnits { set; }
        int TotalTests { set; }
        CodeLocation SourceCodeLocation { set; }
        Settings Settings { get; set; }
        bool EditEnabled { set; }
        string[] Args { set; }
        
        void ThreadedRemoveAssembly(string assembly);
        void ReloadTree();
        void SaveReport(string fileName, string reportType);
        void WriteToLog(LogSeverity severity, string message, Exception exception);
        void ResetTests();
        void CreateReport();
        void AddAssembliesToTree();
        void RemoveAssembliesFromTree();
        void ViewSourceCode(string testId);
        void AssemblyChanged(string filePath);
        void UpdateHintDirectories(IList<string> hintDirectories);
        void UpdateApplicationBaseDirectory(string applicationBaseDirectory);
        void UpdateWorkingDirectory(string workingDirectory);
        void UpdateShadowCopy(bool shadowCopy);
        void OnApplyFilter(string filter);
        void OnSaveFilter(string filter);
        void OnDeleteFilter(string filter);
        void LoadComplete();
        void UpdateSelectedNode(IList<string> testIds);
        void CancelRunningOperation();
    }
}
