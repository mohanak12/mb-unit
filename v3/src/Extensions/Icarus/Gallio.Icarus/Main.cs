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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Aga.Controls.Tree;
using Gallio.Concurrency;
using Gallio.Runtime;
using Gallio.Runtime.Logging;

using Gallio.Icarus.Controls;
using Gallio.Icarus.Core.CustomEventArgs;
using Gallio.Icarus.Interfaces;
using Gallio.Reflection;
using Gallio.Utilities;

using WeifenLuo.WinFormsUI.Docking;
using Gallio.Model.Serialization;

namespace Gallio.Icarus
{
    public partial class Main : Form, IProjectAdapterView
    {
        private TaskManager primaryTaskManager = new TaskManager();
        private TaskManager executionLogTaskManager = new TaskManager();

        private string projectFileName = String.Empty;
        private Settings settings;
        
        // dock panel windows
        private DeserializeDockContent deserializeDockContent;
        private TestExplorer testExplorer;
        private AssemblyList assemblyList;
        private TestResults testResults;
        private ReportWindow reportWindow;
        private RuntimeLogWindow runtimeLogWindow;
        private About aboutDialog;
        private PropertiesWindow propertiesWindow;
        private FiltersWindow filtersWindow;
        private ExecutionLogWindow executionLogWindow;
        private AnnotationsWindow annotationsWindow;
        
        // progress monitoring
        private ProgressMonitor progressMonitor;
        private System.Timers.Timer progressMonitorTimer;
        private bool showProgressMonitor = true;

        public bool ShowProgressMonitor
        {
            set { showProgressMonitor = value; }
        }

