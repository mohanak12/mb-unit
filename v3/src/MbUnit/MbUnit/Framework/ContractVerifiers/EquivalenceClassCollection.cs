﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MbUnit.Framework.ContractVerifiers
{
    /// <summary>
    /// <para>
    /// A collection of equivalence classes. 
    /// </para>
    /// <para>
    /// Equivalent classes are used by some contract verifiers such as 
    /// <see cref="VerifyEqualityContractAttribute"/> to check for the correct
    /// implementation of object equality or comparison.
    /// </para>
    /// <para>
    /// You can construct and initialize a collection of equivalence classes either by using the
    /// constructor, or by using the factory method.
    /// <see cref="T:EquivalenceClassCollection.FromDistinctInstances"/> in more simple scenarios.
    /// <list type="bullet">
    /// <item>
    /// Use the constructor when you want to create equivalence classes containing a variable 
    /// number of object instances.
    /// <example>
    /// <code><![CDATA[
    /// EquivalenceClassCollection<Foo> collection = new EquivalenceClassCollection<Foo>(
    ///     new EquivalenceClass<Foo>(
    ///         new EquivalenceClass<Foo>(
    ///             new Foo(7, 2));
    ///         new EquivalenceClass<Foo>(
    ///             new Foo(25, 2), 
    ///             new Foo(10, 5));
    ///         new EquivalenceClass<Foo>(
    ///             new Foo(3, 4), 
    ///             new Foo(2, 6), 
    ///             new Foo(1, 12)));
    /// ]]></code>
    /// </example>
    /// </item>
    /// <item>
    /// Use the factory method for more simple scenarios, when the equivalence classes
    /// will only contain one single object instance.
    /// <example>
    /// <code><![CDATA[
    /// EquivalenceClassCollection<Foo> collection = 
    ///     EquivalenceClassCollection<Foo>.FromDistinctInstances(
    ///         new Foo(7, 2),
    ///         new Foo(5, 3), 
    ///         new Foo(3, 4));
    /// ]]></code>
    /// </example>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of equivalent object instances.</typeparam>
    public class EquivalenceClassCollection<T> : IEnumerable<EquivalenceClass<T>>
    {
        private readonly List<EquivalenceClass<T>> equivalenceClasses;

        private EquivalenceClassCollection()
        {
            equivalenceClasses = new List<EquivalenceClass<T>>();
        }

        /// <summary>
        /// Constructs a collection of equivalence classes.
        /// </summary>
        /// <param name="equivalenceClasses">An array of equivalence classes.</param>
        public EquivalenceClassCollection(params EquivalenceClass<T>[] equivalenceClasses)
        {
            if (equivalenceClasses == null)
            {
                throw new ArgumentNullException("equivalenceClasses", String.Format("A collection of equivalence classes " +
                    "of type '{0}' cannot be initialized from a null reference.", typeof(T)));
            }

            this.equivalenceClasses = new List<EquivalenceClass<T>>();
            foreach (EquivalenceClass<T> item in equivalenceClasses)
            {
                if (item == null)
                {
                    throw new ArgumentException(String.Format("One of the equivalence classes provided to " +
                        "the constructor of the equivalence class collection of type '{0}' is a null reference.", 
                        typeof(T)), "equivalenceClasses");
                }

                this.equivalenceClasses.Add(item);
            }
        }

        /// <summary>
        /// Constructs a collection of equivalence classes from the specified distinct object
        /// instances. The collection will contain one equivalence class for each of the
        /// distinct object instances provided.
        /// </summary>
        /// <param name="distinctInstances">An array of distinct object instances.</param>
        /// <returns></returns>
        public static EquivalenceClassCollection<T> FromDistinctInstances(params T[] distinctInstances)
        {
            EquivalenceClassCollection<T> collection = new EquivalenceClassCollection<T>();

            if (distinctInstances == null)
            {
                if (default(T) != null)
                    throw new ArgumentNullException("distinctInstances", "The instance cannot be null for a value type.");

                collection.equivalenceClasses.Add(new EquivalenceClass<T>(null));
            }
            else
            {
                foreach (T instance in distinctInstances)
                    collection.equivalenceClasses.Add(new EquivalenceClass<T>(instance));
            }

            return collection;
        }

        /// <summary>
        /// Gets the equivalence classes.
        /// </summary>
        public IList<EquivalenceClass<T>> EquivalenceClasses
        {
            get { return new ReadOnlyCollection<EquivalenceClass<T>>(equivalenceClasses); }
        }

        /// <summary>
        /// Returns a strongly-typed enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A strongly-typed enumerator.</returns>
        public IEnumerator<EquivalenceClass<T>> GetEnumerator()
        {
            return equivalenceClasses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
