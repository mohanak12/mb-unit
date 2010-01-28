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
using System.Windows.Forms;
using Gallio.Common.Concurrency;
using Gallio.Icarus.Models;
using Gallio.Icarus.Models.TestTreeNodes;
using Gallio.Icarus.Utilities;
using Gallio.Model;
using SortOrder=Gallio.Icarus.Models.SortOrder;

namespace Gallio.Icarus.TestExplorer
{
    internal partial class View : DockWindow
    {
        private readonly IController controller;
        private readonly IModel model;
        private bool updateFlag;

        public View(IController controller, IModel model)
        {
            this.controller = controller;
            this.model = model;

            InitializeComponent();

            testTree.PassedColor = model.PassedColor;
            model.PassedColor.PropertyChanged += (s, e) => testTree.PassedColor = model.PassedColor;
            
            testTree.FailedColor = model.FailedColor;
            model.FailedColor.PropertyChanged += (s, e) => testTree.FailedColor = model.FailedColor;
            
            testTree.InconclusiveColor = model.InconclusiveColor;
            model.InconclusiveColor.PropertyChanged += (s, e) => testTree.InconclusiveColor = model.InconclusiveColor;
            
            testTree.SkippedColor = model.SkippedColor;
            model.SkippedColor.PropertyChanged += (s, e) => testTree.SkippedColor = model.SkippedColor;

            if (treeViewComboBox.ComboBox != null)
            {
                updateFlag = true;
                treeViewComboBox.ComboBox.DataSource = model.TreeViewCategories.Value;
                updateFlag = false;
                treeViewComboBox.ComboBox.SelectedItem = model.CurrentTreeViewCategory;
            }

            model.TreeViewCategories.PropertyChanged += (s, e) =>
            {
                if (treeViewComboBox.ComboBox == null) 
                    return;

                updateFlag = true;
                treeViewComboBox.ComboBox.DataSource = model.TreeViewCategories;
                updateFlag = false;
            };

            testTree.Model = model.TreeModel;

            controller.RestoreState += (s,e) => RestoreState();

            model.CanEditTree.PropertyChanged += (s, e) => testTree.EditEnabled = model.CanEditTree;

            filterPassedTestsToolStripMenuItem.Click += (s, e) => FilterStatus(TestStatus.Passed);
            filterPassedTestsToolStripButton.Click += (s, e) => FilterStatus(TestStatus.Passed);
            model.FilterPassed.PropertyChanged += (s, e) => 
            {
                filterPassedTestsToolStripMenuItem.Checked = filterPassedTestsToolStripButton.Checked 
                    = model.FilterPassed.Value;
            };

            filterFailedTestsToolStripMenuItem.Click += (s, e) => FilterStatus(TestStatus.Failed);
            filterFailedTestsToolStripButton.Click += (s, e) => FilterStatus(TestStatus.Failed);
            model.FilterFailed.PropertyChanged += (s, e) =>
            {
                filterFailedTestsToolStripMenuItem.Checked = filterFailedTestsToolStripButton.Checked 
                    = model.FilterFailed.Value;
            };

            filterInconclusiveTestsToolStripMenuItem.Click += (s, e) => FilterStatus(TestStatus.Inconclusive);
            filterInconclusiveTestsToolStripButton.Click += (s, e) => FilterStatus(TestStatus.Inconclusive);
            model.FilterInconclusive.PropertyChanged += (s, e) =>
            {
                filterInconclusiveTestsToolStripMenuItem.Checked = filterInconclusiveTestsToolStripButton.Checked 
                    = model.FilterInconclusive.Value;
            };

            sortAscToolStripButton.Click += (s, e) => SortTree(SortOrder.Ascending);
            sortDescToolStripButton.Click += (s, e) => SortTree(SortOrder.Descending);

            controller.SaveState += (s, e) => SaveState();
        }

        private void FilterStatus(TestStatus testStatus)
        {
            controller.FilterStatus(testStatus);
        }

        private void SortTree(SortOrder sortOrder)
        {
            sortAscToolStripButton.Checked = (sortOrder == SortOrder.Ascending);
            sortDescToolStripButton.Checked = (sortOrder == SortOrder.Descending);
            controller.SortTree(sortOrder);    
        }

        private void removeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (testTree.SelectedNode == null || !(testTree.SelectedNode.Tag is TestTreeNode))
                return;

            var node = (TestTreeNode)testTree.SelectedNode.Tag;

            if (node.FileName != null)
                controller.RemoveFile(node.FileName);
        }

        private void treeViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            model.CurrentTreeViewCategory.Value = (string)treeViewComboBox.SelectedItem;

            // if updateFlag is set, then the index has changed because
            // we are populating the list, so no need to refresh!
            if (updateFlag)
                return;

            SaveState();
            controller.RefreshTree();
            RestoreState();
        }

        private void resetTestsMenuItem_Click(object sender, EventArgs e)
        {
            controller.ResetTests();
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
            ViewSourceCode();
        }

        private void ViewSourceCode()
        {
            if (testTree.SelectedNode == null || !(testTree.SelectedNode.Tag is TestTreeNode))
                return;

            var node = (TestTreeNode)testTree.SelectedNode.Tag;

            controller.ShowSourceCode(node.Id);
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

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = Dialogs.CreateAddFilesDialog())
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                controller.AddFiles(openFileDialog.FileNames);
            }
        }

        private void removeAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.RemoveAllFiles();
        }

        private void testTree_SelectionChanged(object sender, EventArgs e)
        {
            Sync.Invoke(this, TreeSelectionChanged);
        }

        private void TreeSelectionChanged()
        {
            var nodes = new List<TestTreeNode>();

            if (testTree.SelectedNode != null)
            {
                var testTreeNode = (TestTreeNode)testTree.SelectedNode.Tag;
                removeFileToolStripMenuItem.Enabled = testTreeNode.FileName != null;
                viewSourceCodeToolStripMenuItem.Enabled = testTreeNode.SourceCodeAvailable;

                nodes.AddRange(GetSelectedNodes(testTreeNode));
            }
            else
            {
                removeFileToolStripMenuItem.Enabled = false;
                viewSourceCodeToolStripMenuItem.Enabled = false;
            }

            controller.SetTreeSelection(nodes);
        }

        private static IEnumerable<TestTreeNode> GetSelectedNodes(TestTreeNode testTreeNode)
        {
            var nodes = new List<TestTreeNode>();

            if (testTreeNode is NamespaceNode)
            {
                nodes.AddRange(((NamespaceNode)testTreeNode).GetChildren());
            }
            else
            {
                nodes.Add(testTreeNode);
            }
            return nodes;
        }

        private void SaveState()
        {            
            model.CollapsedNodes.Value = testTree.CollapsedNodes;
        }

        private void RestoreState()
        {
            testTree.CollapseNodes(model.CollapsedNodes.Value);
        }

        private void testTree_DoubleClick(object sender, EventArgs e)
        {
            ViewSourceCode();
        }

        private void selectFailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Select(TestStatus.Failed);
        }

        private void selectPassedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Select(TestStatus.Passed);
        }

        private void selectInconclusiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testTree.Select(TestStatus.Inconclusive);
        }
    }
}
