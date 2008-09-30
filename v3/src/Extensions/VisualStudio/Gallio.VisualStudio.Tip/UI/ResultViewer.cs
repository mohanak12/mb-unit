﻿// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gallio.VisualStudio.Shell.ToolWindows;

namespace Gallio.VisualStudio.Tip.UI
{
    /// <summary>
    /// User control which displays detailed about a Gallio test result.
    /// </summary>
    public partial class ResultViewer : UserControl
    {
        /// <summary>
        /// Default constructor.
        /// For the designer only.
        /// </summary>
        public ResultViewer()
        {
            InitializeComponent();
        }

        private GallioTestResult testResult;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testResult">The Gallio test result.</param>
        public ResultViewer(GallioTestResult testResult)
        {
            if (testResult == null)
            {
                throw new ArgumentNullException("testResult");
            }

            InitializeComponent();
            this.testResult = testResult;
        }

        private void ResultViewer_Load(object sender, EventArgs e)
        {
            InitializeContent();
        }

        private void InitializeContent()
        {
            AddRow("Test Name", testResult.TestName);
            AddRow("Description", testResult.TestDescription);
        }

        private void AddRow(string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                dataGridView.Rows.Add(name, value);
            }
        }
    }
}
