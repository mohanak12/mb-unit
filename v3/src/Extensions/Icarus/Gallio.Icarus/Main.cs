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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Aga.Controls.Tree;
using Gallio.Concurrency;
using Gallio.Runtime.Logging;

using Gallio.Icarus.Controls;
using Gallio.Icarus.Core.CustomEventArgs;
using Gallio.Icarus.Interfaces;
using Gallio.Model.Execution;
using Gallio.Reflection;
using Gallio.Utilities;

using WeifenLuo.WinFormsUI.Docking;

namespace Gallio.Icarus
{
    public partial class Main : Form, IProjectAdapterView
    {
        private Task workerTask = null;
        private Thread executionLogThread = null;
        
        private string projectFileName = String.Empty;
        private Settings settings;
        private string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "Gallio/Icarus/Icarus.settings");
        
        // dock panel windows
        private DeserializeDockContent deserializeDockContent;
        private string dockConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Gallio/Icarus/DockPanel.config");
        private TestExplorer testExplorer;
        private AssemblyList assemblyList;
        private TestResults testResults;
        private ReportWindow reportWindow;
        private LogWindow runtimeWindow;
        //private PerformanceMonitor performanceMonitor;
        private About aboutDialog;
        private PropertiesWindow propertiesWindow;
        private FiltersWindow filtersWindow;
        private ExecutionLogWindow executionLogWindow;
        
