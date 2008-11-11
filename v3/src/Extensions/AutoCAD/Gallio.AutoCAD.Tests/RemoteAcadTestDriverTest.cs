﻿using System;
using System.Threading;
using Gallio.Framework;
using Gallio.Model.Logging;
using Gallio.Runner.Drivers;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using Gallio.Tests;
using MbUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Gallio.AutoCAD.Tests
{
    [TestsOn(typeof(RemoteAcadTestDriver))]
    public class RemoteAcadTestDriverTest : BaseTestWithMocks
    {
        [Test, ExpectedArgumentNullException]
        public void NullTestDriverArgumentThrowsArgumentNullException()
        {
            new RemoteAcadTestDriver(null, null);
        }

        [Test]
        public void DisposeDisposesActualTestDriver()
        {
            var driver = Mocks.StrictMock<ITestDriver>();
            using (Mocks.Record())
            {
                driver.Dispose();
            }

            using (Mocks.Playback())
            {
                using (RemoteAcadTestDriver testDriver = new RemoteAcadTestDriver(null, driver))
                {
                }
            }
        }

        [Test]
        public void DisposeCallsShutdown()
        {
            var driver = Mocks.StrictMock<RemoteAcadTestDriver>(null, Mocks.Stub<ITestDriver>());
            using (Mocks.Record())
            {
                Expect.Call(driver.Dispose).CallOriginalMethod(OriginalCallOptions.CreateExpectation);
                Expect.Call(driver.Shutdown).CallOriginalMethod(OriginalCallOptions.CreateExpectation);
            }

            using (Mocks.Playback())
            {
                driver.Dispose();
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CallingITestDriverMethodBeforeRunThrowsInvalidOperationException()
        {
            var driver = new RemoteAcadTestDriver(null, Mocks.Stub<ITestDriver>());
            driver.Initialize(RuntimeAccessor.Instance.GetRuntimeSetup(), new TestLogStreamLogger(TestLog.Default));
        }

        [Test]
        public void WaitForShutdownBlocksUntilShutdown()
        {
            using (RemoteAcadTestDriver driver = new RemoteAcadTestDriver(null, Mocks.Stub<ITestDriver>()))
            {
                var runThread = new Thread(driver.WaitForShutdown);
                runThread.Start();
                Thread.Sleep(100); // Give it some time to start up.

                Assert.AreEqual(runThread.ThreadState, ThreadState.WaitSleepJoin);

                driver.Shutdown();
                Assert.IsTrue(runThread.Join(TimeSpan.FromSeconds(2)));
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CallingWaitForShutdownTwiceThrowsInvalidOperationException()
        {
            using (RemoteAcadTestDriver driver = new RemoteAcadTestDriver(null, Mocks.Stub<ITestDriver>()))
            {
                var runThread = new Thread(driver.WaitForShutdown);
                runThread.Start();
                Thread.Sleep(100); // Give it some time to start up.

                Assert.AreEqual(runThread.ThreadState, ThreadState.WaitSleepJoin);

                driver.WaitForShutdown();
            }
        }

        [Test]
        public void CallingITestDriverMethodsExecuteOnCorrectThread()
        {
            var runtimeSetup = RuntimeAccessor.Instance.GetRuntimeSetup();
            var logger = new TestLogStreamLogger(TestLog.Default);
            var stub = Mocks.Stub<ITestDriver>();
            var driver = new RemoteAcadTestDriver(null, stub);
            var runThread = new Thread(driver.WaitForShutdown);

            using (Mocks.Record())
            {
                Expect.Call(() => stub.Initialize(runtimeSetup, logger))
                      .Do(new Action<RuntimeSetup, ILogger>((x, y) => Assert.AreEqual(Thread.CurrentThread, runThread)));
            }

            using (Mocks.Playback())
            {
                using (driver)
                {
                    runThread.Start();
                    Thread.Sleep(100); // Give it some time to start up.

                    driver.Initialize(runtimeSetup, logger);
                    driver.Shutdown();
                }
            }
        }
    }
}
