// -----------------------------------------------------------------------
// <copyright file="EnhancedBindingList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JB.Collections
{
	/// <summary>
	///     Provides a Move operation and also a more details for deleted Item(s).
	///     Loosely based on a
	///     <see
	///         href="http://stackoverflow.com/questions/23339233/get-deleted-item-in-itemchanging-event-of-bindinglist/23453576#23453576">
	///         StackOverflow
	///     </see>
	///     post by
	///     <see
	///         href="http://stackoverflow.com/questions/23339233/get-deleted-item-in-itemchanging-event-of-bindinglist/23453576#23453576">
	///         Simon
	///         Mourier
	///     </see>
	///     .
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class EnhancedBindingList<T> : BindingList<T>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EnhancedBindingList{T}" /> class.
		/// </summary>
		/// <param name="list">
		///     An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the
		///     <see cref="T:System.ComponentModel.BindingList`1" />.
		/// </param>
		public EnhancedBindingList(IList<T> list = null)
			: base(list ?? new List<T>())
		{
		}

        /// <summary>
        /// Adds the range of items. Use <see cref="BindingList{T}.RaiseListChangedEvents" /> to control whether the range addition will
        /// be communicated via an implicit and per-item <see cref="ListChangedType.ItemAdded"/> event.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> a <see cref="ListChangedType.Reset"/> will be signaled when finished.
        /// This and <see cref="BindingList{T}.RaiseListChangedEvents"/> control if and what <see cref="IBindingList.ListChanged" />
        /// event will be raised while / after adding the <paramref name="items"/>.</param>
        public void AddRange(IEnumerable<T> items, bool signalResetWhenFinished = false)
		{
			if (items == null)
				return;

            foreach (var item in items)
            {
                Add(item);
            }

            if (signalResetWhenFinished)
                ResetBindings();
        }

        /// <summary>
        /// Removes the range of items. Use <see cref="BindingList{T}.RaiseListChangedEvents" /> to control whether the range addition will
        /// be communicated via an implicit and per-item <see cref="ListChangedType.ItemDeleted"/> event.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> a <see cref="ListChangedType.Reset"/> will be signaled when finished.
        /// This and <see cref="BindingList{T}.RaiseListChangedEvents"/> control if and what <see cref="IBindingList.ListChanged" />
        /// event will be raised while / after adding the <paramref name="items"/>.</param>
        public void RemoveRange(IEnumerable<T> items, bool signalResetWhenFinished = false)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Remove(item);
            }

            if (signalResetWhenFinished)
                ResetBindings();
        }

        /// <summary>
        /// Moves the item at the specified index to a new position in the list.
        /// </summary>
        /// <param name="itemIndex">The index of the item to move.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
        /// if required, depending on whether an index shift took place during the move due to the original position of the item.
        /// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than <paramref name="itemIndex" />
        /// will be shifted upwards (logically by -1).
        /// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex"/> position, correction might be useful.</param>
        /// <exception cref="ArgumentOutOfRangeException">item</exception>
        public void Move(int itemIndex, int newIndex, bool correctNewIndexOnIndexShift = true)
		{
			Move(this[itemIndex], newIndex, correctNewIndexOnIndexShift);
		}

		/// <summary>
		///     Moves the specified item to a new index position in the list.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="newIndex">The new index.</param>
		/// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
		/// if required, depending on whether an index shift took place during the move due to the original position of the item.
		/// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than the <paramref name="item" /> ones
		/// will be shifted upwards (logically by -1).
		/// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex"/> position, correction might be useful.</param>
		/// <exception cref="ArgumentOutOfRangeException">item</exception>
		public void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = true)
		{
			// temporarily disabling event notifications to prevent a second, duplicate ListChanged event by the underlying bindinglist itself
			var originalRaiseListChangedEventsValue = RaiseListChangedEvents;
			try
			{
				RaiseListChangedEvents = false;
				var originalIndex = IndexOf(item);

				if (originalIndex <= -1)
					throw new ArgumentOutOfRangeException(nameof(item));

				// first check whether there's no real move, but item kept exactly at its place
				if (originalIndex == newIndex)
				{
					return;
				}

				// otherwise a move takes place, starting of by removing the item itself from the list
				base.RemoveItem(originalIndex);

				// this will cause an index shift - depending on the item's original position relative to the new index
				// this must be corrected.

				// if the original position.. where it was taken out.. was below the newIndex, the indexshift has no impact on the newindex,
				// hence it can be inserted at the new index without troubles.
				int actuallyUsedNewIndex;
				if (originalIndex > newIndex || correctNewIndexOnIndexShift == false)
				{
					actuallyUsedNewIndex = newIndex;
					InsertItem(newIndex, item);
				}
				else
				{
					// otherwise the newIndex is now off by one due to the removal's indexshift
					var correctedNewIndex = newIndex - 1;
					var currentCount = Count;
					if (correctedNewIndex > currentCount)
					{
						correctedNewIndex = currentCount;
					}

					// .. and insert it there
					actuallyUsedNewIndex = correctedNewIndex;
					InsertItem(correctedNewIndex, item);
				}

				// finally check whether this list shall actually raise events, at all
				if (originalRaiseListChangedEventsValue)
				{
					// only if removal was performed, raise corresponding even here
					OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, actuallyUsedNewIndex, originalIndex));
				}
			}
			finally
			{
				// set underlying's list RaiseListChangedEvents back to its originally captured value
				RaiseListChangedEvents = originalRaiseListChangedEventsValue;
			}
		}

		/// <summary>
		///     Removes the item at the specified index and follow-up ListChanged event will have an
		///     <see cref="ItemDeletedListChangedEventArgs{T}" /> as its argument.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.NotSupportedException">
		///     You are removing a newly added item and
		///     <see cref="P:System.ComponentModel.IBindingList.AllowRemove" /> is set to false.
		/// </exception>
		protected override void RemoveItem(int index)
		{
			// temporarily disabling event notifications to prevent a second, duplicate ListChanged event by the underlying bindinglist itself
			var originalRaiseListChangedEventsValue = RaiseListChangedEvents;

			try
			{
				RaiseListChangedEvents = false;
				base.RemoveItem(index);

				// check whether this list shall actually raise events, at all
				if (originalRaiseListChangedEventsValue)
				{
					// only if removal was performed, raise corresponding even here
					OnListChanged(new ItemDeletedListChangedEventArgs<T>(this[index], index));
				}
			}
			finally
			{
				// set underlying's list RaiseListChangedEvents back to its originally captured value
				RaiseListChangedEvents = originalRaiseListChangedEventsValue;
			}
		}
	}
}