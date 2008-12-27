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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Gallio.Icarus.Mediator.Interfaces;
using Gallio.Icarus.Models;
using Gallio.Model;

namespace Gallio.Icarus
{
    public partial class TestExplorer : DockWindow, IMessageFilter
    {
        private readonly IMediator mediator;
        private readonly bool updateFlag;
        private const int WM_DROPFILES = 563;

        public TestExplorer(IMediator mediator)
        {
            this.mediator = mediator;

            InitializeComponent();

            testTree.OptionsController = mediator.OptionsController;

            if (treeViewComboBox.ComboBox != null)
            {
                updateFlag = true;
                treeViewComboBox.ComboBox.BindingContext = BindingContext;
                treeViewComboBox.ComboBox.DataSource = mediator.OptionsController.SelectedTreeViewCategories;
                treeViewComboBox.ComboBox.DataBindings.Add("SelectedItem", mediator.ProjectController, "TreeViewCategory");
                updateFlag = false;
            }

            testTree.Model = mediator.TestController.Model;

            mediator.TestController.LoadStarted += delegate { testTree.EditEnabled = false; };
            mediator.TestController.LoadFinished += delegate
            {
                testTree.EditEnabled = true;
                RestoreState();
            };

            mediator.TestController.RunStarted += delegate { testTree.EditEnabled = false; };
            mediator.TestController.RunFinished += delegate { testTree.EditEnabled = true; };

            filterPassedTestsToolStripMenuItem.DataBindings.Add("Checked", mediator.TestController, "Model.FilterPassed", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            filterPassedTestsToolStripButton.DataBindings.Add("Checked", mediator.TestController, "Model.FilterPassed", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            filterFailedTestsToolStripMenuItem.DataBindings.Add("Checked", mediator.TestController, "Model.FilterFailed", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            filterFailedTestsToolStripButton.DataBindings.Add("Checked", mediator.TestController, "Model.FilterFailed", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            filterSkippedTestsToolStripMenuItem.DataBindings.Add("Checked", mediator.TestController, "Model.FilterSkipped", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            filterSkippedTestsToolStripButton.DataBindings.Add("Checked", mediator.TestController, "Model.FilterSkipped", 
                false, DataSourceUpdateMode.OnPropertyChanged);

            sortAscToolStripButton.DataBindings.Add("Checked", mediator.TestController, "Model.SortAsc", 
                false, DataSourceUpdateMode.OnPropertyChanged);
            sortDescToolStripButton.DataBindings.Add("Checked", mediator.TestController, "Model.SortDesc", 
                false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void removeAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(testTree.SelectedNode.Tag is TestTreeNode))
                return;

            TestTreeNode node = (TestTreeNode)testTree.SelectedNode.Tag;
            mediator.RemoveAssembly(node.Name);
        }

        private void treeViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mediator.TestController.TreeViewCategory = (string)treeViewComboBox.SelectedItem;
            
            if (updateFlag)
                return;

            SaveState();
            mediator.RefreshTestTree();
            RestoreState();
        }

        private void resetTestsMenuItem_Click(object sender, EventArgs e)
        {
            mediator.ResetTests();
        }

        private void expandAllMenuItem_Click(object sender, EventArgs e)
        {
            testTree.ExpandAll();
        }

        private void collapseAllMenuItem_Click(object sender, EventArgs e)
        {
            testTree.CollapseAll();
        }

        private void viewSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.ViewSourceCode(((TestTreeNode)testTree.SelectedNode.Tag).Name);
        }

        private void expandPassedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Expand(TestStatus.Passed);
        }

        private void expandFailedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Expand(TestStatus.Failed);
        }

        private void expandInconclusiveTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Expand(TestStatus.Inconclusive);
        }

        private void addAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ParentForm != null)
                ((Main)ParentForm).AddAssembliesToTree();
        }

        private void removeAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.RemoveAllAssemblies();
        }

        private void testTree_SelectionChanged(object sender, EventArgs e)
        {
            mediator.TestController.SelectedTests.Clear();
            if (testTree.SelectedNode != null)
            {
                TestTreeNode testTreeNode = (TestTreeNode)testTree.SelectedNode.Tag;
                removeAssemblyToolStripMenuItem.Enabled = testTreeNode.NodeType == TestKinds.Assembly;
                viewSourceCodeToolStripMenuItem.Enabled = testTreeNode.SourceCodeAvailable;
                if (testTreeNode.NodeType == TestKinds.Namespace)
                {
                    foreach (Node n in testTreeNode.Nodes)
                        if (n != null) // don't know how this happens yet -- JB
                            mediator.TestController.SelectedTests.Add((TestTreeNode)n);
                }
                else
                    mediator.TestController.SelectedTests.Add(testTreeNode);
            }
            else
            {
                removeAssemblyToolStripMenuItem.Enabled = false;
                viewSourceCodeToolStripMenuItem.Enabled = false;
            }
        }

        internal void SaveState()
        {
            mediator.SaveFilter("AutoSave");
            mediator.ProjectController.TreeViewCategory = (string)treeViewComboBox.SelectedItem;
            mediator.ProjectController.CollapsedNodes = testTree.CollapsedNodes;
        }

        private void RestoreState()
        {
            testTree.CollapseNodes(mediator.ProjectController.CollapsedNodes);
        }

        [DllImport("shell32.dll")]
        private static extern int DragQueryFile(IntPtr hdrop, int ifile, StringBuilder fname, int fnsize);
        [DllImport("shell32.dll")]
        private static extern void DragFinish(IntPtr hdrop);

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_DROPFILES)
            {
                int nFiles = DragQueryFile(m.WParam, -1, null, 0);
                for (int i = 0; i < nFiles; ++i)
                {
                    StringBuilder sb = new StringBuilder(256);
                    DragQueryFile(m.WParam, i, sb, 256);
                    mediator.AddAssemblies(new[] { sb.ToString() });
                }
                DragFinish(m.WParam);
                return true;
            }
            return false;
        }
    }
}
