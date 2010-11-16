﻿// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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
// WITHOUT WC:\Projects\Gallio\v3\src\Extensions\MbUnitCpp\Gallio.MbUnitCppAdapter.Tests\Model\Bridge\UnmanagedTestRepositoryTest.csARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.MbUnitCppAdapter.Model.Bridge;
using Gallio.Model;
using Gallio.Common.Reflection;
using Gallio.Tests;
using MbUnit.Framework;
using System.Reflection;
using System.IO;
using Gallio.Runner.Reports.Schema;
using Gallio.Framework;
using Gallio.Common.Markup;

namespace Gallio.MbUnitCppAdapter.Tests.Integration
{
    [TestFixture]
    public class RunTest : BaseTestWithSampleRunner
    {
        protected override void ConfigureRunner()
        {
            Runner.AddFile(new FileInfo(Helper.GetTestResources()));
        }

        [Test]
        public void ListAllTests()
        {
            IEnumerable<TestStepRun> runs = Runner.Report.TestPackageRun.AllTestStepRuns;
            string[] names = runs.Select(x => x.Step.FullName).Where(x => x.Length > 0).ToArray();
            Assert.IsNotEmpty(names);

            foreach (string name in names)
                TestLog.WriteLine("> " + name);
        }

        [Test, XmlData("//Test", ResourcePath = "Specifications.xml")]
        public void Test(
            [Bind("@name")] string fullName,
            [Bind("@expectedStatus")] TestStatus expectedStatus,
            [Bind("@expectedAssertCount")] int expectedAssertCount,
            [Bind("@expectedFailureLog")] string expectedFailureLog,
            [Bind("@expectedDefaultLog")] string expectedDefaultLog)
        {
            IEnumerable<TestStepRun> runs = Runner.GetTestStepRuns(r => r.Step.FullName == fullName);
            Assert.IsNotEmpty(runs, "Test step not found.");
            TestStepRun run = runs.First();
            Assert.IsNotNull(run);
            Assert.AreEqual(expectedStatus, run.Result.Outcome.Status);
            Assert.AreEqual(expectedAssertCount, run.Result.AssertCount);

            if (expectedFailureLog != null)
                AssertLogContains(run, expectedFailureLog, MarkupStreamNames.Failures);

            if (expectedDefaultLog != null)
                AssertLogContains(run, expectedDefaultLog.Replace("\\n", "\n"), MarkupStreamNames.Default);
        }
    }
}
