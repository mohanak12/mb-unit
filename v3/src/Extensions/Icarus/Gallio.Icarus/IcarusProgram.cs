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
using System.IO;
using System.Windows.Forms;
using Gallio.Icarus.Controllers;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Mediator.Interfaces;
using Gallio.Icarus.Models;
using Gallio.Icarus.Models.Interfaces;
using Gallio.Icarus.Properties;
using Gallio.Icarus.Services.Interfaces;
using Gallio.Reflection;
using Gallio.Runner;
using Gallio.Runner.Projects;
using Gallio.Runner.Reports;
using Gallio.Runtime;
using Gallio.Runtime.ConsoleSupport;
using Gallio.Icarus.Services;
using Gallio.Icarus.Utilities;

namespace Gallio.Icarus
{
    /// <summary>
    /// The Icarus program.
    /// </summary>
    public class IcarusProgram : ConsoleProgram<IcarusArguments>
    {
        /// <summary>
        /// Creates an instance of the program.
        /// </summary>
        public IcarusProgram()
        {
            ApplicationName = Resources.ApplicationName;
        }

        /// <inheritdoc />
        protected override int RunImpl(string[] args)
        {
            if (!ParseArguments(args))
            {
                ShowHelp();
                return 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RuntimeLogController runtimeLogController = new RuntimeLogController();

            RuntimeSetup runtimeSetup = new RuntimeSetup
            {
                RuntimePath =
                    Path.GetDirectoryName(
                    AssemblyUtils.GetFriendlyAssemblyLocation(typeof (IcarusProgram).Assembly))
            };

            OptionsController optionsController = new OptionsController(new FileSystem(), new XmlSerialization(),
                new Utilities.UnhandledExceptionPolicy());

            // Set the installation path explicitly to ensure that we do not encounter problems
            // when the test assembly contains a local copy of the primary runtime assemblies
            // which will confuse the runtime into searching in the wrong place for plugins.
            runtimeSetup.PluginDirectories.AddRange(optionsController.PluginDirectories);
            
            using (RuntimeBootstrap.Initialize(runtimeSetup, runtimeLogController))
            {
                IRuntime runtime = RuntimeAccessor.Instance;

                optionsController.SetTestRunnerManager(runtime.Resolve<ITestRunnerManager>());

                ITestRunner testRunner = runtime.Resolve<ITestRunnerManager>().CreateTestRunner(
                    optionsController.TestRunnerFactory);
                ITestRunnerService testRunnerService = new TestRunnerService(testRunner);
                
                IReportManager reportManager = RuntimeAccessor.Instance.Resolve<IReportManager>();

                IMediator mediator = Mediator.Mediator.Instance;
                mediator.ProjectController = new ProjectController(new ProjectTreeModel(Paths.DefaultProject, 
                    new Project()), new FileSystem(), new XmlSerialization());
                mediator.TestController = new TestController(testRunnerService, new TestTreeModel());
                mediator.ReportController = new ReportController(new ReportService(reportManager));
                mediator.ExecutionLogController = new ExecutionLogController(mediator.TestController);
                mediator.AnnotationsController = new AnnotationsController(mediator.TestController);
                mediator.RuntimeLogController = runtimeLogController;
                mediator.OptionsController = optionsController;

                Main main = new Main(mediator);
                main.Load += delegate
                {
                    List<string> assemblyFiles = new List<string>();
                    if (Arguments != null && Arguments.Assemblies.Length > 0)
                    {
                        foreach (string assembly in assemblyFiles)
                        {
                            if (!File.Exists(assembly))
                                continue;

                            if (Path.GetExtension(assembly) == ".gallio")
                            {
                                mediator.OpenProject(assembly);
                                break;
                            }
                            mediator.AddAssemblies(assemblyFiles);
                        }
                    }
                    else if (optionsController.RestorePreviousSettings && File.Exists(Paths.DefaultProject))
                        mediator.OpenProject(Paths.DefaultProject);
                };

                testRunnerService.Initialize();
                main.CleanUp += delegate { testRunnerService.Dispose(); };

                Application.Run(main);
            }

            return ResultCode.Success;
        }

        protected override void ShowHelp()
        {
            ArgumentParser.ShowUsageInMessageBox(ApplicationTitle);
        }

        [STAThread]
        //[LoaderOptimization(LoaderOptimization.MultiDomain)] // Disabled due to bug: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=95157
        internal static int Main(string[] args)
        {
            return new IcarusProgram().Run(NativeConsole.Instance, args);
        }
    }
}
