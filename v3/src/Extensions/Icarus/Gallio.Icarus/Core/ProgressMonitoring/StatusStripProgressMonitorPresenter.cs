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
using System.Text;
using Gallio.Icarus.Core.Interfaces;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Icarus.Core.CustomEventArgs;

namespace Gallio.Icarus.Core.ProgressMonitoring
{
    public class StatusStripProgressMonitorPresenter : BaseProgressMonitorPresenter
    {
        public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;

        protected override void Initialize()
        {
            ProgressMonitor.TaskStarting += HandleTaskStarting;
            ProgressMonitor.Changed += HandleChanged;
        }

        private void HandleTaskStarting(object sender, EventArgs e)
        {
            if (ProgressUpdate != null)
                ProgressUpdate(this, new ProgressUpdateEventArgs(string.Empty, string.Empty, 0, ProgressMonitor.TotalWorkUnits));
        }

        /// <inheritdoc />
        private void HandleChanged(object sender, EventArgs e)
        {
            if (ProgressMonitor.IsCanceled)
            {
                if (ProgressUpdate != null)
                    ProgressUpdate(this, new ProgressUpdateEventArgs("Operation cancelled", string.Empty, 0, 0));
            }
            else if (ProgressMonitor.IsDone)
            {
                if (ProgressUpdate != null)
                    ProgressUpdate(this, new ProgressUpdateEventArgs(string.Empty, string.Empty, 0, 0));
            }
            else
            {
                if (ProgressUpdate != null)
                    ProgressUpdate(this, new ProgressUpdateEventArgs(ProgressMonitor.TaskName, ProgressMonitor.LeafSubTaskName, 
                        ProgressMonitor.CompletedWorkUnits, ProgressMonitor.TotalWorkUnits));
            }
        }
    }
}