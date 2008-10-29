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
using System.Threading;
using Gallio.Concurrency;
using Gallio.Framework.Data;
using Gallio.Framework.Conversions;
using Gallio.Framework.Formatting;
using Gallio.Framework;
using Gallio.Model;
using Gallio.Model.Diagnostics;
using Gallio.Model.Execution;
using Gallio.Model.Logging;
using Gallio.Reflection;
using Gallio.Runtime.ProgressMonitoring;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// Encapsulates the algorithm for recursively running a <see cref="PatternTest" />.
    /// </summary>
    internal class PatternTestExecutor
    {
        public delegate TestOutcome PatternTestHandlerDecorator(Sandbox sandbox, ref IPatternTestHandler handler);

        private readonly TestExecutionOptions options;
        private readonly IProgressMonitor progressMonitor;
        private readonly IFormatter formatter;
        private readonly IConverter converter;

        public PatternTestExecutor(TestExecutionOptions options, IProgressMonitor progressMonitor,
            IFormatter formatter, IConverter converter)
        {
            this.options = options;
            this.progressMonitor = progressMonitor;
            this.formatter = formatter;
            this.converter = converter;
        }

        public TestOutcome RunTest(ITestCommand testCommand, ITestStep parentTestStep,
            Sandbox parentSandbox, PatternTestHandlerDecorator testHandlerDecorator)
        {
            if (progressMonitor.IsCanceled)
                return TestOutcome.Canceled;

            if (!testCommand.AreDependenciesSatisfied())
            {
                ITestContext context = testCommand.StartPrimaryChildStep(parentTestStep);
                context.LogWriter.Warnings.WriteLine("Skipped due to an unsatisfied test dependency.");
                context.FinishStep(TestOutcome.Skipped, null);
                return TestOutcome.Skipped;
            }

            progressMonitor.SetStatus(testCommand.Test.Name);

            PatternTest test = testCommand.Test as PatternTest;
            if (test != null)
            {
                try
                {
                    using (Sandbox sandbox = parentSandbox.CreateChild())
                    {
                        return RunTestBody(testCommand, parentTestStep, sandbox, testHandlerDecorator, test);
                    }
                }
                catch (Exception ex)
                {
                    return ReportTestError(testCommand, parentTestStep, ex, String.Format("An exception occurred while preparing to run test '{0}'.", test.FullName));
                }
                finally
                {
                    progressMonitor.SetStatus("");
                    progressMonitor.Worked(1);
                }
            }
            else
            {
                ITestController controller = testCommand.Test.TestControllerFactory();
                using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(1))
                    controller.RunTests(testCommand, parentTestStep, options, subProgressMonitor);
                return testCommand.RootStepFailureCount == 0 ? TestOutcome.Passed : TestOutcome.Failed;
            }
        }

        private TestOutcome RunTestBody(ITestCommand testCommand, ITestStep parentTestStep,
            Sandbox sandbox, PatternTestHandlerDecorator testHandlerDecorator, PatternTest test)
        {
            TestOutcome outcome = TestOutcome.Error;

            DoWithProcessIsolation(delegate
            {
                sandbox.UseTimeout(test.Timeout, delegate
                {
                    DoWithApartmentState(test.ApartmentState, delegate
                    {
                        IPatternTestHandler testHandler = test.TestActions;

                        if (testHandlerDecorator != null)
                            outcome = testHandlerDecorator(sandbox, ref testHandler);
                        else
                            outcome = TestOutcome.Passed;

                        if (outcome.Status == TestStatus.Passed)
                        {
                            PatternTestStep primaryTestStep = new PatternTestStep(test, parentTestStep);
                            PatternTestState testState = new PatternTestState(primaryTestStep, testHandler, converter,
                                formatter, testCommand.IsExplicit);

                            bool invisible = true;

                            outcome = outcome.CombineWith(DoBeforeTest(sandbox, testState));
                            if (outcome.Status == TestStatus.Passed)
                            {
                                bool reusePrimaryTestStep = !testState.BindingContext.HasBindings;
                                if (!reusePrimaryTestStep)
                                    primaryTestStep.IsTestCase = false;

                                invisible = false;
                                TestContext context = TestContext.PrepareContext(
                                    testCommand.StartStep(primaryTestStep), sandbox);
                                testState.SetInContext(context);

                                outcome = outcome.CombineWith(DoInitializeTest(context, testState));

                                if (outcome.Status == TestStatus.Passed)
                                    outcome =
                                        outcome.CombineWith(RunTestInstances(testCommand, context, testState,
                                            reusePrimaryTestStep));

                                outcome = outcome.CombineWith(DoDisposeTest(context, testState));

                                context.FinishStep(outcome);
                            }

                            outcome = outcome.CombineWith(DoAfterTest(sandbox, testState));

                            if (invisible)
                                PublishOutcomeFromInvisibleTest(testCommand, primaryTestStep, ref outcome);
                        }
                    });
                });
            });

            return outcome;
        }

        private TestOutcome RunTestInstances(ITestCommand testCommand, TestContext primaryContext,
            PatternTestState testState, bool reusePrimaryTestStep)
        {
            try
            {
                TestOutcome outcome = TestOutcome.Passed;
                foreach (IDataItem item in testState.BindingContext.GetItems(!options.SkipDynamicTests))
                {
                    outcome = outcome.CombineWith(RunTestInstance(testCommand, primaryContext, testState, item, reusePrimaryTestStep));
                }

                return reusePrimaryTestStep ? outcome : outcome.Generalize();
            }
            catch (Exception ex)
            {
                TestLog.Failures.WriteException(ex, String.Format("An exception occurred while getting data items for test '{0}'.", testState.Test.FullName));
                return TestOutcome.Error;
            }
        }

        private TestOutcome RunTestInstance(ITestCommand testCommand, TestContext primaryContext,
            PatternTestState testState, IDataItem bindingItem, bool reusePrimaryTestStep)
        {
            try
            {
                PatternTestInstanceActions decoratedTestInstanceActions =
                    PatternTestInstanceActions.CreateDecorator(testState.TestHandler.TestInstanceHandler);

                TestOutcome outcome = DoDecorateTestInstance(primaryContext.Sandbox, testState, decoratedTestInstanceActions);
                if (outcome.Status == TestStatus.Passed)
                {
                    bool invisible = true;

                    PatternTestStep testStep;
                    if (reusePrimaryTestStep)
                    {
                        testStep = testState.PrimaryTestStep;
                        invisible = false;

                        MetadataMap map = DataItemUtils.GetMetadata(bindingItem);
                        foreach (KeyValuePair<string, string> entry in map.Pairs)
                            primaryContext.AddMetadata(entry.Key, entry.Value);
                    }
                    else
                    {
                        testStep = new PatternTestStep(testState.Test, testState.PrimaryTestStep,
                            testState.Test.Name, testState.Test.CodeElement, false);
                        testStep.Kind = testState.Test.Kind;

                        testStep.IsDynamic = bindingItem.IsDynamic;
                        bindingItem.PopulateMetadata(testStep.Metadata);
                    }

                    PatternTestInstanceState testInstanceState = new PatternTestInstanceState(testStep, decoratedTestInstanceActions, testState, bindingItem);

                    outcome = outcome.CombineWith(DoBeforeTestInstance(primaryContext.Sandbox, testInstanceState));
                    if (outcome.Status == TestStatus.Passed)
                    {
                        progressMonitor.SetStatus(testStep.Name);

                        TestContext context = reusePrimaryTestStep
                            ? primaryContext
                            : TestContext.PrepareContext(testCommand.StartStep(testStep), primaryContext.Sandbox.CreateChild());
                        testState.SetInContext(context);
                        testInstanceState.SetInContext(context);
                        invisible = false;

                        outcome = outcome.CombineWith(RunTestInstanceWithContext(testCommand, context, testInstanceState));

                        if (!reusePrimaryTestStep)
                            context.FinishStep(outcome);

                        progressMonitor.SetStatus("");
                    }

                    outcome = outcome.CombineWith(DoAfterTestInstance(primaryContext.Sandbox, testInstanceState));

                    if (invisible)
                        PublishOutcomeFromInvisibleTest(testCommand, testStep, ref outcome);
                }

                return outcome;
            }
            catch (Exception ex)
            {
                string message = String.Format("An exception occurred while preparing an instance of test '{0}'.", testState.Test.FullName);

                if (reusePrimaryTestStep)
                {
                    TestLog.Failures.WriteException(ex, message);
                    return TestOutcome.Error;
                }
                else
                {
                    return ReportTestError(testCommand, testState.PrimaryTestStep, ex, message);
                }
            }
        }

        private TestOutcome RunTestInstanceWithContext(ITestCommand testCommand, TestContext context,
            PatternTestInstanceState testInstanceState)
        {
            try
            {
                if (options.SkipTestExecution)
                {
                    return RunTestChildren(testCommand, context.Sandbox, testInstanceState);
                }
                else
                {
                    TestOutcome outcome = TestOutcome.Passed;                    
                    UpdateInterimOutcome(context, ref outcome, DoInitializeTestInstance(context, testInstanceState));
                    if (outcome.Status == TestStatus.Passed)
                    {
                        UpdateInterimOutcome(context, ref outcome, DoSetUpTestInstance(context, testInstanceState));
                        if (outcome.Status == TestStatus.Passed)
                        {
                            UpdateInterimOutcome(context, ref outcome, DoExecuteTestInstance(context, testInstanceState));

                            if (outcome.Status == TestStatus.Passed)
                            {
                                UpdateInterimOutcome(context, ref outcome, RunTestChildren(testCommand, context.Sandbox, testInstanceState));
                            }
                        }

                        UpdateInterimOutcome(context, ref outcome, DoTearDownTestInstance(context, testInstanceState));
                    }

                    UpdateInterimOutcome(context, ref outcome, DoDisposeTestInstance(context, testInstanceState));
                    return outcome;
                }
            }
            catch (Exception ex)
            {
                TestLog.Failures.WriteException(ex,
                    String.Format("An exception occurred while running test instance '{0}'.", testInstanceState.TestStep.Name));
                return TestOutcome.Error;
            }
        }

        private TestOutcome RunTestChildren(ITestCommand testCommand, Sandbox sandbox,
            PatternTestInstanceState testInstanceState)
        {
            TestOutcome outcome = TestOutcome.Passed;

            foreach (ITestCommand childTestCommand in testCommand.Children)
            {
                if (progressMonitor.IsCanceled)
                    return TestOutcome.Canceled;

                PatternTestHandlerDecorator testHandlerDecorator = delegate(Sandbox childSandbox, ref IPatternTestHandler childHandler)
                {
                    PatternTestActions decoratedChildTestActions = PatternTestActions.CreateDecorator(childHandler);
                    childHandler = decoratedChildTestActions;

                    return DoDecorateChildTest(childSandbox, testInstanceState, decoratedChildTestActions);
                };

                outcome = outcome.CombineWith(RunTest(childTestCommand, testInstanceState.TestStep, sandbox, testHandlerDecorator));
            }

            return outcome.Generalize();
        }

        private static void UpdateInterimOutcome(TestContext context, ref TestOutcome outcome, TestOutcome newOutcome)
        {
            outcome = outcome.CombineWith(newOutcome);
            context.SetInterimOutcome(outcome);
        }

        private static void PublishOutcomeFromInvisibleTest(ITestCommand testCommand, ITestStep testStep, ref TestOutcome outcome)
        {
            switch (outcome.Status)
            {
                case TestStatus.Skipped:
                case TestStatus.Passed:
                    // Either nothing interesting happened or the test was silently skipped during Before/After.
                    outcome = TestOutcome.Passed;
                    break;

                case TestStatus.Failed:
                case TestStatus.Inconclusive:
                default:
                    // Something bad happened during Before/After that prevented the test from running.
                    ITestContext context = testCommand.StartStep(testStep);
                    context.LogWriter.Failures.Write("The test did not run.  Consult the parent test log for more details.");
                    context.FinishStep(outcome, null);
                    outcome = outcome.Generalize();
                    break;
            }
        }

        #region Actions
        [TestEntryPoint]
        private static TestOutcome DoBeforeTest(Sandbox sandbox, PatternTestState testState)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                foreach (PatternTestParameter parameter in testState.Test.Parameters)
                {
                    IDataAccessor accessor = parameter.Binder.Register(testState.BindingContext, parameter.DataContext);
                    testState.SlotBindingAccessors.Add(new KeyValuePair<ISlotInfo, IDataAccessor>(parameter.Slot, accessor));
                }

                testState.TestHandler.BeforeTest(testState);
            }, "Before Test");
        }

        [TestEntryPoint]
        private static TestOutcome DoInitializeTest(TestContext context, PatternTestState testState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.Initialize;

                return context.Sandbox.Run(TestLog.Writer, delegate
                {
                    testState.TestHandler.InitializeTest(testState);
                }, "Initialize");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoDisposeTest(TestContext context, PatternTestState testState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.Dispose;

                return context.Sandbox.Run(TestLog.Writer, delegate
                {
                    testState.TestHandler.DisposeTest(testState);
                }, "Dispose");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoAfterTest(Sandbox sandbox, PatternTestState testState)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                testState.TestHandler.AfterTest(testState);
            }, "After Test");
        }

        [TestEntryPoint]
        private static TestOutcome DoDecorateTestInstance(Sandbox sandbox, PatternTestState testState, PatternTestInstanceActions decoratedTestInstanceActions)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                testState.TestHandler.DecorateTestInstance(testState, decoratedTestInstanceActions);
            }, "Decorate Child Test");
        }

        [TestEntryPoint]
        private static TestOutcome DoBeforeTestInstance(Sandbox sandbox, PatternTestInstanceState testInstanceState)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                if (testInstanceState.TestState.SlotBindingAccessors.Count != 0)
                {
                    foreach (KeyValuePair<ISlotInfo, IDataAccessor> entry in testInstanceState.TestState.SlotBindingAccessors)
                    {
                        object value = Bind(entry.Value, testInstanceState.BindingItem, testInstanceState.Formatter);
                        testInstanceState.SlotValues.Add(entry.Key, value);
                    }
                }

                testInstanceState.TestInstanceHandler.BeforeTestInstance(testInstanceState);
            }, "Before Test Instance");
        }

        [TestEntryPoint]
        private static TestOutcome DoInitializeTestInstance(TestContext context, PatternTestInstanceState testInstanceState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.Initialize;

                return context.Sandbox.Run(context.LogWriter, delegate
                {
                    testInstanceState.TestInstanceHandler.InitializeTestInstance(testInstanceState);
                }, "Initialize");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoSetUpTestInstance(TestContext context, PatternTestInstanceState testInstanceState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.SetUp;

                return context.Sandbox.Run(context.LogWriter, delegate
                {
                    testInstanceState.TestInstanceHandler.SetUpTestInstance(testInstanceState);
                }, "Set Up");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoExecuteTestInstance(TestContext context, PatternTestInstanceState testInstanceState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.Execute;

                return context.Sandbox.Run(context.LogWriter, delegate
                {
                    testInstanceState.TestInstanceHandler.ExecuteTestInstance(testInstanceState);
                }, null);
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoTearDownTestInstance(TestContext context, PatternTestInstanceState testInstanceState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.TearDown;

                return context.Sandbox.Run(context.LogWriter, delegate
                {
                    testInstanceState.TestInstanceHandler.TearDownTestInstance(testInstanceState);
                }, "Tear Down");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoDisposeTestInstance(TestContext context, PatternTestInstanceState testInstanceState)
        {
            using (context.Enter())
            {
                context.LifecyclePhase = LifecyclePhases.Dispose;

                return context.Sandbox.Run(context.LogWriter, delegate
                {
                    testInstanceState.TestInstanceHandler.DisposeTestInstance(testInstanceState);
                }, "Dispose");
            }
        }

        [TestEntryPoint]
        private static TestOutcome DoAfterTestInstance(Sandbox sandbox, PatternTestInstanceState testInstanceState)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                testInstanceState.TestInstanceHandler.AfterTestInstance(testInstanceState);
            }, "After Test Instance");
        }

        [TestEntryPoint]
        private static TestOutcome DoDecorateChildTest(Sandbox sandbox, PatternTestInstanceState testInstanceState, PatternTestActions decoratedChildTestActions)
        {
            return sandbox.Run(TestLog.Writer, delegate
            {
                testInstanceState.TestInstanceHandler.DecorateChildTest(testInstanceState, decoratedChildTestActions);
            }, "Decorate Child Test");
        }
        #endregion

        private static object Bind(IDataAccessor accessor, IDataItem bindingItem, IFormatter formatter)
        {
            try
            {
                return accessor.GetValue(bindingItem);
            }
            catch (DataBindingException ex)
            {
                using (TestLog.Failures.BeginSection("Data binding failure"))
                {
                    if (!string.IsNullOrEmpty(ex.Message))
                        TestLog.Failures.WriteLine(ex.Message);

                    bool first = true;
                    foreach (DataBinding binding in bindingItem.GetBindingsForInformalDescription())
                    {
                        if (first)
                        {
                            TestLog.Failures.Write("\nAvailable data bindings for this item:\n\n");
                            first = false;
                        }

                        using (TestLog.Failures.BeginMarker(Marker.Label))
                        {
                            TestLog.Failures.Write(binding);
                            TestLog.Failures.Write(": ");
                        }

                        TestLog.Failures.WriteLine(bindingItem.GetValue(binding));
                    }

                    if (first)
                        TestLog.Failures.Write("\nThis item does not appear to provide any data bindings.\n");
                }

                throw new SilentTestException(TestOutcome.Error);
            }
        }

        private static TestOutcome ReportTestError(ITestCommand testCommand, ITestStep parentTestStep, Exception ex, string message)
        {
            ITestContext context = testCommand.StartPrimaryChildStep(parentTestStep);
            TestLog.Failures.WriteException(ex, message);
            context.FinishStep(TestOutcome.Error, null);
            return TestOutcome.Error;
        }

        private static void DoWithProcessIsolation(Action action)
        {
            using (new ProcessIsolation())
            {
                action();
            }
        }

        private static void DoWithApartmentState(ApartmentState apartmentState, Action action)
        {
            if (apartmentState != ApartmentState.Unknown
                && Thread.CurrentThread.GetApartmentState() != apartmentState)
            {
                ThreadTask task = new ThreadTask("Test Runner " + apartmentState, action);
                task.ApartmentState = apartmentState;
                task.Run(null);

                if (task.Result.Exception != null)
                    throw new ModelException(String.Format("Failed to perform action in thread with overridden apartment state {0}.",
                        apartmentState), task.Result.Exception);
            }
            else
            {
                action();
            }
        }
    }
}
