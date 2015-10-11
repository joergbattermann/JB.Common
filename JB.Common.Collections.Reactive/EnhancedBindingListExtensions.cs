// -----------------------------------------------------------------------
// <copyright file="EnhancedBindingListExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace JB.Common.Collections
{
	/// <summary>
	///     Provides Extension Method(s) for <see cref="EnhancedBindingList{T}" /> instances.
	/// </summary>
	public static class EnhancedBindingListExtensions
	{
		/// <summary>
		/// Forwards the <paramref name="sourceBindingList" /> changes to the <paramref name="targetReactiveList" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceBindingList">The source binding list.</param>
		/// <param name="targetReactiveList">The target reactive list.</param>
		/// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="targetReactiveList" /> via replacing the item completely.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">sourceReactiveList
		/// or
		/// targetBindingList</exception>
		/// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
		/// the exact same index position(s)</exception>
		public static IDisposable ForwardListChangesTo<T>(this EnhancedBindingList<T> sourceBindingList, ReactiveList<T> targetReactiveList, bool includeItemChanges = false)
		{
			if (sourceBindingList == null) throw new ArgumentNullException("sourceBindingList");
			if (targetReactiveList == null) throw new ArgumentNullException("targetReactiveList");

			if (sourceBindingList.Except(targetReactiveList, EqualityComparer<T>.Default).Any()
				|| targetReactiveList.Except(sourceBindingList, EqualityComparer<T>.Default).Any()
				|| sourceBindingList.Any(element => sourceBindingList.IndexOf(element) != targetReactiveList.IndexOf(element)))
			{
				throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
			}

			return Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
				handler => sourceBindingList.ListChanged += handler,
				handler => sourceBindingList.ListChanged -= handler)
				.Subscribe(eventPattern => OnNextListChanged(eventPattern, targetReactiveList, includeItemChanges));
		}

		/// <summary>
		/// Handler for List Changed Events
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventPattern">The event pattern.</param>
		/// <param name="targetReactiveList">The target reactive list.</param>
		/// <param name="includeItemChanges">if set to <c>true</c> [include item changes].</param>
		/// <exception cref="System.ArgumentNullException">
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">eventPattern
		/// or
		/// eventPattern</exception>
		private static void OnNextListChanged<T>(EventPattern<ListChangedEventArgs> eventPattern, ReactiveList<T> targetReactiveList, bool includeItemChanges = false)
		{
			if (eventPattern == null) throw new ArgumentNullException(nameof(eventPattern));
			if (targetReactiveList == null) throw new ArgumentNullException(nameof(targetReactiveList));

			var senderAsBindingList = eventPattern.Sender as BindingList<T>;

			if (senderAsBindingList == null)
				throw new ArgumentOutOfRangeException("eventPattern");

			switch (eventPattern.EventArgs.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					{
						targetReactiveList.Add(senderAsBindingList[eventPattern.EventArgs.NewIndex]);
						break;
					}
				case ListChangedType.ItemChanged:
					{
						if (includeItemChanges)
						{
							var itemAtPosition = targetReactiveList[eventPattern.EventArgs.NewIndex];
							targetReactiveList[eventPattern.EventArgs.NewIndex] = itemAtPosition;
						}
						// ToDo: .. for now.. do nothing?
						break;
					}
				case ListChangedType.ItemMoved:
					{
						targetReactiveList.Move(eventPattern.EventArgs.OldIndex, eventPattern.EventArgs.NewIndex);
						break;
					}
				case ListChangedType.ItemDeleted:
					{
						var itemRemovedListChangedEventArgs = eventPattern.EventArgs as ItemRemovedListChangedEventArgs<T>;
						if (itemRemovedListChangedEventArgs == null)
							throw new ArgumentOutOfRangeException("eventPattern");

						targetReactiveList.Remove(itemRemovedListChangedEventArgs.Item);
						break;
					}

				case ListChangedType.Reset:
					{
						using (targetReactiveList.SuppressChangeNotifications())
						{
							targetReactiveList.Clear();
							targetReactiveList.AddRange(senderAsBindingList);
						}
						break;
					}
				default: // everything else..
					break;
			}
		}
	}
}