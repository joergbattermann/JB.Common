using System.Collections.Generic;

namespace JB.Collections
{
    /// <summary>
    /// Provides bulk add- and remove modifications.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public interface IBulkModifiable<in T>
    {
        /// <summary>
        /// Adds a range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Removes the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        void RemoveRange(IEnumerable<T> items);
    }
}