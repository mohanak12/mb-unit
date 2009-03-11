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
using System.Threading;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Model.Messages;
using Gallio.Model.Serialization;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Tests;
using MbUnit.Framework;

namespace Gallio.AutoCAD.Tests
{
    [TestsOn(typeof(RemoteTestDriver))]
    public class RemoteTestDriverTest : BaseTestWithMocks
    {
        [Test]
        public void ShutdownIsCalledIfPingNotRecieved()
        {
            var timeout = TimeSpan.FromMilliseconds(200);
            var driver = new RemoteTestDriverTestDouble(timeout);
            Thread.Sleep(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * 1.5));
            Assert.IsTrue(driver.ShutdownCalled);
        }

        [Test]
        public void ShutdownIsNotCalledIfPingIsRecieved()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(200);
            var driver = new RemoteTestDriverTestDouble(timeout);

            int numPings = 3;

            using (driver)
            {
                for (int i = 0; i < numPings; i++)
                {
                    driver.Ping();
                    Thread.Sleep(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 2));
                    Assert.IsFalse(driver.ShutdownCalled);
                }
            }
        }

        public class RemoteTestDriverTestDouble : RemoteTestDriver
        {
            public bool ShutdownCalled;

            public RemoteTestDriverTestDouble(TimeSpan pingTimeout)
                : base(pingTimeout)
            {
            }

            public override void Shutdown()
            {
                ShutdownCalled = true;
                base.Shutdown();
            }

            protected override void ExploreImpl(TestPackageConfig testPackageConfig, TestExplorationOptions testExplorationOptions, ITestExplorationListener testExplorationListener, IProgressMonitor progressMonitor)
            {
                throw new NotImplementedException();
            }

            protected override void RunImpl(TestPackageConfig testPackageConfig, TestExplorationOptions testExplorationOptions, ITestExplorationListener testExplorationListener, TestExecutionOptions testExecutionOptions, ITestExecutionListener testExecutionListener, IProgressMonitor progressMonitor)
            {
                throw new NotImplementedException();
            }
        }
    }
}
