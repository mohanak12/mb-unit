// created on 30/01/2004 at 11:15

namespace MbUnit.Framework {
    using System;
    using System.Collections;
    using MbUnit.Core;
    using MbUnit.Core.Framework;
    using System.Diagnostics;
    using MbUnit.Core.Invokers;
    using MbUnit.Core.Runs;

    /// <summary>
    /// Tags the class as a TypeFixture
    /// </summary>
    /// <remarks>
    /// <para>The type-specific fixture assumes that all tests contained in the fixture that have an argument of the same 
    /// type specified in the <see cref="TypeFixtureAttribute"/> will be provided by the methods tagged by 
    /// <see cref="ProviderAttribute"/> or by a class specified by the <see cref="ProviderFactoryAttribute"/>
    /// which tags the same fixture class as the <see cref="TypeFixtureAttribute"/>.</para>
    /// <para><b>This fixture is particularly useful for writing fixtures of interfaces and apply it to all the types that implement the interface.</b></para>
    /// <para>The TypeFixture has the following execution logic:</para>
    /// <list type="number">
    /// <item>(optional)Set-up the fixture, (SetUpAttribute)</item>
    /// <item>Get an instance of the tested type provided by the user (ProviderAttribute)</item>
    /// <item>Get instances of the tested type provided by a factory (ProviderFactoryAttribute)</item>
    /// <item>Run test method with this instance as argument (TestAttribute)</item>
    /// <item>(optional)Teardown fixture (TearDownAttribute)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>The following example shows a class called ListFactory being identified as a provider factory 
    /// for a type fixture that tests IEnumerable objects. These objects are generated by the [Provider]-tagged methods
    /// in the ListTester class and by the public [Factory]-tagged properties in the ListFactory class</para>
    /// <code>
    /// public class ListFactory
    /// {
    ///     [Factory]
    ///     public GetArrayList
    ///     { 
    ///         get { return new ArrayList(); 
    ///     } 
    /// 
    ///     [Factory]
    ///     public GetIntArray
    ///     {
    ///         get { return new int[] {1, 2, 3}; }
    ///     }
    /// }
    /// 
    /// [TypeFixture(typeof(IList),"IList test")]
    /// [ProviderFactory(typeof(ListFactory), typeof(IEnumerable))]
    /// public class ListTester
    /// {
    ///     [Provider(typeof(ArrayList))]
    ///     public ArrayList ProvideFilledArrayList()
    ///     {
    ///         ArrayList list = new ArrayList();
    ///     	list.Add("hello");
    ///     	list.Add("world");
    ///     	return list;
    ///     }
    ///
    ///     [Test]
    ///     [ExpectedException(typeof(InvalidOperationException))]
    ///     public void CurrentCalledBeforeMoveNext(IEnumerable en)
    ///     {
    ///           IEnumerator  er = en.GetEnumerator(); 
    ///           object p = er.Current;
    ///     } 
    /// 
    ///     [Test]
    ///     [ExpectedException(typeof(InvalidOperationException))]
    ///     public void CurrentCalledAfterFinishedMoveNext(IEnumerable en)
    ///     {
    ///           IEnumerator  er = en.GetEnumerator(); 
    ///           while(er.MoveNext());
    ///           object p = er.Current;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ProviderAttribute"/>
    /// <seealso cref="FactoryAttribute"/>
    /// <seealso cref="ProviderFactoryAttribute"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TypeFixtureAttribute : TestFixturePatternAttribute {
        private Type testedType;

        /// <summary>
        /// Creates a fixture for the <paramref name="testedType"/> type.
        /// </summary>
        /// <param name="testedType">type to apply the fixture to</param>
        /// <remarks>
        /// Initializes the attribute with <paramref name="testedType"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">testedType is a null reference</exception>
        public TypeFixtureAttribute(Type testedType)
            : base() {
            if (testedType == null)
                throw new ArgumentNullException("testedType");
            this.testedType = testedType;
        }

        /// <summary>
        /// Creates a fixture for the <paramref name="testedType"/> type 
        /// and a description
        /// </summary>
        /// <remarks>
        /// Initializes the attribute with <paramref name="testedType"/>.
        /// </remarks>
        /// <param name="testedType">type to apply the fixture to</param>
        /// <param name="description">description of the fixture</param>
        /// <exception cref="ArgumentNullException">testedType is a null reference</exception>
        public TypeFixtureAttribute(Type testedType, string description)
            : base(description) {
            if (testedType == null)
                throw new ArgumentNullException("testedType");
            this.testedType = testedType;
        }

        /// <summary>
        /// Gets the test runner class defining all the tests to be run and the test logic to be used within the tagged fixture class.
        /// </summary>
        /// <returns>A <see cref="SequenceRun"/> object</returns>
        public override IRun GetRun() {
            SequenceRun runs = new SequenceRun();

            // creating parallel
            ParallelRun para = new ParallelRun();
            para.AllowEmpty = false;
            runs.Runs.Add(para);

            // method providers
            MethodRun provider = new MethodRun(
                                               typeof(ProviderAttribute),
                                               typeof(ArgumentFeederRunInvoker),
                                               false,
                                               true
                                               );
            para.Runs.Add(provider);

            // fixture class provider
            FixtureDecoratorRun providerFactory = new FixtureDecoratorRun(
                typeof(ProviderFixtureDecoratorPatternAttribute)
                );
            para.Runs.Add(providerFactory);

            // setup
            OptionalMethodRun setup = new OptionalMethodRun(typeof(SetUpAttribute), false);
            runs.Runs.Add(setup);

            // tests
            MethodRun test = new MethodRun(typeof(TestPatternAttribute), true, true);
            runs.Runs.Add(test);

            // tear down
            OptionalMethodRun tearDown = new OptionalMethodRun(typeof(TearDownAttribute), false);
            runs.Runs.Add(tearDown);

            return runs;
        }
    }

}
