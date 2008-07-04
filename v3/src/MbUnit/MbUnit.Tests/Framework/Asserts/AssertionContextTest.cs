﻿using System;
using System.Collections.Generic;
using Gallio.Collections;
using Gallio.Framework;
using MbUnit.Framework;

namespace MbUnit.Tests.Framework
{
    [TestsOn(typeof(AssertionContext))]
    public class AssertionContextTest
    {
        [Test]
        public void CurrentAssertionContextIsAssociatedWithTheCurrentTestContext()
        {
            AssertionContext current = AssertionContext.CurrentContext;
            Assert.AreSame(TestContext.CurrentContext, current.TestContext);
        }

        [Test]
        public void CurrentAssertionContextIsPreservedAcrossMultipleRequests()
        {
            Assert.AreSame(AssertionContext.CurrentContext, AssertionContext.CurrentContext);
        }

        [Test]
        public void DifferentTestContextsHaveDifferentAssertionContexts()
        {
            AssertionContext a = null, b = null;
            Step.RunStep("A", () => a = AssertionContext.CurrentContext);
            Step.RunStep("B", () => b = AssertionContext.CurrentContext);

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreNotSame(a, b);
        }

        [Test]
        public void InitialAssertionFailureBehaviorIsLogAndThrow()
        {
            Assert.AreEqual(AssertionFailureBehavior.LogAndThrow, AssertionContext.CurrentContext.AssertionFailureBehavior);
        }

        public class WhenAssertionFailureBehaviorIsLogAndThrow
        {
            [Test]
            public void TheAssertionIsLoggedAndCapturedButExecutionEnds()
            {
                StubAssertionFailure failure1 = new StubAssertionFailure();
                StubAssertionFailure failure2 = new StubAssertionFailure();
                bool completed = false;

                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(delegate
                {
                    AssertionContext.CurrentContext.SubmitFailure(failure1);
                    AssertionContext.CurrentContext.SubmitFailure(failure2);
                    completed = true;
                }, AssertionFailureBehavior.LogAndThrow, false);

                ArrayAssert.AreEqual(new[] { failure1 }, failures);
                Assert.IsTrue(failure1.WasLogCalled);
                Assert.IsFalse(failure2.WasLogCalled);
                Assert.IsFalse(completed);
            }

            [Test]
            public void AnExceptionMayBeReifiedAsAnAssertionFailure()
            {
                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(delegate
                {
                    throw new InvalidOperationException("Boom");
                }, AssertionFailureBehavior.LogAndThrow, true);

                Assert.AreEqual(1, failures.Length);
                Assert.AreEqual("An exception occurred.", failures[0].Description);
                Assert.AreEqual(1, failures[0].Exceptions.Count);
                Assert.Contains(failures[0].Exceptions[0], "Boom");
            }

            [Test]
            public void AnExceptionMayEscapeTheBlock()
            {
                NewAssert.Throws<InvalidOperationException>(delegate
                {
                    AssertionContext.CurrentContext.CaptureFailures(delegate
                    {
                        throw new InvalidOperationException("Boom");
                    }, AssertionFailureBehavior.LogAndThrow, false);
                });
            }
        }

        public class WhenAssertionFailureBehaviorIsLog
        {
            [Test]
            public void TheAssertionIsLoggedAndCapturedAndExecutionContinues()
            {
                StubAssertionFailure failure1 = new StubAssertionFailure();
                StubAssertionFailure failure2 = new StubAssertionFailure();
                bool completed = false;

                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(delegate
                {
                    AssertionContext.CurrentContext.SubmitFailure(failure1);
                    AssertionContext.CurrentContext.SubmitFailure(failure2);
                    completed = true;
                }, AssertionFailureBehavior.Log, false);

                ArrayAssert.AreEqual(new[] { failure1, failure2 }, failures);
                Assert.IsTrue(failure1.WasLogCalled);
                Assert.IsTrue(failure2.WasLogCalled);
                Assert.IsTrue(completed);
            }
        }

        public class WhenAssertionFailureBehaviorIsDefer
        {
            [Test]
            public void TheAssertionIsNotLoggedButIsCapturedAndExecutionContinues()
            {
                StubAssertionFailure failure1 = new StubAssertionFailure();
                StubAssertionFailure failure2 = new StubAssertionFailure();
                bool completed = false;

                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(delegate
                {
                    AssertionContext.CurrentContext.SubmitFailure(failure1);
                    AssertionContext.CurrentContext.SubmitFailure(failure2);
                    completed = true;
                }, AssertionFailureBehavior.Defer, false);

                ArrayAssert.AreEqual(new[] { failure1, failure2 }, failures);
                Assert.IsFalse(failure1.WasLogCalled);
                Assert.IsFalse(failure2.WasLogCalled);
                Assert.IsTrue(completed);
            }
        }

        public class WhenAssertionFailureBehaviorIsIgnore
        {
            [Test]
            public void NothingHappens()
            {
                StubAssertionFailure failure1 = new StubAssertionFailure();
                StubAssertionFailure failure2 = new StubAssertionFailure();
                bool completed = false;

                AssertionFailure[] failures = AssertionContext.CurrentContext.CaptureFailures(delegate
                {
                    AssertionContext.CurrentContext.SubmitFailure(failure1);
                    AssertionContext.CurrentContext.SubmitFailure(failure2);
                    completed = true;
                }, AssertionFailureBehavior.Ignore, false);

                Assert.IsEmpty(failures);
                Assert.IsFalse(failure1.WasLogCalled);
                Assert.IsFalse(failure2.WasLogCalled);
                Assert.IsTrue(completed);
            }
        }

        private sealed class StubAssertionFailure : AssertionFailure
        {
            private bool wasLogCalled;

            public StubAssertionFailure() :
                base("Description", "Message", "Stack", EmptyArray<KeyValuePair<string, string>>.Instance,
                EmptyArray<string>.Instance)
            {
            }

            public bool WasLogCalled
            {
                get { return wasLogCalled; }
            }

            public override void Log(LogWriter logWriter)
            {
                wasLogCalled = true;
                base.Log(logWriter);
            }
        }
    }
}
