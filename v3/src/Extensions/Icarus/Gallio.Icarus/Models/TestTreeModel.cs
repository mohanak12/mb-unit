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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Gallio.Icarus.Models.Interfaces;
using Gallio.Model;
using Gallio.Model.Filters;
using Gallio.Model.Serialization;
using Gallio.Reflection;
using Gallio.Runner.Reports;

namespace Gallio.Icarus.Models
{
    public class TestTreeModel : TreeModel, ITestTreeModel
    {
        private bool filterPassed, filterFailed, filterSkipped;
        private readonly TestTreeSorter testTreeSorter = new TestTreeSorter();

        public event EventHandler<EventArgs> TestCountChanged;

        public bool FilterPassed
        {
            set
            {
                filterPassed = value;
                if (value)
                    FilterTree();
                else
                    ClearFilter(TestStatus.Passed);
                OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
            }
        }

        public bool FilterFailed
        {
            set
            {
                filterFailed = value;
                if (value)
                    FilterTree();
                else
                    ClearFilter(TestStatus.Failed);
                OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
            }
        }

        public bool FilterSkipped
        {
            set
            {
                filterSkipped = value;
                if (value)
                    FilterTree();
                else
                    ClearFilter(TestStatus.Skipped);
                OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
            }
        }

        public SortOrder SortOrder
        {
            get { return testTreeSorter.SortOrder; }
            set
            {
                testTreeSorter.SortOrder = value;
                OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
            }
        }

        public int TestCount
        {
            get
            {
                int count = 0;
                foreach (Node node in Nodes)
                    count += CountTests(node);
                return count;
            }
        }

        private static int CountTests(Node node)
        {
            int count = 0;
            if (node is TestTreeNode && ((TestTreeNode)node).IsTest && node.IsChecked)
                count += 1;
            foreach (Node n in node.Nodes)
                count += CountTests(n);
            return count;
        }

        public new TestTreeNode Root
        {
            get { return (TestTreeNode)Nodes[0]; }
        }

        public void ResetTestStatus()
        {
            foreach (Node node in Nodes)
                ResetTestStatus(node);
        }

        private static void ResetTestStatus(Node node)
        {
            ((TestTreeNode)node).ClearTestStepRuns();
            foreach (Node n in node.Nodes)
                ResetTestStatus(n);
        }

        public void UpdateTestStatus(TestData testData, TestStepRun testStepRun)
        {
            List<TestTreeNode> nodes = Root.Find(testData.Id, true);
            foreach (TestTreeNode node in nodes)
            {
                node.AddTestStepRun(testStepRun);
                Filter(node);
            }
        }

        public void FilterTree()
        {
            foreach (Node node in Nodes)
                Filter(node);
        }

        private bool Filter(Node n)
        {
            if (n is TestTreeNode && (filterPassed || filterFailed || filterSkipped))
            {
                TestTreeNode node = (TestTreeNode)n;
                
                // only filter leaf nodes
                if (n.Nodes.Count == 0)
                {
                    switch (node.TestStatus)
                    {
                        case TestStatus.Passed:
                            if (filterPassed)
                            {
                                FilterNode(node, "Passed", TestStatus.Passed, "FilterPassed");
                                return false;
                            }
                            break;
                        case TestStatus.Skipped:
                            if (filterSkipped)
                            {
                                FilterNode(node, "Skipped", TestStatus.Skipped, "FilterSkipped");
                                return false;
                            }
                            break;
                        case TestStatus.Failed:
                            if (filterFailed)
                            {
                                FilterNode(node, "Failed", TestStatus.Failed, "FilterFailed");
                                return false;
                            }
                            break;
                    }
                }
                else if (node.Name != TestStatus.Passed.ToString() && node.Name != TestStatus.Failed.ToString()
                    && node.Name != TestStatus.Inconclusive.ToString())
                {
                    int i = 0;
                    while (i < node.Nodes.Count)
                    {
                        if (Filter(node.Nodes[i]))
                            i++;
                    }
                }
            }
            return true;
        }

