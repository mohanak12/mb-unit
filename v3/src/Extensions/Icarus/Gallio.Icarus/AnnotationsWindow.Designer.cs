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

namespace Gallio.Icarus
{
    partial class AnnotationsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnnotationsWindow));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.annotationsListView = new System.Windows.Forms.ListView();
            this.annotationMessage = new System.Windows.Forms.ColumnHeader();
            this.annotationDetails = new System.Windows.Forms.ColumnHeader();
            this.annotationLocation = new System.Windows.Forms.ColumnHeader();
            this.annotationReference = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.showErrorsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.showWarningsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.showInfoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.annotationsListView);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(409, 248);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(409, 273);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // annotationsListView
            // 
            this.annotationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.annotationMessage,
            this.annotationDetails,
            this.annotationLocation,
            this.annotationReference});
            this.annotationsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.annotationsListView.FullRowSelect = true;
            this.annotationsListView.Location = new System.Drawing.Point(0, 0);
            this.annotationsListView.Name = "annotationsListView";
            this.annotationsListView.Size = new System.Drawing.Size(409, 248);
            this.annotationsListView.SmallImageList = this.imageList1;
            this.annotationsListView.TabIndex = 0;
            this.annotationsListView.UseCompatibleStateImageBehavior = false;
            this.annotationsListView.View = System.Windows.Forms.View.Details;
            // 
            // annotationMessage
            // 
            this.annotationMessage.Text = "Message";
            this.annotationMessage.Width = 150;
            // 
            // annotationDetails
            // 
            this.annotationDetails.Text = "Details";
            this.annotationDetails.Width = 150;
            // 
            // annotationLocation
            // 
            this.annotationLocation.Text = "Location";
            this.annotationLocation.Width = 150;
            // 
            // annotationReference
            // 
            this.annotationReference.Text = "Reference";
            this.annotationReference.Width = 150;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Error.ico");
            this.imageList1.Images.SetKeyName(1, "Warning.ico");
            this.imageList1.Images.SetKeyName(2, "Information.ico");
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showErrorsToolStripButton,
            this.showWarningsToolStripButton,
            this.showInfoToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(242, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // showErrorsToolStripButton
            // 
            this.showErrorsToolStripButton.Checked = true;
            this.showErrorsToolStripButton.CheckOnClick = true;
            this.showErrorsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showErrorsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("showErrorsToolStripButton.Image")));
            this.showErrorsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showErrorsToolStripButton.Name = "showErrorsToolStripButton";
            this.showErrorsToolStripButton.Size = new System.Drawing.Size(75, 22);
            this.showErrorsToolStripButton.Text = "{0} Errors";
            this.showErrorsToolStripButton.Click += new System.EventHandler(this.annotationsToolStripButton_Click);
            // 
            // showWarningsToolStripButton
            // 
            this.showWarningsToolStripButton.Checked = true;
            this.showWarningsToolStripButton.CheckOnClick = true;
            this.showWarningsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showWarningsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("showWarningsToolStripButton.Image")));
            this.showWarningsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showWarningsToolStripButton.Name = "showWarningsToolStripButton";
            this.showWarningsToolStripButton.Size = new System.Drawing.Size(91, 22);
            this.showWarningsToolStripButton.Text = "{0} Warnings";
            this.showWarningsToolStripButton.Click += new System.EventHandler(this.annotationsToolStripButton_Click);
            // 
            // showInfoToolStripButton
            // 
            this.showInfoToolStripButton.Checked = true;
            this.showInfoToolStripButton.CheckOnClick = true;
            this.showInfoToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showInfoToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("showInfoToolStripButton.Image")));
            this.showInfoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showInfoToolStripButton.Name = "showInfoToolStripButton";
            this.showInfoToolStripButton.Size = new System.Drawing.Size(66, 22);
            this.showInfoToolStripButton.Text = "{0} Info";
            this.showInfoToolStripButton.Click += new System.EventHandler(this.annotationsToolStripButton_Click);
            // 
            // AnnotationsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 273);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "AnnotationsWindow";
            this.TabText = "Annotations";
            this.Text = "Annotations";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView annotationsListView;
        private System.Windows.Forms.ColumnHeader annotationMessage;
        private System.Windows.Forms.ColumnHeader annotationDetails;
        private System.Windows.Forms.ColumnHeader annotationLocation;
        private System.Windows.Forms.ColumnHeader annotationReference;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton showErrorsToolStripButton;
        private System.Windows.Forms.ToolStripButton showWarningsToolStripButton;
        private System.Windows.Forms.ToolStripButton showInfoToolStripButton;

    }
}