        public ITreeModel TreeModel
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    testExplorer.TreeModel = value;
                    ((TestTreeModel)value).TestCountChanged += delegate
                    {
                        TotalTests = ((TestTreeModel)value).TestCount;
                    };
                    ((TestTreeModel)value).TestResult += delegate(object sender, TestResultEventArgs e)
                    {
                        Sync.Invoke(this, delegate
                        {
                            testResults.UpdateTestResults(e.TestData, e.TestStepRun);
                        });
                    };
                });
            }
        }

        public ListViewItem[] Assemblies
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    assemblyList.DataBind(value);
                });
            }
        }

        public string StatusText
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    toolStripStatusLabel.Text = value;
                    progressMonitor.StatusText = value;
                });
            }
        }

        public int TotalWorkUnits
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    toolStripProgressBar.Maximum = value;
                    progressMonitor.TotalWorkUnits = value;
                    if (value == 0)
                    {
                        showProgressMonitor = true;
                        progressMonitor.Hide();
                        Cursor = Cursors.Default;
                    }
                });
            }
        }

        public int CompletedWorkUnits
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    toolStripProgressBar.Value = value;
                    progressMonitor.CompletedWorkUnits = value;
                    if (value > 0 && !progressMonitor.Visible && showProgressMonitor)
                        progressMonitorTimer.Enabled = true;
                    Cursor = Cursors.WaitCursor;
                });
            }
        }

        public int TotalTests
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    testResults.TotalTests = value;
                });
            }
        }

        public string ReportPath
        {
            set
            {
                if (value != "")
                {
                    Sync.Invoke(this, delegate
                    {
                        reportWindow.ReportPath = value;
                    });
                }
            }
        }

        public IList<string> ReportTypes
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    reportWindow.ReportTypes = value;
                });
            }
        }

        public IList<string> TestFrameworks
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    aboutDialog.TestFrameworks = value;
                });
            }
        }

        public Stream ExecutionLog
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    executionLogWindow.Log = value;
                });
            }
        }

        public CodeLocation SourceCodeLocation
        {
            set
            {
                foreach (DockPane dockPane in dockPanel.Panes)
                {
                    foreach (IDockContent dockContent in dockPane.Contents)
                    {
                        if (dockContent.ToString() == value.Path)
                        {
                            ((CodeWindow)dockContent).JumpTo(value.Line, value.Column);
                            dockContent.DockHandler.Show();
                            return;
                        }
                    }
                }
                CodeWindow codeWindow = new CodeWindow(value);
                codeWindow.Show(dockPanel, DockState.Document);
            }
        }

        public Settings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = LoadSettings();
                    if (settings == null)
                        settings = new Settings();
                }
                return settings;
            }
            set
            {
                if (settings == null)
                    throw new ArgumentNullException("value");
                settings = value;
            }
        }

        private Settings LoadSettings()
        {
            try
            {
                if (File.Exists(Paths.SettingsFile))
                    return XmlSerializationUtils.LoadFromXml<Settings>(Paths.SettingsFile);
            }
            catch
            { }
            return null;
        }

        public IList<string> HintDirectories
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    propertiesWindow.HintDirectories = value;
                });
            }
        }

        public string ApplicationBaseDirectory
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    propertiesWindow.ApplicationBaseDirectory = value;
                });
            }
        }

        public string WorkingDirectory
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    propertiesWindow.WorkingDirectory = value;
                });
            }
        }

        public bool ShadowCopy
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    propertiesWindow.ShadowCopy = value;
                });
            }
        }

        public string ProjectFileName
        {
            set
            {
                projectFileName = value;
                if (value != string.Empty)
                    Text = String.Format("{0} - Gallio Icarus", value);
                else
                    Text = "Gallio Icarus";
            }
        }

        public IList<string> TestFilters
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    filtersWindow.Filters = value;
                });
            }
        }

        public List<AnnotationData> Annotations
        {
            set
            {
                Sync.Invoke(this, delegate
                {
                    annotationsWindow.Annotations = value;
                    if (value.Count > 0)
                        annotationsWindow.Show();
                });
            }
        }

        public event EventHandler<GetTestTreeEventArgs> GetTestTree;
        public event EventHandler<SingleEventArgs<IList<string>>> AddAssemblies;
        public event EventHandler<EventArgs> RemoveAssemblies;
        public event EventHandler<SingleEventArgs<string>> RemoveAssembly;
        public event EventHandler<EventArgs> RunTests;
        public event EventHandler<EventArgs> GenerateReport;
        public event EventHandler<EventArgs> CancelOperation;
        public event EventHandler<SingleEventArgs<string>> SaveFilter;
        public event EventHandler<SingleEventArgs<string>> ApplyFilter;
        public event EventHandler<SingleEventArgs<string>> DeleteFilter;
        public event EventHandler<EventArgs> GetReportTypes;
        public event EventHandler<EventArgs> GetTestFrameworks;
        public event EventHandler<SaveReportAsEventArgs> SaveReportAs;
        public event EventHandler<SingleEventArgs<string>> SaveProject;
        public event EventHandler<OpenProjectEventArgs> OpenProject;
        public event EventHandler<EventArgs> NewProject;
        public event EventHandler<SingleEventArgs<string>> GetSourceLocation;
        public event EventHandler<SingleEventArgs<IList<string>>> UpdateHintDirectoriesEvent;
        public event EventHandler<SingleEventArgs<string>> UpdateApplicationBaseDirectoryEvent;
        public event EventHandler<SingleEventArgs<string>> UpdateWorkingDirectoryEvent;
        public event EventHandler<SingleEventArgs<bool>> UpdateShadowCopyEvent;
        public event EventHandler<EventArgs> ResetTestStatus;
        public event EventHandler<SingleEventArgs<string>> GetExecutionLog;
        public event EventHandler<EventArgs> UnloadTestPackage;
        public event EventHandler<EventArgs> CleanUp;

        public Main()
        {
            InitializeComponent();

            UnhandledExceptionPolicy.ReportUnhandledException += ReportUnhandledException;

            testExplorer = new TestExplorer(this);
            assemblyList = new AssemblyList(this);
            testResults = new TestResults();
            reportWindow = new ReportWindow(this);
            runtimeLogWindow = new RuntimeLogWindow();
            aboutDialog = new About();
            propertiesWindow = new PropertiesWindow(this);
            filtersWindow = new FiltersWindow(this);
            executionLogWindow = new ExecutionLogWindow();
            annotationsWindow = new AnnotationsWindow(this);
            progressMonitor = new ProgressMonitor();

            deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);

            progressMonitorTimer = new System.Timers.Timer();
            progressMonitorTimer.Interval = 300;
            progressMonitorTimer.AutoReset = false;
            progressMonitorTimer.Elapsed += delegate { Sync.Invoke(this, delegate { progressMonitor.Show(this); }); };
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TestExplorer).ToString())
                return testExplorer;
            else if (persistString == typeof(AssemblyList).ToString())
                return assemblyList;
            else if (persistString == typeof(TestResults).ToString())
                return testResults;
            else if (persistString == typeof(ReportWindow).ToString())
                return reportWindow;
            else if (persistString == typeof(PropertiesWindow).ToString())
                return propertiesWindow;
            else if (persistString == typeof(FiltersWindow).ToString())
                return filtersWindow;
            else if (persistString == typeof(ExecutionLogWindow).ToString())
                return executionLogWindow;
            else if (persistString == typeof(RuntimeLogWindow).ToString())
                return runtimeLogWindow;
            else if (persistString == typeof(AnnotationsWindow).ToString())
                return annotationsWindow;
            else
                return null;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            // Set the application version in the window title
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Text = String.Format(Text, appVersion.Major, appVersion.Minor);

            // try to load the dock state, if the file does not exist
            // or loading fails then use defaults.
            try
            {
                dockPanel.LoadFromXml(Paths.DockConfigFile, deserializeDockContent);
            }
            catch
            {
                DefaultDockState();
            }

            primaryTaskManager.StartTask(delegate
            {
                if (GetReportTypes != null)
                    GetReportTypes(this, EventArgs.Empty);
                if (GetTestFrameworks != null)
                    GetTestFrameworks(this, EventArgs.Empty);
                if (projectFileName != string.Empty)
                    OpenProjectFromFile(projectFileName);
                else
                    ThreadedReloadTree(true);
            });
        }

        private void DefaultDockState()
        {
            assemblyList.Show(dockPanel, DockState.DockLeftAutoHide);
            testResults.Show(dockPanel, DockState.Document);
            executionLogWindow.Show(dockPanel, DockState.Document);
            runtimeLogWindow.DockPanel = dockPanel;
            annotationsWindow.Show(dockPanel, DockState.Document);
            testExplorer.Show(dockPanel, DockState.DockLeft);
            assemblyList.Show(dockPanel, DockState.DockLeftAutoHide);
            reportWindow.DockPanel = dockPanel;
            propertiesWindow.DockPanel = dockPanel;
            filtersWindow.DockPanel = dockPanel;
        }

        private void fileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            aboutDialog.ShowDialog();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartTests();
        }

        private void StartTests()
        {
            try
            {
                // reset progress monitors
                ResetTests();

                // enable/disable buttons
                startButton.Enabled = startTestsToolStripMenuItem.Enabled = false;
                stopButton.Enabled = stopTestsToolStripMenuItem.Enabled = true;

                primaryTaskManager.StartTask(delegate
                {
                    // save test filter
                    OnSaveFilter("LastRun");
                    
                    // run tests
                    if (RunTests != null)
                        RunTests(this, new EventArgs());

                    // create report (if necessary)
                    if (!reportWindow.IsHidden)
                        ThreadedCreateReport();

                    // enable/disable buttons
                    Invoke((MethodInvoker)delegate()
                    {
                        stopButton.Enabled = stopTestsToolStripMenuItem.Enabled = false;
                        startButton.Enabled = startTestsToolStripMenuItem.Enabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("An exception occurred while starting the tests.", ex);
            }
        }

        private void reloadToolbarButton_Click(object sender, EventArgs e)
        {
            primaryTaskManager.StartTask(delegate
            {
                ThreadedReloadTree(true);
            });
        }

        private void openProject_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Gallio Projects (*.gallio)|*.gallio";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                primaryTaskManager.StartTask(delegate
                {
                    OpenProjectFromFile(openFile.FileName);
                });
            }
        }

        private void OpenProjectFromFile(string fileName)
        {
            try
            {
                if (OpenProject != null)
                    OpenProject(this, new OpenProjectEventArgs(fileName, testExplorer.TreeFilter));
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("An exception occurred while opening a project.", ex);
            }
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectFileName = string.Empty;
            SaveProjectToFile();
        }

        private void SaveProjectToFile()
        {
            if (projectFileName == String.Empty)
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.OverwritePrompt = true;
                saveFile.AddExtension = true;
                saveFile.DefaultExt = "Gallio Projects (*.gallio)|*.gallio";
                saveFile.Filter = "Gallio Projects (*.gallio)|*.gallio";
                if (saveFile.ShowDialog() == DialogResult.OK)
                    ProjectFileName = saveFile.FileName;
            }

            primaryTaskManager.StartTask(delegate
            {
                if (SaveProject != null)
                    SaveProject(this, new SingleEventArgs<string>(projectFileName));
            });
        }

        private void addAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddAssembliesToTree();
        }

        public void AddAssembliesToTree()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Assemblies or Executables (*.dll, *.exe)|*.dll;*.exe|All Files (*.*)|*.*";
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                primaryTaskManager.StartTask(delegate
                {
                    if (AddAssemblies != null)
                        AddAssemblies(this, new SingleEventArgs<IList<string>>(openFile.FileNames));
                    ThreadedReloadTree(true);
                });
            }
        }

        public void ReloadTree()
        {
            primaryTaskManager.StartTask(delegate
            {
                ThreadedReloadTree(false);
            });
        }

        private void ThreadedReloadTree(bool reloadTestModelData)
        {
            if (GetTestTree != null)
                GetTestTree(this, new GetTestTreeEventArgs(testExplorer.TreeFilter, reloadTestModelData));
        }

        private void optionsMenuItem_Click(object sender, EventArgs e)
        {
            Options options = new Options(this);
            if (options.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlSerializationUtils.SaveToXml<Settings>(settings, Paths.SettingsFile);
                }
                catch (Exception ex)
                {
                    UnhandledExceptionPolicy.Report("An exception occurred while saving the report.", ex);
                }
            }
            if (!options.IsDisposed)
                options.Dispose();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void ResetTests()
        {
            testResults.Reset();
            if (ResetTestStatus != null)
                ResetTestStatus(this, EventArgs.Empty);
        }

        private void removeAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveAssembliesFromTree();
        }

        public void RemoveAssembliesFromTree()
        {
            primaryTaskManager.StartTask(delegate
            {
                // remove assemblies
                if (RemoveAssemblies != null)
                    RemoveAssemblies(this, new EventArgs());
                ThreadedReloadTree(true);
            });
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            CancelTests();
        }

        private void CancelTests()
        {
            CancelRunningOperation();

            // reset buttons
            stopButton.Enabled = stopTestsToolStripMenuItem.Enabled = false;
            startButton.Enabled = startTestsToolStripMenuItem.Enabled = true;
        }

        public void CancelRunningOperation()
        {
            if (CancelOperation != null)
                CancelOperation(this, new EventArgs());
        }

        public void ThreadedRemoveAssembly(string assembly)
        {
            primaryTaskManager.StartTask(delegate
            {
                if (RemoveAssembly != null)
                    RemoveAssembly(this, new SingleEventArgs<string>(assembly));
                ThreadedReloadTree(true);
            });
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProjectToFile();
        }

        private void openProjectToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void CreateNewProject()
        {
            ProjectFileName = string.Empty;
            primaryTaskManager.StartTask(delegate
            {
                if (NewProject != null)
                    NewProject(this, EventArgs.Empty);
                ThreadedReloadTree(true);
            });
        }

        private void newProjectToolStripButton_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CancelTests();
        }

        private void startTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartTests();
        }

        private void showOnlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowOnlineHelp();
        }

        private void ShowOnlineHelp()
        {
            System.Diagnostics.Process.Start("http://docs.mbunit.com");
        }

        private void helpToolbarButton_Click(object sender, EventArgs e)
        {
            ShowOnlineHelp();
        }

        public void SaveReport(string fileName, string reportType)
        {
            if (SaveReportAs != null)
                SaveReportAs(this, new SaveReportAsEventArgs(fileName, reportType));
        }

        public void WriteToLog(LogSeverity severity, string message, Exception exception)
        {
            Sync.Invoke(this, delegate
            {
                Color color = Color.Black;
                switch (severity)
                {
                    case LogSeverity.Error:
                        color = Color.Red;
                        break;

                    case LogSeverity.Warning:
                        color = Color.Gold;
                        break;

                    case LogSeverity.Info:
                        color = Color.Gray;
                        break;

                    case LogSeverity.Debug:
                        color = Color.DarkGray;
                        break;
                }

                runtimeLogWindow.AppendTextLine(message, color);
                if (exception != null)
                    runtimeLogWindow.AppendTextLine(ExceptionUtils.SafeToString(exception), color);
            });
        }

        public void CreateReport()
        {
            primaryTaskManager.StartTask(delegate
            {
                ThreadedCreateReport();
            });
        }

        private void ThreadedCreateReport()
        {
            if (GenerateReport != null)
                GenerateReport(this, EventArgs.Empty);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetTests();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;

                primaryTaskManager.StartTask(delegate
                {
                    try
                    {
                        if (UnloadTestPackage != null)
                            UnloadTestPackage(this, EventArgs.Empty);

                        CleanUpOnClose();
                    }
                    finally
                    {
                        Application.Exit();
                    }
                });
            }
        }

        private void CleanUpOnClose()
        {
            // FIXME: Improve error handling
            try
            {
                // save test filter
                OnSaveFilter("AutoSave");
            }
            catch
            { }

            try
            {
                // save project
                if (SaveProject != null)
                    SaveProject(this, new SingleEventArgs<string>(string.Empty));

            }
            catch
            { }

            try
            {
                // save dock panel config
                dockPanel.SaveAsXml(Paths.DockConfigFile);
            }
            catch
            { }

            EventHandlerUtils.SafeInvoke(CleanUp, this, EventArgs.Empty);

            primaryTaskManager.AbortTask();
            executionLogTaskManager.AbortTask();

            UnhandledExceptionPolicy.ReportUnhandledException -= ReportUnhandledException;
        }

        public void ViewSourceCode(string testId)
        {
            if (GetSourceLocation != null)
                GetSourceLocation(this, new SingleEventArgs<string>(testId));
        }

        private void showWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            switch (item.Name)
            {
                case "testResultsToolStripMenuItem":
                    testResults.Show(dockPanel);
                    break;
                case "assemblyListToolStripMenuItem":
                    assemblyList.Show(dockPanel);
                    break;
                case "testExplorerToolStripMenuItem":
                    testExplorer.Show(dockPanel);
                    break;
                case "reportToolStripMenuItem":
                    CreateReport();
                    reportWindow.Show(dockPanel);
                    break;
                case "runtimeLogToolStripMenuItem":
                    runtimeLogWindow.Show(dockPanel);
                    break;
                case "propertiesToolStripMenuItem":
                    propertiesWindow.Show(dockPanel);
                    break;
                case "testFiltersToolStripMenuItem":
                    filtersWindow.Show(dockPanel);
                    break;
                case "executionLogToolStripMenuItem":
                    executionLogWindow.Show(dockPanel);
                    break;
                case "annotationsToolStripMenuItem":
                    annotationsWindow.Show(dockPanel);
                    break;
            }
        }

        public void AssemblyChanged(string filePath)
        {
            List<TaskButton> taskButtons = new List<TaskButton>();
            TaskButton yes = new TaskButton();
            yes.Text = "Yes";
            yes.Icon = global::Gallio.Icarus.Properties.Resources.tick;
            yes.Description = "Reload the test model.";
            taskButtons.Add(yes);
            TaskButton no = new TaskButton();
            no.Text = "No";
            no.Icon = global::Gallio.Icarus.Properties.Resources.cross;
            no.Description = "Don't reload.";
            taskButtons.Add(no);

            if (TaskDialog.Show("Assembly changed", filePath + " has changed, would you like to reload the test model?", 
                taskButtons) == yes)
            {
                primaryTaskManager.StartTask(delegate()
                {
                    ThreadedReloadTree(true);
                });
            }
        }

        public void UpdateHintDirectories(IList<string> hintDirectories)
        {
            if (UpdateHintDirectoriesEvent != null)
                UpdateHintDirectoriesEvent(this, new SingleEventArgs<IList<string>>(hintDirectories));
        }

        public void UpdateApplicationBaseDirectory(string applicationBaseDirectory)
        {
            if (UpdateApplicationBaseDirectoryEvent != null)
                UpdateApplicationBaseDirectoryEvent(this, new SingleEventArgs<string>(applicationBaseDirectory));
        }

        public void UpdateWorkingDirectory(string workingDirectory)
        {
            if (UpdateWorkingDirectoryEvent != null)
                UpdateWorkingDirectoryEvent(this, new SingleEventArgs<string>(workingDirectory));
        }

        public void UpdateShadowCopy(bool shadowCopy)
        {
            if (UpdateShadowCopyEvent != null)
                UpdateShadowCopyEvent(this, new SingleEventArgs<bool>(shadowCopy));
        }

        public void OnSaveFilter(string filterName)
        {
            if (SaveFilter != null)
                SaveFilter(this, new SingleEventArgs<string>(filterName));
        }

        public void OnApplyFilter(string filterName)
        {
            if (ApplyFilter != null)
                ApplyFilter(this, new SingleEventArgs<string>(filterName));
        }

        public void OnDeleteFilter(string filterName)
        {
            if (DeleteFilter != null)
                DeleteFilter(this, new SingleEventArgs<string>(filterName));
        }

        public void LoadComplete()
        {
            Sync.Invoke(this, delegate
            {
                testExplorer.ExpandAll();
                startButton.Enabled = true;
                startTestsToolStripMenuItem.Enabled = true;
            });
        }

        public void OnGetExecutionLog(string testId)
        {
            executionLogTaskManager.StartTask(delegate
            {
                if (GetExecutionLog != null)
                    GetExecutionLog(this, new SingleEventArgs<string>(testId));
            });
        }

        private void ReportUnhandledException(object sender, CorrelatedExceptionEventArgs e)
        {
            if (e.Exception is ThreadAbortException || e.IsRecursive)
                return;

            // We already print the errors to the log and most of them are harmless.
            // Ideally we should display errors more unobtrusively.  Say by flashing
            // a little icon in the status area to indicate that some new error has been
            // logged.  I really don't like the fact that Icarus is using Thread Aborts all
            // over the place.  That's the cause of most of these errors anyways.
            // Better if we introduced a real abstraction for background task management
            // and displayed progress monitor dialogs for long-running operations. -- Jeff.
            //Sync.Invoke(this, delegate
            //{
            //    MessageBox.Show(this, e.GetDescription(), e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //});
        }
    }
}
