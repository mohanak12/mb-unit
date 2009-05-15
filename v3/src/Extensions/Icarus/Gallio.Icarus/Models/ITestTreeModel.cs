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

using System.ComponentModel;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Gallio.Icarus.Helpers;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Model.Serialization;
using Gallio.Runner.Reports;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.Icarus.Models
{
    public interface ITestTreeModel : ITreeModel, INotifyPropertyChanged
    {
        bool FilterPassed { get; }
        bool FilterFailed { get; }
        bool FilterInconclusive { get; }
        int TestCount { get; }
        TestTreeNode Root { get; }
        bool SortAsc { get; }
        bool SortDesc { get; }
        int Passed { get; }
        int Failed { get; }
        int Skipped { get; }
        int Inconclusive { get; }

        void ApplyFilterSet(FilterSet<ITest> filterSet);
        void BuildTestTree(IProgressMonitor progressMonitor, TestModelData testModelData, 
            TestTreeBuilderOptions options);
        Node FindNode(TreePath path);
        FilterSet<ITest> GenerateFilterSetFromSelectedTests();
        TreePath GetPath(Node node);
        void RemoveFilter(TestStatus testStatus);
        void ResetTestStatus(IProgressMonitor progressMonitor);
        void SetFilter(TestStatus testStatus);
        void SetSortOrder(SortOrder sortOrder);
        void UpdateTestStatus(TestData testData, TestStepRun testStepRun);
    }
}