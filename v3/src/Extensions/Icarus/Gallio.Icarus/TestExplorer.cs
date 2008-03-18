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

using System;
using System.Windows.Forms;

using Aga.Controls.Tree;

using Gallio.Icarus.Controls;
using Gallio.Icarus.Interfaces;
using Gallio.Model;

namespace Gallio.Icarus
{
    public partial class TestExplorer : DockWindow
    {
        private IProjectAdapterView projectAdapterView;
        private TestTreeModel testTreeModel;

        public string TreeFilter
        {
            get
            {
                if (InvokeRequired)
                {
                    string treeFilter = "";
                    Invoke((MethodInvoker)delegate()
                    {
                        treeFilter = TreeFilter;
                    });
                    return treeFilter;
                }
                else
                {
                    return (string)treeViewComboBox.SelectedItem;
                }
            }
        }

        public ITreeModel TreeModel
        {
            set
            {
                testTree.Model = value;
                testTreeModel = (TestTreeModel)((SortedTreeModel)value).InnerModel;
            }
        }

        public TestExplorer(IProjectAdapterView projectAdapterView)
        {
            if (projectAdapterView == null)
                throw new ArgumentNullException("projectAdapterView");

            this.projectAdapterView = projectAdapterView;
            InitializeComponent();
            treeViewComboBox.SelectedIndex = 0;
        }

        private void ExpandTree(TestStatus state)
        {
            testTree.BeginUpdate();
            testTree.CollapseAll();
            foreach (TreeNodeAdv node in testTree.AllNodes)
                TestNodes(node, state);
            testTree.EndUpdate();
        }

        private void TestNodes(TreeNodeAdv node, TestStatus state)
        {
            if (node.Tag is TestTreeNode)
            {
                if (((TestTreeNode)node.Tag).TestStatus == state)
                    ExpandNode(node);
            }

            // Loop though all the child nodes and expand them if they
            // meet the test state.
            foreach (TreeNodeAdv tNode in node.Children)
                TestNodes(tNode, state);
        }

        private void ExpandNode(TreeNodeAdv node)
        {
            // Loop through all parent nodes that are not already
            // expanded and expand them.
            if (node.Parent != null && !node.Parent.IsExpanded)
                ExpandNode(node.Parent);

            node.Expand();
        }

        private void removeAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (testTree.SelectedNode.Tag is TestTreeNode)
            {
                TestTreeNode node = (TestTreeNode)testTree.SelectedNode.Tag;
                projectAdapterView.ThreadedRemoveAssembly(node.Name);
            }
        }

        private void treeViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            projectAdapterView.ReloadTree();
        }

        private void filterPassedTestsToolStripButton_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterPassed = filterPassedTestsToolStripMenuItem.Checked = filterPassedTestsToolStripButton.Checked;
        }

        private void filterFailedTestsToolStripButton_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterFailed = filterFailedTestsToolStripMenuItem.Checked = filterFailedTestsToolStripButton.Checked;
        }

        private void filterSkippedTestsToolStripButton_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterSkipped = filterSkippedTestsToolStripMenuItem.Checked = filterSkippedTestsToolStripButton.Checked;
        }

        private void resetTestsMenuItem_Click(object sender, EventArgs e)
        {
            testTreeModel.ResetTestStatus();
        }

        private void expandAllMenuItem_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        public void ExpandAll()
        {
            testTree.ExpandAll();
        }

        private void collapseAllMenuItem_Click(object sender, EventArgs e)
        {
            testTree.CollapseAll();
        }

        private void sortTree_Click(object sender, EventArgs e)
        {
            //testTree.Sorted = sortTree.Checked;
        }

        private void viewSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectAdapterView.ViewSourceCode(((TestTreeNode)testTree.SelectedNode.Tag).Name);
        }

        private void expandPassedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandTree(TestStatus.Passed);
        }

        private void expandFailedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandTree(TestStatus.Failed);
        }

        private void expandInconclusiveTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandTree(TestStatus.Inconclusive);
        }

        private void filterPassedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterPassed = filterPassedTestsToolStripButton.Checked = filterPassedTestsToolStripMenuItem.Checked;
        }

        private void filterFailedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterFailed = filterFailedTestsToolStripButton.Checked = filterFailedTestsToolStripMenuItem.Checked;
        }

        private void filterSkippedTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTreeModel.FilterSkipped = filterSkippedTestsToolStripButton.Checked = filterSkippedTestsToolStripMenuItem.Checked;
        }

        private void addAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectAdapterView.AddAssembliesToTree();
        }

        private void removeAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectAdapterView.RemoveAssembliesFromTree();
        }

        private void testTree_SelectionChanged(object sender, EventArgs e)
        {
            if (testTree.SelectedNode != null)
            {
                removeAssemblyToolStripMenuItem.Enabled = (((TestTreeNode)testTree.SelectedNode.Tag).NodeTypeIcon == global::Gallio.Icarus.Properties.Resources.Assembly);
                viewSourceCodeToolStripMenuItem.Enabled = ((TestTreeNode)testTree.SelectedNode.Tag).SourceCodeAvailable;
            }
        }
    }
}