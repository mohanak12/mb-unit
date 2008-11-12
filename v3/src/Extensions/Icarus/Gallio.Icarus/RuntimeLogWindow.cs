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

using System.Drawing;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Utilities;

namespace Gallio.Icarus
{
    public partial class RuntimeLogWindow : DockWindow
    {
        public RuntimeLogWindow(IRuntimeLogController runtimeLogController)
        {
            InitializeComponent();

            runtimeLogController.LogMessage += runtimeLogController_LogMessage;
        }

        void runtimeLogController_LogMessage(object sender, Controllers.EventArgs.RuntimeLogEventArgs e)
        {
            //Sync.Invoke(this, () => AppendTextLine(e.Message, e.Color));
        }

        private void AppendTextLine(string text, Color color)
        {
            AppendText(text, color);
            AppendText("\n", color);
        }

        private void AppendText(string text, Color color)
        {
            int start = logBody.TextLength;
            logBody.AppendText(text);
            int end = logBody.TextLength;
           
            // Textbox may transform chars, so (end-start) != text.Length
            logBody.Select(start, end - start);
            {
                logBody.SelectionColor = color;
                // could set box.SelectionBackColor, box.SelectionFont too.
            }
            // clear selection
            logBody.SelectionLength = 0;
        }

        private void clearAllToolStripButton_Click(object sender, System.EventArgs e)
        {
            logBody.Clear();
        }
    }
}
