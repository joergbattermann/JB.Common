using System.Collections.Generic;

namespace JB.Collections
{
    /// <summary>
    /// A <see cref="IDictionary{TKey,TValue}" /> that provides bulk add- and remove modifications.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IBulkModifiableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Adds a range of key/value pair(s).
        /// </summary>
        /// <param name="items">The items to add.</param>
        void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes the specified key/value pair(s).
        /// </summary>
        /// <param name="items">The items to remove.</param>
        void RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes the items for the provided <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys.</param>
        void RemoveRange(IEnumerable<TKey> keys);
    }
}