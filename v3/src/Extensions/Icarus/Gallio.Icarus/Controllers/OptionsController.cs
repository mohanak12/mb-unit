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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using Gallio.Common.IO;
using Gallio.Common.Xml;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Options;
using Gallio.Model;
using Gallio.Icarus.Utilities;
using Gallio.Runtime.Logging;

namespace Gallio.Icarus.Controllers
{
    public class OptionsController : NotifyController, IOptionsController
    {
        private Settings settings;
        private MRUList recentProjects;
        private IOptionsManager optionsManager;

        private List<string> unselectedTreeViewCategoriesList;

        public bool AlwaysReloadAssemblies
        {
            get { return settings.AlwaysReloadAssemblies; }
            set { settings.AlwaysReloadAssemblies = value; }
        }

        public bool RunTestsAfterReload
        {
            get { return settings.RunTestsAfterReload; }
            set { settings.RunTestsAfterReload = value; }
        }

        public string TestStatusBarStyle
        {
            get { return settings.TestStatusBarStyle; }
            set { settings.TestStatusBarStyle = value; }
        }

        public bool ShowProgressDialogs
        {
            get { return settings.ShowProgressDialogs; }
            set { settings.ShowProgressDialogs = value; }
        }

        public bool RestorePreviousSettings
        {
            get { return settings.RestorePreviousSettings; }
            set { settings.RestorePreviousSettings = value; }
        }

        public string TestRunnerFactory
        {
            get { return settings.TestRunnerFactory; }
            set
            {
                settings.TestRunnerFactory = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TestRunnerFactory"));
            }
        }

        public BindingList<string> PluginDirectories { get; private set; }

        public IList<string> SelectedTreeViewCategories
        {
            get 
            { 
                return settings.TreeViewCategories; 
            }
            set 
            {
                settings.TreeViewCategories.Clear();
                settings.TreeViewCategories.AddRange(value);
                unselectedTreeViewCategoriesList = null;
            }
        }

        public IList<string> UnselectedTreeViewCategories 
        {
            get 
            {
                if (unselectedTreeViewCategoriesList == null)
                {
                    unselectedTreeViewCategoriesList = new List<string>();
                    foreach (var fieldInfo in typeof(MetadataKeys).GetFields())
                    {
                        if (!settings.TreeViewCategories.Contains(fieldInfo.Name))
                            unselectedTreeViewCategoriesList.Add(fieldInfo.Name);
                    }
                }
                return unselectedTreeViewCategoriesList;
            }
        }

        public Color PassedColor
        {
            get { return Color.FromArgb(settings.PassedColor); }
            set { settings.PassedColor = value.ToArgb(); }
        }

        public Color FailedColor
        {
            get { return Color.FromArgb(settings.FailedColor); }
            set { settings.FailedColor = value.ToArgb(); }
        }

        public Color InconclusiveColor
        {
            get { return Color.FromArgb(settings.InconclusiveColor); }
            set { settings.InconclusiveColor = value.ToArgb(); }
        }

        public Color SkippedColor
        {
            get { return Color.FromArgb(settings.SkippedColor); }
            set { settings.SkippedColor = value.ToArgb(); }
        }

        public double UpdateDelay
        {
            get { return 1000; }
        }

        public Size Size
        {
            get { return settings.Size; }
            set { settings.Size = value; }
        }

        public Point Location
        {
            get { return settings.Location; }
            set { settings.Location = value; }
        }

        public MRUList RecentProjects
        {
            get
            {
                if (recentProjects == null)
                {
                    recentProjects = new MRUList(settings.RecentProjects, 10);
                    recentProjects.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == "Items")
                                OnPropertyChanged(new PropertyChangedEventArgs("RecentProjects"));
                        };
                }
                return recentProjects;
            }
        }

        public LogSeverity MinLogSeverity
        {
            get { return settings.MinLogSeverity; }
            set { settings.MinLogSeverity = value; }
        }

        public bool AnnotationsShowErrors
        {
            get { return settings.AnnotationsShowErrors; }
            set { settings.AnnotationsShowErrors = value; }
        }

        public bool AnnotationsShowWarnings
        {
            get { return settings.AnnotationsShowWarnings; }
            set { settings.AnnotationsShowWarnings = value; }
        }

        public bool AnnotationsShowInfos
        {
            get { return settings.AnnotationsShowInfos; }
            set { settings.AnnotationsShowInfos = value; }
        }

        public BindingList<string> TestRunnerExtensions { get; private set; }

        public bool TestTreeSplitNamespaces
        {
            get { return settings.TestTreeSplitNamespaces; }
            set { settings.TestTreeSplitNamespaces = value; }
        }

        private void Load()
        {
            settings = OptionsManager.Settings;

            if (settings.TreeViewCategories.Count == 0)
            {
                // add default categories
                settings.TreeViewCategories.AddRange(new[] { "Namespace", MetadataKeys.AuthorName, 
                    MetadataKeys.Category, MetadataKeys.Importance, MetadataKeys.TestsOn });
            }

            // set up bindable lists (for options dialogs)
            PluginDirectories = new BindingList<string>(settings.PluginDirectories);
            TestRunnerExtensions = new BindingList<string>(settings.TestRunnerExtensions);
        }

        public void Save()
        {
            OptionsManager.Save();
        }

        public bool GenerateReportAfterTestRun
        {
            get { return settings.GenerateReportAfterTestRun; }
            set { settings.GenerateReportAfterTestRun = value; }
        }

        public IOptionsManager OptionsManager
        {
            get
            {
                if (optionsManager == null)
                {
                    SetOptionsManager(new OptionsManager(new FileSystem(), new DefaultXmlSerializer(),
                        new UnhandledExceptionPolicy()));
                }

                return optionsManager;
            }
            set
            {
                SetOptionsManager(value);
            }
        }

        private void SetOptionsManager(IOptionsManager optionsManager)
        {
            if (optionsManager == null)
                throw new ArgumentNullException("optionsManager");

            this.optionsManager = optionsManager;
            Load();
        }

        public void Cancel()
        {
            OptionsManager.Load();

            Load();
        }
    }
}
