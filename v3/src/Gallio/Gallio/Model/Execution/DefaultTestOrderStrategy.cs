﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// <para>
    /// Defines the test ordering strategy.
    /// </para>
    /// <para>
    /// This default strategy first compares test by explicit ordering (using <see cref="ITest.Order"/>)
    /// then defines an implicit ordering by name (using <see cref="ITestComponent.Name"/>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Why implicitly order tests by name instead of randomly?
    /// </para>
    /// <para>
    /// It is my belief that ordering tests by name significantly eases test result interpretation
    /// because the test results are produced in a meaningful and predictable order.  If the implicit
    /// order is known, then it's easier for a tester to guage test progress by inspecting the name
    /// of the currently executing test (in addition to using cues provided by progress monitors, of course).
    /// Related tests also tend to have similar names or share a common prefix so sorting by name
    /// can effectively cluster these tests together thereby providing feedback about related topics
    /// around the same time.
    /// </para>
    /// <para>
    /// A tester may of course take advantage of this known ordering to produce tests that are
    /// dependent upon one another without making that clear in the code by specifying explicit test
    /// dependencies or ordering.  When a test is renamed, the implicit ordering will change, possibly
    /// causing other tests to fail.  However, it is not Gallio's objective to police
    /// testers, who may well adopt any number of inadvisable practices to meet their ends.
    /// </para>
    /// <para>
    /// Nevertheless, any test framework is still free to produce a random ordering of independent tests.
    /// It suffices for the framework to initialize the <see cref="ITest.Order" /> property of each of its
    /// testsw to a distinct random number.  Problem solved.
    /// </para>
    /// <para>
    /// Perhaps someday we can also offer the user a global choice among alternative test ordering strategies.
    /// </para>
    /// <para>
    /// -- Jeff.
    /// </para>
    /// </remarks>
    public sealed class DefaultTestOrderStrategy : IComparer<ITest>
    {
        /// <inheritdoc />
        public int Compare(ITest a, ITest b)
        {
            int discriminator = a.Order.CompareTo(b.Order);
            if (discriminator == 0)
                discriminator = a.Name.CompareTo(b.Name);

            return discriminator;
        }
    }
}
