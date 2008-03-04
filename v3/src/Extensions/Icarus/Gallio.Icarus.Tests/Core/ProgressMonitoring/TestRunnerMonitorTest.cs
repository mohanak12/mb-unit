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

using Gallio.Icarus.Core.Interfaces;
using Gallio.Icarus.Core.ProgressMonitoring;
using Gallio.Runner;
using Gallio.Runner.Monitors;

using MbUnit.Framework;

namespace Gallio.Icarus.Tests.Core.ProgressMonitoring
{
    [TestFixture]
    public class TestRunnerMonitorTest : MockTest
    {
        [Test, ExpectedArgumentNullException("presenter")]
        public void NullPresenter_Test()
        {
            TestRunnerMonitor testRunnerMonitor = new TestRunnerMonitor(null, null);
        }

        [Test, ExpectedArgumentNullException("reportMonitor")]
        public void NullReportMonitor_Test()
        {
            IProjectPresenter projectPresenter = mocks.CreateMock<IProjectPresenter>();
            mocks.ReplayAll();
            TestRunnerMonitor testRunnerMonitor = new TestRunnerMonitor(projectPresenter, null);
        }
    }
}
