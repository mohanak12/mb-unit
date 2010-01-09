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

using Gallio.UI.Menus;
using Gallio.UI.ProgressMonitoring;

namespace Gallio.UI.Controls
{
    /// <summary>
    /// Extends the win forms ToolStripMenuItem to wrap a MenuCommand.
    /// </summary>
    public sealed class CommandToolStripMenuItem : System.Windows.Forms.ToolStripMenuItem
    {
        /// <summary>
        /// Constructor providing a command and task manager.
        /// </summary>
        /// <param name="command">The command to use.</param>
        /// <param name="taskManager">The task manager to use.</param>
        public CommandToolStripMenuItem(MenuCommand command, ITaskManager taskManager)
        {
            Text = command.Text;

            Enabled = command.CanExecute;
            command.CanExecute.PropertyChanged += (s, e) => Enabled = command.CanExecute;

            Click += (s, e) => taskManager.QueueTask(command.Command);
        }
    }
}