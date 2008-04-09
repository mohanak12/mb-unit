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
using System.Collections;
using System.IO;
using Gallio.Concurrency;
using Gallio.Runtime;
using Gallio.Framework;
using Gallio.Reflection;
using MbUnit.Framework;

namespace Gallio.PowerShellCommands.Tests
{
    /// <summary>
    /// Simple integration tests that ensures that the Cmdlet can be used from PowerShell.
    /// </summary>
    [TestFixture]
    [TestsOn(typeof(RunGallioCommand))]
    [Category("IntegrationTests")]
    public class RunGallioCommandIntegrationTest
    {
        private Hashtable state;

        [FixtureSetUp]
        public void InstallSnapIn()
        {
            Hashtable state = new Hashtable();
            new GallioSnapIn().Install(state);
        }

        [FixtureTearDown]
        public void UninstallSnapIn()
        {
            if (state != null)
            {
                new GallioSnapIn().Uninstall(state);
                state = null;
            }
        }

        [Test]
        public void CmdletPrintsCorrectOutputForPassingTestsAndReturnsAnExitCodeOfZero()
        {
            ProcessTask task = RunPowerShell("-verbose -filter Type:PassingTests -ignore-annotations");
            Assert.Contains(task.ConsoleOutput, "2 run, 2 passed, 0 failed, 0 inconclusive, 0 skipped");
            Assert.AreEqual(task.ExitCode, 0, "Exit code for passing tests should be zero.");
        }

        [Test]
        public void CmdletPrintsCorrectOutputForPassingAndFailingTestsAndReturnsAnExitCodeOfOne()
        {
            ProcessTask task = RunPowerShell("-verbose -filter Type:SimpleTest -ignore-annotations");
            Assert.Contains(task.ConsoleOutput, "2 run, 1 passed, 1 failed, 0 inconclusive, 0 skipped");
            Assert.AreEqual(task.ExitCode, 1, "Exit code for failing tests should be one.");
        }

        [Test]
        public void CmdletDoesNotCausePowerShellToTerminateAbruptlyOnUnhandledExceptions()
        {
            ProcessTask task = RunPowerShell("-verbose -filter Type:UnhandledExceptionTest -ignore-annotations");
            Assert.Contains(task.ConsoleOutput, "Unhandled!");
            Assert.Contains(task.ConsoleOutput, "2 run, 2 passed, 0 failed, 0 inconclusive, 0 skipped");
            Assert.IsFalse(task.ConsoleOutput.Contains("An error has occurred that was not properly handled. Additional information is shown below. The Windows PowerShell process will exit."),
                "Should not print a message about the unhandled exception.");
            Assert.AreEqual(task.ExitCode, 1, "Exit code should be zero because an error message was written.  This is despite all of the tests having passed and a result code of zero having been returned.  PowerShell is somewhat unusual in this regard.");
        }

        private ProcessTask RunPowerShell(string options)
        {
            string executablePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                @"windowspowershell\v1.0\powershell.exe");

            string workingDirectory = Path.GetDirectoryName((AssemblyUtils.GetAssemblyLocalPath(GetType().Assembly)));

            ProcessTask task = Tasks.StartProcessTask(executablePath,
               "\"& Add-PSSnapIn Gallio; Run-Gallio 'MbUnit.TestResources.dll' -pd '" +
               RuntimeAccessor.InstallationPath + "' " + options + "\"",
               workingDirectory);

            Assert.IsTrue(task.Run(TimeSpan.FromSeconds(60)), "A timeout occurred.");
            return task;
        }
    }
}
