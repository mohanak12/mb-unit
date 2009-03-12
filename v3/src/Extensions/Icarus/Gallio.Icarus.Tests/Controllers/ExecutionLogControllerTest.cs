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
using System.Threading;
using Gallio.Concurrency;
using Gallio.Icarus.Controllers;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Models;
using Gallio.Icarus.Models.Interfaces;
using Gallio.Model;
using Gallio.Model.Logging;
using Gallio.Model.Serialization;
using Gallio.Runner.Events;
using Gallio.Runner.Reports;
using MbUnit.Framework;
using Rhino.Mocks;

namespace Gallio.Icarus.Tests.Controllers
{
    [MbUnit.Framework.Category("Controllers"), Author("Graham Hay")]
    class ExecutionLogControllerTest
    {
        [Test]
        public void TestStepFinished_Test()
        {
            var testStepRun = new TestStepRun(new TestStepData("rootStep", "name", "fullName", "root"))
                                  {TestLog = new StructuredTestLog()};
            testStepRun.TestLog.Attachments.Add(new TextAttachment("name", "contentType", "text").ToAttachmentData());
            var e = new TestStepFinishedEventArgs(new Report(), new TestData("root", "name", "fullName"), testStepRun);
            
            var testController = MockRepository.GenerateStub<ITestController>();
            testController.Stub(x => x.SelectedTests).Return(new BindingList<TestTreeNode>(new List<TestTreeNode>()));
            var report = new Report
                             {
                                 TestPackageRun = new TestPackageRun(),
                                 TestModel = new TestModelData()
                             };
            report.TestPackageRun.RootTestStepRun = testStepRun;

            testController.Stub(x => x.ReadReport(null)).IgnoreArguments().Do((Action<ReadAction<Report>>)(action => action(report)));
            var testTreeModel = MockRepository.GenerateStub<ITestTreeModel>();
            testTreeModel.Stub(x => x.Root).Return(new TestTreeNode("root", "name", "nodeType"));
            testController.Stub(x => x.Model).Return(testTreeModel);
            var optionsController = MockRepository.GenerateStub<IOptionsController>();
            optionsController.Stub(x => x.UpdateDelay).Return(1);

            var executionLogController = new ExecutionLogController(testController, optionsController);
            var flag = false;
            executionLogController.ExecutionLogUpdated += delegate { flag = true; };
            testController.Raise(x => x.TestStepFinished += null, testController, e);
            Thread.Sleep(200);
            Assert.IsTrue(flag);
        }
    }
}