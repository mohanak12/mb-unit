using System;
using System.Collections;
using System.Collections.Generic;

namespace MbUnit.Framework.Kernel.Collections
{
    /// <summary>
    /// A read-only empty dictionary.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type</typeparam>
    /// <typeparam name="TValue">The dictionary value type</typeparam>
    public class EmptyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// A read-only empty dictionary instance.
        /// </summary>
        public static readonly IDictionary<TKey, TValue> Instance =new EmptyDictionary<TKey, TValue>();

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return false;
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            ThrowCollectionIsReadOnly();
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            ThrowCollectionIsReadOnly();
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get { throw new KeyNotFoundException(); }
            set { ThrowCollectionIsReadOnly(); }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get { return EmptyArray<TKey>.Instance; }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get { return EmptyArray<TValue>.Instance; }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ThrowCollectionIsReadOnly();
        }

        /// <inheritdoc />
        public void Clear()
        {
            ThrowCollectionIsReadOnly();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return false;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            ThrowCollectionIsReadOnly();
            return false;
        }

        /// <inheritdoc />
        public int Count
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            yield break;
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            yield break;
        }

        private static void ThrowCollectionIsReadOnly()
        {
            throw new NotSupportedException("Dictionary is read-only.");
        }
    }
}