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
using System.IO;

using Gallio.Icarus.Core.Model;
using Gallio.Icarus.Core.Interfaces;
using Gallio.Icarus.Core.Presenter;
using Gallio.Icarus.Tests;
using Gallio.Model;
using Gallio.Model.Filters;

using MbUnit.Framework;

using Rhino.Mocks;

namespace Gallio.Icarus.Core.Model.Tests
{
    [TestFixture]
    public class TestRunnerModelTest : MockTest
    {
        //private IProjectAdapter mockAdapter;
        //private ITestRunnerModel mockModel;
        //private ProjectPresenter mockProjectPresenter;
        private TestRunnerModel testRunnerModel;

        [SetUp]
        public void SetUp()
        {
            testRunnerModel = new TestRunnerModel();
        }

        [Test, ExpectedArgumentNullException("projectPresenter")]
        public void SetProjectPresenterNull_Test()
        {
            testRunnerModel.ProjectPresenter = null;
        }

        //[Test]
        //public void TestRunnerModel_Test()
        //{
        //    // set up mocks
        //    mockAdapter = MockRepository.GenerateStub<IProjectAdapter>();
        //    mockModel = MockRepository.GenerateStub<ITestRunnerModel>();
        //    mockProjectPresenter = mocks.CreateMock<ProjectPresenter>(mockAdapter, mockModel);
        //    testRunnerModel.ProjectPresenter = mockProjectPresenter;

        //    // set up expectations
        //    mockProjectPresenter.TotalWorkUnits = 0;
        //    LastCall.IgnoreArguments();
        //    mockProjectPresenter.StatusText = "";
        //    LastCall.IgnoreArguments();
        //    mockProjectPresenter.CompletedWorkUnits = 0;
        //    LastCall.IgnoreArguments();

        //    mocks.ReplayAll();
            
        //    // these cannot be split up into seperate tests
        //    testRunnerModel.LoadTestPackage(new TestPackageConfig());
        //    testRunnerModel.BuildTestModel();
        //    testRunnerModel.ReportFolder = Path.GetTempPath();
        //    testRunnerModel.RunTests();
        //    testRunnerModel.GenerateReport();
        //    testRunnerModel.SaveReportAs(Path.GetTempFileName(), "html");
        //    testRunnerModel.StopTests();
        //}

        //[Test]
        //public void GetReportTypes_Test()
        //{
        //    IList<string> reportTypes = testRunnerModel.GetReportTypes();
        //}

        //[Test]
        //public void GetTestFrameworks_Test()
        //{
        //    IList<string> frameworks = testRunnerModel.GetTestFrameworks();
        //}
    }
}