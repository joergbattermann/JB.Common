using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections
{
	public interface IReactiveCollection<T> : INotifyReactiveCollectionChanged<T>, IReadOnlyCollection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
	{
		/// <summary>
		/// Adds a range of items.
		/// </summary>
		/// <param name="items">The items.</param>
		void AddRange(IEnumerable<T> items);

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
		void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = false);

		/// <summary>
		/// Moves the item(s) at the specified index to a new position in the list.
		/// </summary>
		/// <param name="itemIndex">The (starting) index of the item(s) to move.</param>
		/// <param name="newIndex">The new index.</param>
		/// <param name="count">The amount of items to move, starting at the itemIndex.</param>
		/// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
		/// if required, depending on whether an index shift took place during the move due to the original position of the item.
		/// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than <paramref name="itemIndex" />
		/// will be shifted upwards (logically by -1).
		/// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex" /> position, correction might be useful.</param>
		void Move(int itemIndex, int newIndex, int count = 1, bool correctNewIndexOnIndexShift = false);

        /// <summary>
        /// Inserts the range of items at the specified <paramref name="index"/> position.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="index">The index.</param>
        void InsertRange(IEnumerable<T> items, int index);

        /// <summary>
        /// Removes the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        void RemoveRange(IEnumerable<T> items);

		/// <summary>
		/// Removes the amount of items starting at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		void RemoveRange(int index, int count);

		/// <summary>
		/// Resets this instance.
		/// </summary>
		void Reset();
    }
}