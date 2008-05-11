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

namespace Gallio.Model
{
    /// <summary>
    /// <para>
    /// A test step is a delimited region of a test defined at run-time.
    /// Each test that is executed consists of at least one step: the "primary" step.
    /// During execution, each test may spawn additional nested steps that may run in
    /// parallel or in series with one another as required.
    /// </para>
    /// <para>
    /// A test step may be used to describe a particular instance of a data driven test.
    /// It can also be used to generate a completely dynamic hierarchy at runtime that
    /// mirrors and extends the static test hierarchy with new information derived from
    /// the actual execution of the test.
    /// </para>
    /// <para>
    /// Each step has its own execution log and test result (pass/fail/inconclusive).
    /// Therefore a multi-step test may possess multiple execution logs and test results.
    /// This is deliberate.  Think of a <see cref="ITest" /> as being the declarative component
    /// of a test that specifies which test method to invoke and its arguments.  An
    /// <see cref="ITestStep" /> is the runtime counterpart of the <see cref="ITest" /> that
    /// captures output and control flow information about part or all of the test.
    /// </para>
    /// <para>
    /// A test step also has metadata that can be update at run-time to carry additional
    /// declarative information about the step.
    /// </para>
    /// </summary>
    public interface ITestStep : ITestComponent
    {
        /// <summary>
        /// <para>
        /// Gets the full name of the step.
        /// </para>
        /// <para>
        /// The full name is derived by concatenating the <see cref="FullName" /> of the
        /// <see cref="Parent"/> followed by a slash ('/') followed by the <see cref="ITestComponent.Name" />
        /// of this test step.
        /// </para>
        /// <para>
        /// The full name of the root test step is empty.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item><term>""</term><description>The root step</description></item>
        /// <item><term>"SomeAssembly/SomeFixture/SomeTest"</term><description>The step corresponding to SomeTest</description></item>
        /// <item><term>"SomeAssembly/SomeFixture/SomeTest/ChildStep"</term><description>A child step of SomeTest</description></item>
        /// </list>
        /// </para>
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the step that contains this one, or null if this instance represents the
        /// root step of the test step tree.
        /// </summary>
        ITestStep Parent { get; }

        /// <summary>
        /// Gets the test to which this step belongs.
        /// </summary>
        ITest Test { get; }

        /// <summary>
        /// <para>
        /// Returns true if the test step is the top node of a hierarchy of test steps that are
        /// all associated with the same test.  In the case where a single test step is associated
        /// with a test, then it is the primary test step.
        /// There may be multiple primary test steps of the same test if they are not
        /// directly related by ancestry (ie. one does not contain the other).
        /// </para>
        /// <para>
        /// A non-primary test step is known as a derived test step.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For example, suppose we have a data-driven test fixture F which contains a data-driven
        /// test T.  There will be a primary test step of F that represents the undifferentiated
        /// fixture with no data bound to it.  Its immediate children will be derived test steps of F with particular data bindings.
        /// Similarly, each of these derived test steps of F will contain a primary test step of T
        /// which then contains derived test steps of T.
        /// </para>
        /// <para>
        /// This case is illustrated like this where periods delimit path segments in the test step hierarchy.
        /// <list type="bullet">
        /// <item><term>F</term><description>Primary test step of F</description></item>
        /// <item><term>F.F1</term><description>Non-primary test step of F with data bindings</description></item>
        /// <item><term>F.F1.T</term><description>Primary test step of T</description></item>
        /// <item><term>F.F1.T.T1</term><description>Non-primary test step of T with data bindings</description></item>
        /// <item><term>F.F1.T.T2</term><description>Non-primary test step of T with data bindings</description></item>
        /// <item><term>F.F2</term><description>Non-primary test step of F with data bindings</description></item>
        /// <item><term>F.F2.T</term><description>Primary test step of T</description></item>
        /// <item><term>F.F2.T.T1</term><description>Non-primary test step of T with data bindings</description></item>
        /// <item><term>F.F2.T.T2</term><description>Non-primary test step of T with data bindings</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        bool IsPrimary { get; }

        /// <summary>
        /// <para>
        /// Returns true if the test step is dynamic.  A dynamic test step is one whose
        /// execution is governed by parameters that are not known a priori.  
        /// </para>
        /// <para>
        /// For example, a primary test step is generally not dynamic because its existence
        /// usually only depends on statically known information derived from the test model.
        /// However, a child test step created at runtime by a data binding process might be
        /// considered dynamic if the data items that were used may change between test runs
        /// even when the static structure of the test code remains the same.  So a test step
        /// that uses random data items or that owes its existence to processes that are not
        /// entirely under the control of the test framework should be flagged as dynamic.
        /// </para>
        /// <para>
        /// It can be useful to distinguish between static and dynamic test steps when
        /// correlating results across test runs.  Dynamic test steps are more likely to change
        /// in dramatic ways between test runs than static test steps.
        /// </para>
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// <para>
        /// Returns true if the test step represents a distinct test case.  A test case is typically a leaf
        /// of the test step hierarchy.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// A test step can only be a test case if its associated <see cref="ITest" /> is a test case.  However,
        /// not all test steps of tests that are test cases will themselves be test cases.  For example, a
        /// data-driven test might have one primary test step with multiple derived test steps as children.
        /// The primary test step will generally not be considered a test case but its children will.
        /// </para>
        /// </remarks>
        /// <seealso cref="ITest.IsTestCase"/>
        bool IsTestCase { get; }
    }
}