        public ITreeModel TreeModel
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                        {
                            TreeModel = value;
                        }));
                }
                else
                {
                    testExplorer.TreeModel = value;
                    ((TestTreeModel)((SortedTreeModel)value).InnerModel).TestCountChanged += delegate
                    {
                        TotalTests = ((TestTreeModel)((SortedTreeModel)value).InnerModel).TestCount;
                    };
                    ((TestTreeModel)((SortedTreeModel)value).InnerModel).TestResult += delegate(object sender, TestResultEventArgs e)
                    {
                        testResults.UpdateTestResults(e.TestName, e.TestOutcome, e.Duration, e.TypeName, e.NamespaceName, e.AssemblyName);
                        //performanceMonitor.UpdateTestResults(e.TestOutcome, e.TypeName, e.NamespaceName, e.AssemblyName);
                    };
                }
            }
        }

        public ListViewItem[] Assemblies
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        Assemblies = value;
                    }));
                }
                else
                {
                    assemblyList.DataBind(value);
                }
            }
        }

        public string StatusText
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        StatusText = value;
                    });
                }
                else
                    toolStripStatusLabel.Text = value;
            }
        }

        public int TotalWorkUnits
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        TotalWorkUnits = value;
                    });
                }
                else
                    toolStripProgressBar.Maximum = value;
            }
        }

        public int CompletedWorkUnits
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        CompletedWorkUnits = value;
                    });
                }
                else
                    toolStripProgressBar.Value = value;
            }
        }

        public int TotalTests
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        TotalTests = value;
                    });
                }
                else
                {
                    testResults.TotalTests = value;
                }
            }
        }

        public string ReportPath
        {
            set
            {
                if (value != "")
                {
                    if (InvokeRequired)
                    {
                        Invoke(new MethodInvoker(delegate()
                        {
                            ReportPath = value;
                        }));
                    }
                    else
                    {
                        reportWindow.ReportPath = value;
                    }
                }
            }
        }

        public IList<string> ReportTypes
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        ReportTypes = value;
                    }));
                }
                else
                {
                    reportWindow.ReportTypes = value;
                }
            }
        }

        public IList<string> TestFrameworks
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        TestFrameworks = value;
                    }));
                }
                else
                {
                    aboutDialog.TestFrameworks = value;
                }
            }
        }

        public void NotifyException(Exception value)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate() { NotifyException(value); }));
            }
            else
            {
                if (!(value is ThreadAbortException))
                    MessageBox.Show(String.Format("Message: {0}\nStack trace: {1}", value.Message, value.StackTrace), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string ExecutionLog
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        ExecutionLog = value;
                    }));
                }
                else
                    executionLogWindow.Log = value;
            }
        }

        public CodeLocation SourceCodeLocation
        {
            set
            {
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
                if (File.Exists(settingsFile))
                    return XmlSerializationUtils.LoadFromXml<Settings>(settingsFile);
            }
            catch
            { }
            return null;
        }

        public IList<string> HintDirectories
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                        {
                            HintDirectories = value;
                        }));
                }
                else
                    propertiesWindow.HintDirectories = value;
            }
        }

        public string ApplicationBaseDirectory
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        ApplicationBaseDirectory = value;
                    }));
                }
                else
                    propertiesWindow.ApplicationBaseDirectory = value;
            }
        }

        public string WorkingDirectory
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        WorkingDirectory = value;
                    }));
                }
                else
                    propertiesWindow.WorkingDirectory = value;
            }
        }

        public bool ShadowCopy
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        ShadowCopy = value;
                    }));
                }
                else
                    propertiesWindow.ShadowCopy = value;
            }
        }

        public string ProjectFileName
        {
            set { projectFileName = value; }
        }

        public IList<string> TestFilters
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate()
                    {
                        TestFilters = value;
                    }));
                }
                else
                    filtersWindow.Filters = value;
            }
        }

        public event EventHandler<GetTestTreeEventArgs> GetTestTree;
        public event EventHandler<SingleEventArgs<IList<string>>> AddAssemblies;
        public event EventHandler<EventArgs> RemoveAssemblies;
        public event EventHandler<SingleEventArgs<string>> RemoveAssembly;
        public event EventHandler<EventArgs> RunTests;
        public event EventHandler<EventArgs> GenerateReport;
        public event EventHandler<EventArgs> StopTests;
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

        public Main()
        {
            InitializeComponent();

            testExplorer = new TestExplorer(this);
            assemblyList = new AssemblyList(this);
            testResults = new TestResults();
            reportWindow = new ReportWindow(this);
            runtimeWindow = new LogWindow("Runtime");
            //performanceMonitor = new PerformanceMonitor();
            aboutDialog = new About();
            propertiesWindow = new PropertiesWindow(this);
            filtersWindow = new FiltersWindow(this);
            executionLogWindow = new ExecutionLogWindow();

            deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
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
            //else if (persistString == typeof(PerformanceMonitor).ToString())
            //    return performanceMonitor;
            else if (persistString == typeof(PropertiesWindow).ToString())
                return propertiesWindow;
            else if (persistString == typeof(FiltersWindow).ToString())
                return filtersWindow;
            else if (persistString == typeof(ExecutionLogWindow).ToString())
                return executionLogWindow;
            else
            {
                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length != 2)
                    return null;
                if (parsedStrings[0] != typeof(LogWindow).ToString())
                    return null;
                switch (parsedStrings[1])
                {
                    case "Runtime":
                        return runtimeWindow;
                    default:
                        return null;
                }
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            // Set the application version in the window title
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Text = String.Format(Text, appVersion.Major, appVersion.Minor);

            if (File.Exists(dockConfigFile))
            {
                try
                {
                    dockPanel.LoadFromXml(dockConfigFile, deserializeDockContent);
                }
                catch
                { }
            }
            else
            {
                assemblyList.Show(dockPanel, DockState.DockLeftAutoHide);
                //performanceMonitor.Show(dockPanel);
                testResults.Show(dockPanel);
                executionLogWindow.Show(dockPanel);
                runtimeWindow.Show(dockPanel, DockState.DockBottomAutoHide);
                testExplorer.Show(dockPanel, DockState.DockLeft);
            }

            StartWorkerTask(delegate
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
                StatusText = "Running tests";

                // reset progress monitors
                Reset();

                // enable/disable buttons
                startButton.Enabled = startTestsToolStripMenuItem.Enabled = false;
                stopButton.Enabled = stopTestsToolStripMenuItem.Enabled = true;

                StartWorkerTask(delegate
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
                NotifyException(ex);
            }
        }

        //private void ShowTaskDialog()
        //{
        //    //trayIcon.Icon = Resources.FailMb;
        //    //trayIcon.ShowBalloonTip(5, "Gallio Test Notice", "Recent changes have caused 5 of your unit tests to fail.",
        //    //                        ToolTipIcon.Error);
        //    List<TaskButton> taskButtons = new List<TaskButton>();

        //    TaskButton button1 = new TaskButton();
        //    button1.Text = "Button 1";
        //    button1.Icon = Resources.tick;
        //    button1.Description = "This is the first button, it should explain what the option does.";
        //    taskButtons.Add(button1);

        //    TaskButton button2 = new TaskButton();
        //    button2.Text = "Button 2";
        //    button2.Icon = Resources.help_browser;
        //    button2.Description =
        //        "This is the second button, much the same as the first button but this one demonstrates that the text will wrap onto the next line.";
        //    taskButtons.Add(button2);

        //    TaskButton button3 = new TaskButton();
        //    button3.Text = "Close Window";
        //    button3.Icon = Resources.cross;
        //    button3.Description = "Saves all changes and exits the application.";
        //    taskButtons.Add(button3);

        //    TaskButton res = TaskDialog.Show("Title Text",
        //                                     "Description about the problem and what you need to do to resolve it. Each button can have its own description too.",
        //                                     taskButtons);
        //    if (res == button2)
        //        MessageBox.Show("Button 2 was clicked.");
        //    else if (res == button1)
        //        MessageBox.Show("Button 1 was clicked.");
        //}

        private void reloadToolbarButton_Click(object sender, EventArgs e)
        {
            StatusText = "Reloading...";
            StartWorkerTask(delegate
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
                StartWorkerTask(delegate
                {
                    OpenProjectFromFile(openFile.FileName);
                });
            }
        }

        private void OpenProjectFromFile(string fileName)
        {
            StatusText = "Opening project";
            try
            {
                if (OpenProject != null)
                    OpenProject(this, new OpenProjectEventArgs(fileName, testExplorer.TreeFilter));
            }
            catch (Exception ex)
            {
                NotifyException(ex);
            }
            StatusText = string.Empty;
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
                {
                    projectFileName = saveFile.FileName;
                }
            }

            StatusText = "Saving project";
            StartWorkerTask(delegate
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
                StatusText = "Adding assemblies";
                StartWorkerTask(delegate
                {
                    if (AddAssemblies != null)
                        AddAssemblies(this, new SingleEventArgs<IList<string>>(openFile.FileNames));
                    ThreadedReloadTree(true);
                });
            }
        }

        public void ReloadTree()
        {
            StartWorkerTask(delegate
            {
                ThreadedReloadTree(false);
            });
        }

        private void ThreadedReloadTree(bool reloadTestModelData)
        {
            StatusText = "Reloading tree";
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
                    XmlSerializationUtils.SaveToXml<Settings>(settings, settingsFile);
                }
                catch (Exception ex)
                {
                    NotifyException(ex);
                }
            }
            if (!options.IsDisposed)
                options.Dispose();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void Reset()
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
            StatusText = "Removing assemblies";
            StartWorkerTask(delegate
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
            StatusText = "Stopping tests";
            StartWorkerTask(delegate
            {
                if (StopTests != null)
                    StopTests(this, new EventArgs());

                // enable/disable buttons
                toolStripContainer.Invoke((MethodInvoker)delegate()
                {
                    stopButton.Enabled = stopTestsToolStripMenuItem.Enabled = false;
                    startButton.Enabled = startTestsToolStripMenuItem.Enabled = true;
                });
            });
        }

        public void ThreadedRemoveAssembly(string assembly)
        {
            StatusText = "Removing assembly";
            StartWorkerTask(delegate
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
            StatusText = "Creating new project";
            StartWorkerTask(delegate
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
            Color color = Color.Black;
            switch (severity)
            {
                case LogSeverity.Error:
                    color = Color.Red;
                    break;

                case LogSeverity.Warning:
                    color = Color.Yellow;
                    break;

                case LogSeverity.Info:
                    color = Color.Gray;
                    break;

                case LogSeverity.Debug:
                    color = Color.DarkGray;
                    break;
            }
            runtimeWindow.AppendText(message, color);
            runtimeWindow.AppendText(ExceptionUtils.SafeToString(exception), color);
        }

        public void CreateReport()
        {
            StartWorkerTask(delegate
            {
                ThreadedCreateReport();
            });
        }

        private void ThreadedCreateReport()
        {
            StatusText = "Generating report";
            if (GenerateReport != null)
                GenerateReport(this, EventArgs.Empty);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;

                StartWorkerTask(delegate
                {
                    try
                    {
                        if (UnloadTestPackage != null)
                            UnloadTestPackage(this, EventArgs.Empty);

                        SaveStateOnClose();
                    }
                    finally
                    {
                        Application.Exit();
                    }
                });
            }
        }

        private void SaveStateOnClose()
        {
            try
            {
                // create folder (if necessary)
                string gallioDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallio/Icarus");
                if (!Directory.Exists(gallioDir))
                    Directory.CreateDirectory(gallioDir);

                // save test filter
                OnSaveFilter("AutoSave");

                // save project
                if (SaveProject != null)
                    SaveProject(this, new SingleEventArgs<string>(Path.Combine(gallioDir, "Icarus.gallio")));

                // save dock panel config
                dockPanel.SaveAsXml(dockConfigFile);
            }
            catch
            {
                // FIXME.  Must be able to improve this!
            }
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
                //case "performanceMonitorToolStripMenuItem":
                //    performanceMonitor.Show(dockPanel);
                //    break;
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
                case "runtimeToolStripMenuItem":
                    runtimeWindow.Show(dockPanel);
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
            }
        }

        public void AssemblyChanged(string filePath)
        {
            MessageBox.Show(filePath + " has changed!");
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
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { LoadComplete(); });
            }
            else
            {
                testExplorer.ExpandAll();
                startButton.Enabled = true;
                startTestsToolStripMenuItem.Enabled = true;
            }
        }

        public void OnGetExecutionLog(string testId)
        {
            if (executionLogThread != null)
            {
                executionLogThread.Abort();
                executionLogThread = null;
            }
            executionLogThread = new Thread(delegate()
                {
                    try
                    {
                        if (GetExecutionLog != null)
                            GetExecutionLog(this, new SingleEventArgs<string>(testId));
                    }
                    catch (Exception ex)
                    {
                        NotifyException(ex);
                    }
                });
            executionLogThread.Start();
        }

        private void StartWorkerTask(Action action)
        {
            AbortWorkerTask();

            workerTask = new ThreadTask("Icarus Worker", action);

            workerTask.Terminated += delegate
            {
                if (!workerTask.IsAborted)
                {
                    StatusText = "";

                    if (workerTask.Result.Exception != null)
                        NotifyException(workerTask.Result.Exception);
                }
            };

            workerTask.Start();
        }

        private void AbortWorkerTask()
        {
            Task cachedWorkerTask = Interlocked.Exchange(ref workerTask, null);
            if (cachedWorkerTask != null)
            {
                StatusText = "Aborting worker thread";
                cachedWorkerTask.Abort();
                cachedWorkerTask.Join(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
