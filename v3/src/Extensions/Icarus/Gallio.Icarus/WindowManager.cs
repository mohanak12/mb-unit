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
using System.Drawing;
using System.Windows.Forms;
using Gallio.Common;
using Gallio.Icarus.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace Gallio.Icarus
{
    public class WindowManager : IWindowManager
    {
        private readonly Dictionary<string, Window> windows = new Dictionary<string, Window>();
        private readonly Dictionary<string, Action> hooks = new Dictionary<string, Action>();

        public DockPanel DockPanel 
        {
            get;
            internal set;
        }

        public ToolStripItemCollection StatusStrip
        {
            get;
            internal set;
        }

        public ToolStripContainer ToolStrip
        {
            get;
            internal set;
        }

        public ToolStripItemCollection Menu
        {
            get;
            internal set;
        }

        public Window Add(string identifier, Control content, string caption)
        {
            return Add(identifier, content, caption, null);
        }

        public Window Add(string identifier, Control content, string caption, Icon icon)
        {
            if (windows.ContainsKey(identifier))
                throw new Exception("Identifier is not unique");

            var window = new Window(identifier, content, caption);
            if (icon != null)
                window.Icon = icon;

            window.FormClosed += (sender, e) => Remove(window.Id);

            windows.Add(identifier, window);

            return window;
        }

        public void Remove(string identifier)
        {
            if (windows.ContainsKey(identifier))
                windows.Remove(identifier);
        }

        internal Window Get(string identifier)
        {
            // if we have the window stored, then return it
            if (windows.ContainsKey(identifier))
                return windows[identifier];

            // if we have an action registered, run it
            if (hooks.ContainsKey(identifier))
                hooks[identifier]();

            Window window;
            windows.TryGetValue(identifier, out window);
            return window;
        }

        public void Show(string identifier)
        {
            var window = Get(identifier);
            if (window == null)
                throw new Exception(Resources.NoWindowWithThatIdentifierExists);
               
            window.Show(DockPanel);
        }

        public void Show(string identifier, DockState dockState)
        {
            var window = Get(identifier);
            if (window == null)
                throw new Exception(Resources.NoWindowWithThatIdentifierExists);

            window.Show(DockPanel, dockState);
        }

        public void Register(string identifier, Action action)
        {
            if (hooks.ContainsKey(identifier))
                throw new Exception("Identifier is not unique");

            hooks.Add(identifier, action);
        }
    }
}
