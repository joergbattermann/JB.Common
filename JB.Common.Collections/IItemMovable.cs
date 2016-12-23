// -----------------------------------------------------------------------
// <copyright file="IItemMovableList.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

namespace JB.Collections
{
    /// <summary>
    /// Provides moving of its items from one index position to another
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItemMovable<in T>
    {
        /// <summary>
        /// Moves the specified item to the new index position.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
        /// if required, depending on whether an index shift took place during the move due to the original position of the item.
        /// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than the <paramref name="item" /> ones
        /// will be shifted upwards (logically by -1).
        /// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex"/> position, correction might be useful.</param>
        void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = true);

        /// <summary>
        /// Moves the item(s) at the specified index to a new position in the list.
        /// </summary>
        /// <param name="itemIndex">The (starting) index of the item(s) to move.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
        ///     if required, depending on whether an index shift took place during the move due to the original position of the item.
        ///     Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than <paramref name="itemIndex" />
        ///     will be shifted upwards (logically by -1).
        ///     Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex" /> position, correction might be useful.</param>
        void Move(int itemIndex, int newIndex, bool correctNewIndexOnIndexShift = true);
    }
}