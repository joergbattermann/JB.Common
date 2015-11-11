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
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace JB.Collections.ExtensionMethods
{
	/// <summary>
	///     Provides Extension Method(s) for <see cref="EnhancedBindingList{T}" /> instances.
	/// </summary>
	public static class EnhancedBindingListExtensions
	{
        /// <summary>
        /// Forwards the <paramref name="sourceBindingList" /> changes to the <paramref name="targetReactiveLists" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBindingList">The source binding list.</param>
        /// <param name="targetReactiveLists">The target reactive lists.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="targetReactiveLists" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetReactiveLists"/>.</param>
        /// <exception cref="System.ArgumentNullException">sourceReactiveList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this EnhancedBindingList<T> sourceBindingList, bool includeItemChanges = false, bool includeMoves = false, params ReactiveList<T>[] targetReactiveLists)
		{
			if (sourceBindingList == null) throw new ArgumentNullException(nameof(sourceBindingList));
			if (targetReactiveLists == null) throw new ArgumentNullException(nameof(targetReactiveLists));

			if (targetReactiveLists.Length <= 0) throw new ArgumentOutOfRangeException(nameof(targetReactiveLists));

			return new CompositeDisposable(targetReactiveLists.Select(targetBindingList => sourceBindingList.ForwardListChangesTo(targetBindingList, includeItemChanges, includeMoves)));
		}

        /// <summary>
        /// Forwards the <paramref name="sourceBindingList" /> changes to the <paramref name="targetReactiveList" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBindingList">The source binding list.</param>
        /// <param name="targetReactiveList">The target reactive list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="targetReactiveList" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetReactiveList"/>.</param>
        /// <exception cref="System.ArgumentNullException">sourceReactiveList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s) when using <paramref name="includeMoves"/>.</exception>
        public static IDisposable ForwardListChangesTo<T>(this EnhancedBindingList<T> sourceBindingList, ReactiveList<T> targetReactiveList, bool includeItemChanges = false, bool includeMoves = false)
		{
			if (sourceBindingList == null) throw new ArgumentNullException(nameof(sourceBindingList));
			if (targetReactiveList == null) throw new ArgumentNullException(nameof(targetReactiveList));

			if (includeMoves && (sourceBindingList.Except(targetReactiveList, EqualityComparer<T>.Default).Any()
				|| targetReactiveList.Except(sourceBindingList, EqualityComparer<T>.Default).Any()
				|| sourceBindingList.Any(element => sourceBindingList.IndexOf(element) != targetReactiveList.IndexOf(element))))
			{
				throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
			}

			return Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
				handler => sourceBindingList.ListChanged += handler,
				handler => sourceBindingList.ListChanged -= handler)
				.Subscribe(eventPattern => OnNextListChanged(eventPattern, targetReactiveList, includeItemChanges, includeMoves));
		}

        /// <summary>
        /// Handler for List Changed Events
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventPattern">The event pattern.</param>
        /// <param name="targetReactiveList">The target reactive list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> includes item changes.</param>
        /// <param name="includeMoves">if set to <c>true</c> includes move operations.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">eventPattern
        /// or
        /// eventPattern</exception>
        private static void OnNextListChanged<T>(EventPattern<ListChangedEventArgs> eventPattern, ReactiveList<T> targetReactiveList, bool includeItemChanges = false, bool includeMoves = false)
		{
			if (eventPattern == null) throw new ArgumentNullException(nameof(eventPattern));
			if (targetReactiveList == null) throw new ArgumentNullException(nameof(targetReactiveList));

			var senderAsBindingList = eventPattern.Sender as BindingList<T>;

			if (senderAsBindingList == null)
				throw new ArgumentOutOfRangeException(nameof(eventPattern));

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
					    if (includeMoves)
					    {
					        targetReactiveList.Move(eventPattern.EventArgs.OldIndex, eventPattern.EventArgs.NewIndex);
					    }
						break;
					}
				case ListChangedType.ItemDeleted:
					{
						var itemRemovedListChangedEventArgs = eventPattern.EventArgs as ItemDeletedListChangedEventArgs<T>;
						if (itemRemovedListChangedEventArgs == null)
							throw new ArgumentOutOfRangeException(nameof(eventPattern));

						targetReactiveList.Remove(itemRemovedListChangedEventArgs.Item);
						break;
					}

				case ListChangedType.Reset:
					{
						using (targetReactiveList.SuppressCollectionChangedNotifications(true))
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