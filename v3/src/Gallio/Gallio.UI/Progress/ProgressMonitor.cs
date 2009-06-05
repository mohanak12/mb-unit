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

using System;
using System.Windows.Forms;
using Gallio.Common.Concurrency;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.UI.Progress
{
    ///<summary>
    /// Progress dialog.
    ///</summary>
    public partial class ProgressMonitor : Form
    {
        private readonly ObservableProgressMonitor progressMonitor;

        ///<summary>
        /// Default constructor.
        ///</summary>
        ///<param name="progressMonitor">The progress monitor to display information for.</param>
        public ProgressMonitor(ObservableProgressMonitor progressMonitor)
        {
            InitializeComponent();

            this.progressMonitor = progressMonitor;

            progressMonitor.Changed += (sender, e) => Sync.Invoke(this, ProgressUpdate);
        }

        private void ProgressUpdate()
        {
            // update task details
            progressBar.Maximum = double.IsNaN(progressMonitor.TotalWorkUnits)
                ? 0 : Convert.ToInt32(progressMonitor.TotalWorkUnits);
            progressBar.Value = Convert.ToInt32(progressMonitor.CompletedWorkUnits);
            Text = progressMonitor.TaskName;
            subTaskNameLabel.Text = progressMonitor.LeafSubTaskName;
            percentLabel.Text = (progressMonitor.TotalWorkUnits > 0) ? 
                String.Format("({0:P0})", progressMonitor.CompletedWorkUnits / 
                progressMonitor.TotalWorkUnits) : String.Empty;

            // if we're finished, then close the window
            if (progressMonitor.CompletedWorkUnits == progressMonitor.TotalWorkUnits)
                Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            progressMonitor.Cancel();
        }

        private void runInBackgroundButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}