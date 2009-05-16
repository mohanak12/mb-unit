﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Gallio.UI.Properties;

namespace Gallio.UI.ControlPanel.Plugins
{
    internal partial class PluginControlPanelTab : ControlPanelTab
    {
        public PluginControlPanelTab()
        {
            InitializeComponent();
        }

        public void AddPlugin(string id, string name, Version version,
            Icon icon, string description, string disabledReason)
        {
            if (icon == null)
                icon = Resources.DefaultPluginAboutIcon;

            Image smallIcon = new Icon(icon, new Size(16, 16)).ToBitmap();
            Image bigIcon = new Icon(icon, new Size(32, 32)).ToBitmap();

            var pluginInfo = new PluginInfo()
            {
                Id = id,
                Name = name,
                Version = version != null ? version.ToString() : "",
                SmallIcon = smallIcon,
                BigIcon = bigIcon,
                Description = description ?? name,
                DisabledReason = disabledReason
            };

            int rowIndex = pluginGrid.Rows.Add(pluginInfo.SmallIcon, pluginInfo.Name, pluginInfo.Version);
            DataGridViewRow row = pluginGrid.Rows[rowIndex];
            row.Tag = pluginInfo;
        }

        private void pluginGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (pluginGrid.SelectedRows.Count != 0)
            {
                var row = pluginGrid.SelectedRows[0];
                var pluginInfo = (PluginInfo)row.Tag;

                pluginIconPictureBox.Image = pluginInfo.BigIcon;
                pluginDescriptionLabel.Text = pluginInfo.Description;
            }
            else
            {
                pluginIconPictureBox.Image = null;
                pluginDescriptionLabel.Text = "";
            }
        }

        private sealed class PluginInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public Image SmallIcon { get; set; }
            public Image BigIcon { get; set; }
            public string Description { get; set; }
            public string DisabledReason { get; set; }
        }
    }
}
