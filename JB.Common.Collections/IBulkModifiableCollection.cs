// -----------------------------------------------------------------------
// <copyright file="IBulkModifiableCollection.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace JB.Collections
{
    /// <summary>
    /// A <see cref="ICollection{T}"/> that provides bulk add- and remove modifications.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public interface IBulkModifiableCollection<T> : ICollection<T>
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