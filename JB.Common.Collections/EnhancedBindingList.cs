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

namespace JB.Common.Collections
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
		/// Initializes a new instance of the <see cref="EnhancedBindingList{T}"/> class.
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
		///     Moves the item at the specified index to a new position in the list.
		/// </summary>
		/// <param name="itemIndex">The index of the item to move.</param>
		/// <param name="newIndex">The new index.</param>
		/// <exception cref="ArgumentOutOfRangeException">item</exception>
		public void Move(int itemIndex, int newIndex)
		{
			Move(this[itemIndex], newIndex);
		}

		/// <summary>
		///     Moves the specified item to a new index position in the list.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="newIndex">The new index.</param>
		/// <exception cref="ArgumentOutOfRangeException">item</exception>
		public void Move(T item, int newIndex)
		{
			// temporarily disabling event notifications to prevent a second, duplicate ListChanged event by the underlying bindinglist itself
			var originalRaiseListChangedEventsValue = RaiseListChangedEvents;
			try
			{
				RaiseListChangedEvents = false;
				var originalIndex = IndexOf(item);

				if (originalIndex <= -1)
					throw new ArgumentOutOfRangeException("item");

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
				if (originalIndex > newIndex)
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
		///     <see cref="ItemRemovedListChangedEventArgs{T}" /> as its argument.
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
					OnListChanged(new ItemRemovedListChangedEventArgs<T>(this[index], index));
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