        private static void FilterNode(Node node, string text, TestStatus testStatus, string nodeType)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            string key = testStatus.ToString();
            TestTreeNode filterNode;
            List<TestTreeNode> nodes = ((TestTreeNode)node.Parent).Find(key, true);
            if (nodes.Count > 0)
                filterNode = nodes[0];
            else
            {
                filterNode = new TestTreeNode(text, key, nodeType);
                filterNode.TestStatus = testStatus;
                node.Parent.Nodes.Add(filterNode);
            }
            node.Parent.Nodes.Remove(node);
            filterNode.Nodes.Add(node);
        }

        private void ClearFilter(TestStatus testStatus)
        {
            foreach (TestTreeNode filterNode in Root.Find(testStatus.ToString(), true))
            {
                Node[] nodes = new Node[filterNode.Nodes.Count];
                filterNode.Nodes.CopyTo(nodes, 0);
                foreach (Node n in nodes)
                    filterNode.Parent.Nodes.Add(n);
                filterNode.Parent.Nodes.Remove(filterNode);
            }
        }

        public void OnTestCountChanged(EventArgs e)
        {
            if (TestCountChanged != null)
                TestCountChanged(this, e);
        }

        public new IEnumerable GetChildren(TreePath treePath)
        {
            if (testTreeSorter.SortOrder != SortOrder.None)
            {
                ArrayList list = new ArrayList();
                IEnumerable res = base.GetChildren(treePath);
                if (res != null)
                {
                    foreach (object obj in res)
                        list.Add(obj);
                    list.Sort(testTreeSorter);
                    return list;
                }
                return null;
            }
            return base.GetChildren(treePath);
        }

        public void BuildTestTree(TestModelData testModelData, string treeViewCategory)
        {
            Nodes.Clear();
            TestTreeNode root = new TestTreeNode(testModelData.RootTest.Name, testModelData.RootTest.Id, TestKinds.Root);
            Nodes.Add(root);
            if (treeViewCategory == "Namespace")
                PopulateNamespaceTree(testModelData.RootTest.Children, root);
            else
                PopulateMetadataTree(treeViewCategory, testModelData.RootTest.Children, root);
            OnTestCountChanged(EventArgs.Empty);
        }

        private static void PopulateNamespaceTree(IList<TestData> list, TestTreeNode parent)
        {
            for (int i = 0; i < list.Count; i++)
            {
                TestData td = list[i];
                string componentKind = td.Metadata.GetValue(MetadataKeys.TestKind);
                if (componentKind == null)
                    continue;
                TestTreeNode ttnode;
                if (componentKind != TestKinds.Fixture)
                {
                    // create an appropriate node
                    ttnode = new TestTreeNode(td.Name, td.Id, componentKind);
                    parent.Nodes.Add(ttnode);
                }
                else
                {
                    // fixtures need special treatment to insert the namespace layer!
                    string @namespace = td.CodeReference.NamespaceName ?? "";

                    // find the namespace node (or add if it doesn't exist)
                    TestTreeNode nsNode;
                    List<TestTreeNode> nodes = parent.Find(@namespace, true);
                    if (nodes.Count > 0)
                        nsNode = nodes[0];
                    else
                    {
                        nsNode = new TestTreeNode(@namespace, @namespace, "Namespace");
                        parent.Nodes.Add(nsNode);
                    }

                    // add the fixture to the namespace
                    ttnode = new TestTreeNode(td.Name, td.Id, componentKind);
                    nsNode.Nodes.Add(ttnode);
                }
                ttnode.SourceCodeAvailable = (td.CodeLocation != CodeLocation.Unknown);
                ttnode.IsTest = td.IsTestCase;

                // process child nodes
                PopulateNamespaceTree(td.Children, ttnode);
            }
        }

