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
        /// Adds a range of items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        void AddRange(ICollection<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes the specified items.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        void RemoveRange(ICollection<KeyValuePair<TKey, TValue>> items);
    }
}