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
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Gallio.Icarus.Properties;
using Gallio.Model;
using Gallio.Runner.Reports.Schema;
using Gallio.Runtime;

namespace Gallio.Icarus.Models
{
    public class TestTreeNode : Node
    {
        // TODO: Refactor me.
        private static readonly object imageCacheLock = new object();
        private static Dictionary<string, Image> imageCache;

        private TestStatus testStatus = TestStatus.Skipped;
        private readonly List<TestStepRun> testStepRuns = new List<TestStepRun>();
        private string testKind;

        public string Name { get; private set; }

        public string FileName { get; protected set; }

        public TestStatus TestStatus
        {
            get { return testStatus; }
            set
            {
                testStatus = value;
                TestStatusIcon = GetTestStatusIcon(value);
                NotifyModel();
                UpdateParentTestStatus();
            }
        }

        public bool SourceCodeAvailable { get; set; }

        public bool IsTest { get; set; }

        public string TestKind 
        {
            get
            {
                return testKind;
            }
            set
            {
                testKind = value;
                NodeTypeIcon = GetNodeTypeImage(testKind);
            }
        }

        /// <summary>
        /// Returns the 'combined' state for all siblings of a node.
        /// </summary>
        private CheckState SiblingsState
        {
            get
            {
                // If parent is null, cannot have any siblings or if the parent
                // has only one child (i.e. this node) then return the state of this 
                // instance as the state.
                if (Parent == null || Parent.Nodes.Count == 1)
                    return CheckState;

                // The parent has more than one child.  Walk through parent's child
                // nodes to determine the state of all this node's siblings,
                // including this node.
                foreach (var node in Parent.Nodes)
                {
                    var child = node as TestTreeNode;
                    if (child != null && CheckState != child.CheckState)
                        return CheckState.Indeterminate;
                }
                return CheckState;
            }
        }

        private TestStatus SiblingTestStatus
        {
            get
            {
                if (Parent == null || Parent.Nodes.Count == 1)
                    return TestStatus;

                var ts = TestStatus.Skipped;
                foreach (var node in Parent.Nodes)
                {
                    var child = node as TestTreeNode;
                    if (child == null)
                        continue;

                    if (child.TestStatus == TestStatus.Failed)
                        return TestStatus.Failed;
                    if (child.TestStatus == TestStatus.Inconclusive)
                        ts = TestStatus.Inconclusive;
                    if (child.TestStatus == TestStatus.Passed && ts != TestStatus.Inconclusive)
                        ts = TestStatus.Passed;
                }
                return ts;
            }
        }

        public Image NodeTypeIcon { get; protected set; }

        public Image TestStatusIcon { get; private set; }

        public List<TestStepRun> TestStepRuns
        {
            get { return testStepRuns; }
        }

        public TestTreeNode(string id, string name)
            : base(name)
        {
            this.Name = id;
        }

        private static Image GetNodeTypeImage(string nodeType)
        {
            if (nodeType == null)
                return null;

            lock (imageCacheLock)
            {
                if (imageCache == null)
                {
                    imageCache = new Dictionary<string, Image>();

                    var testKindManager = RuntimeAccessor.ServiceLocator.Resolve<ITestKindManager>();
                    foreach (var handle in testKindManager.TestKindHandles)
                    {
                        TestKindTraits traits = handle.GetTraits();
                        Image image = traits.Icon != null
                            ? new Icon(traits.Icon, 16, 16).ToBitmap()
                            : Resources.Group;

                        imageCache.Add(traits.Name, image);
                    }
                }

                Image nodeTypeImage;
                imageCache.TryGetValue(nodeType, out nodeTypeImage);
                return nodeTypeImage;
            }
        }

        public List<TestTreeNode> Find(string key, bool searchChildren)
        {
            var nodes = new List<TestTreeNode>();

            if (Name == key)
                nodes.Add(this);

            // always search one level deep...
            foreach (var n in Nodes)
                nodes.AddRange(Find(key, searchChildren, n));

            return nodes;
        }

        private static List<TestTreeNode> Find(string key, bool searchChildren, Node node)
        {
            var nodes = new List<TestTreeNode>();
            if (node is TestTreeNode)
            {
                var ttnode = (TestTreeNode)node;
                if (ttnode.Name == key)
                    nodes.Add(ttnode);

                // continue down the tree if necessary
                if (searchChildren)
                    foreach (var n in node.Nodes)
                        nodes.AddRange(Find(key, true, n));
            }
            return nodes;
        }

        /// <summary>
        /// Manages updating related child and parent nodes of this instance.
        /// </summary>
        public void UpdateStateOfRelatedNodes()
        {
            // If you want to cascade checkbox state changes to children of this node and
            // the current state is not indeterminate, then update the state of child nodes.
            if (CheckState != CheckState.Indeterminate)
                UpdateChildNodeState();

            UpdateParentNodeState(true);
        }

        /// <summary>
        /// Recursively update child node's state based on the state of this node.
        /// </summary>
        private void UpdateChildNodeState()
        {
            foreach (var node in Nodes)
            {
                // It is possible node is not a ThreeStateTreeNode, so check first.
                var child = node as TestTreeNode;
                if (child == null)
                    continue;
                child.CheckState = CheckState;
                child.UpdateChildNodeState();
            }
        }

        /// <summary>
        /// Recursively update parent node state based on the current state of this node.
        /// </summary>
        private void UpdateParentNodeState(bool isStartingPoint)
        {
            // If isStartingPoint is false, then know this is not the initial call
            // to the recursive method as we want to force on the first time
            // this is called to set the instance's parent node state based on
            // the state of all the siblings of this node, including the state
            // of this node.  So, if not the startpoint (!isStartingPoint) and
            // the state of this instance is indeterminate (Enumerations.CheckBoxState.Indeterminate)
            // then know to set all subsequent parents to the indeterminate
            // state.  However, if not in an indeterminate state, then still need
            // to evaluate the state of all the siblings of this node, including the state
            // of this node before setting the state of the parent of this instance.
            var parent = Parent as TestTreeNode;
            if (parent == null)
                return;
            CheckState state;

            // Determine the new state
            if (!isStartingPoint && (CheckState == CheckState.Indeterminate))
                state = CheckState.Indeterminate;
            else
                state = SiblingsState;

            // Update parent state if not the same.
            if (parent.CheckState == state)
                return;
            parent.CheckState = state;
            parent.UpdateParentNodeState(false);
        }

        private void UpdateParentTestStatus()
        {
            var parent = Parent as TestTreeNode;
            if (parent != null)
                parent.TestStatus = SiblingTestStatus;
        }

        private static Image GetTestStatusIcon(TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Failed:
                    return Properties.Resources.cross;
                case TestStatus.Passed:
                    return Properties.Resources.tick;
                case TestStatus.Inconclusive:
                    return Properties.Resources.error;
                default:
                    return null;
            }
        }

        public void AddTestStepRun(TestStepRun testStepRun)
        {
            testStepRuns.Add(testStepRun);
            
            // combine test status
            if (testStepRun.Result.Outcome.Status > TestStatus || testStepRun.Step.IsPrimary)
                TestStatus = testStepRun.Result.Outcome.Status;
        }

        public void Reset()
        {
            testStatus = TestStatus.Skipped;
            TestStatusIcon = GetTestStatusIcon(TestStatus.Skipped);

            testStepRuns.Clear();

            foreach (var n in Nodes)
                ((TestTreeNode)n).Reset();
        }
    }
}