        private void PopulateMetadataTree(string key, IList<TestData> list, Node parent)
        {
            for (int i = 0; i < list.Count; i++)
            {
                TestData td = list[i];
                string componentKind = td.Metadata.GetValue(MetadataKeys.TestKind);
                if (componentKind == null)
                    continue;
                switch (componentKind)
                {
                    case TestKinds.Fixture:
                    case TestKinds.Test:
                        IList<string> metadata = td.Metadata[key];
                        if (metadata.Count == 0)
                        {
                            metadata = new List<string>();
                            metadata.Add("None");
                        }
                        foreach (string m in metadata)
                        {
                            // find metadata node (or add if it doesn't exist)
                            TestTreeNode metadataNode;
                            List<TestTreeNode> nodes = Root.Find(m, false);
                            if (nodes.Count > 0)
                                metadataNode = nodes[0];
                            else
                            {
                                metadataNode = new TestTreeNode(m, m, key);
                                Root.Nodes.Add(metadataNode);
                            }

                            // add node in the appropriate place
                            if (componentKind == TestKinds.Fixture)
                            {
                                TestTreeNode ttnode = new TestTreeNode(td.Name, td.Id, componentKind);
                                metadataNode.Nodes.Add(ttnode);
                                PopulateMetadataTree(key, td.Children, ttnode);
                            }
                            else
                            {
                                // test
                                TestTreeNode ttnode = new TestTreeNode(td.Name, td.Id, componentKind);
                                ttnode.SourceCodeAvailable = (td.CodeLocation != CodeLocation.Unknown);
                                ttnode.IsTest = td.IsTestCase;
                                if (m != "None")
                                    metadataNode.Nodes.Add(ttnode);
                                else
                                    parent.Nodes.Add(ttnode);
                            }
                        }
                        break;
                }
                if (componentKind != TestKinds.Fixture)
                    PopulateMetadataTree(key, td.Children, parent);
            }
        }

        public void ApplyFilter(Filter<ITest> filter)
        {
            if ((filter is AnyFilter<ITest>))
                return;

            // toggle root node
            foreach (Node node in Nodes)
            {
                if (!(node is TestTreeNode))
                    continue;
                node.CheckState = CheckState.Unchecked;
                ((TestTreeNode)node).UpdateStateOfRelatedNodes();
            }
            RecursivelyApplyFilter(filter);
            OnTestCountChanged(EventArgs.Empty);
        }

        private void RecursivelyApplyFilter(Filter<ITest> filter)
        {
            if (filter is NoneFilter<ITest>)
                return;
            if (filter is OrFilter<ITest>)
            {
                OrFilter<ITest> orFilter = (OrFilter<ITest>)filter;
                foreach (Filter<ITest> childFilter in orFilter.Filters)
                    RecursivelyApplyFilter(childFilter);
            }
            else if (filter is IdFilter<ITest>)
            {
                IdFilter<ITest> idFilter = (IdFilter<ITest>)filter;
                EqualityFilter<string> equalityFilter = (EqualityFilter<string>)idFilter.ValueFilter;
                foreach (TestTreeNode n in Root.Find(equalityFilter.Comparand, true))
                {
                    n.CheckState = CheckState.Checked;
                    n.UpdateStateOfRelatedNodes();
                }
            }
        }

        private class TestTreeSorter : IComparer
        {
            /// <summary>
            /// Specifies the order in which to sort (i.e. 'Ascending').
            /// </summary>
            private SortOrder sortOrder = SortOrder.None;
            /// <summary>
            /// Case insensitive comparer object
            /// </summary>
            private readonly CaseInsensitiveComparer caseInsensitiveComparer = new CaseInsensitiveComparer();

            /// <summary>
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
            /// </summary>
            public SortOrder SortOrder
            {
                get { return sortOrder; }
                set { sortOrder = value; }
            }

            public int Compare(object x, object y)
            {
                // Cast the objects to be compared to ListViewItem objects
                TestTreeNode X = (TestTreeNode)x;
                TestTreeNode Y = (TestTreeNode)y;

                // standard text sort (ci)
                int compareResult = caseInsensitiveComparer.Compare(X.Text, Y.Text);

                // Calculate correct return value based on object comparison
                if (SortOrder == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                if (SortOrder == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                // Return '0' to indicate they are equal
                return 0;
            }
        }
    }
}
