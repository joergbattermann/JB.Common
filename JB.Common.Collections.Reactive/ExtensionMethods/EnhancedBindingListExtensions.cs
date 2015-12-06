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

namespace JB.Collections.Reactive.ExtensionMethods
{
	/// <summary>
	///     Provides Extension Method(s) for <see cref="EnhancedBindingList{T}" /> instances.
	/// </summary>
	public static class EnhancedBindingListExtensions
	{
        /// <summary>
        /// Forwards the <paramref name="sourceBindingList" /> changes to the <paramref name="targetObservableLists" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBindingList">The source binding list.</param>
        /// <param name="targetObservableLists">The target observable lists.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="targetObservableLists" />
        /// via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetObservableLists"/>.</param>
        /// <exception cref="System.ArgumentNullException">sourceBindingList
        /// or
        /// targetObservableLists</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this EnhancedBindingList<T> sourceBindingList,
            bool includeItemChanges = false,
            bool includeMoves = false,
            params ObservableList<T>[] targetObservableLists)
		{
			if (sourceBindingList == null) throw new ArgumentNullException(nameof(sourceBindingList));
			if (targetObservableLists == null) throw new ArgumentNullException(nameof(targetObservableLists));

			if (targetObservableLists.Length <= 0) throw new ArgumentOutOfRangeException(nameof(targetObservableLists));

			return new CompositeDisposable(targetObservableLists.Select(targetBindingList => sourceBindingList.ForwardListChangesTo(targetBindingList, includeItemChanges, includeMoves)));
		}

        /// <summary>
        /// Forwards the <paramref name="sourceBindingList" /> changes to the <paramref name="targetObservableList" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBindingList">The source binding list.</param>
        /// <param name="targetObservableList">The target observable list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="targetObservableList" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetObservableList"/>.</param>
        /// <exception cref="System.ArgumentNullException">sourceBindingList
        /// or
        /// targetObservableList</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s) when using <paramref name="includeMoves"/>.</exception>
        public static IDisposable ForwardListChangesTo<T>(this EnhancedBindingList<T> sourceBindingList, ObservableList<T> targetObservableList, bool includeItemChanges = false, bool includeMoves = false)
		{
			if (sourceBindingList == null) throw new ArgumentNullException(nameof(sourceBindingList));
			if (targetObservableList == null) throw new ArgumentNullException(nameof(targetObservableList));

			if (includeMoves && (sourceBindingList.Except(targetObservableList, EqualityComparer<T>.Default).Any()
				|| targetObservableList.Except(sourceBindingList, EqualityComparer<T>.Default).Any()
				|| sourceBindingList.Any(element => sourceBindingList.IndexOf(element) != targetObservableList.IndexOf(element))))
			{
				throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
			}

			return Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
				handler => sourceBindingList.ListChanged += handler,
				handler => sourceBindingList.ListChanged -= handler)
				.Subscribe(eventPattern => OnNextListChanged(eventPattern, targetObservableList, includeItemChanges, includeMoves));
		}

        /// <summary>
        /// Handler for List Changed Events
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventPattern">The event pattern.</param>
        /// <param name="targetObservableList">The target observable list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> includes item changes.</param>
        /// <param name="includeMoves">if set to <c>true</c> includes move operations.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">eventPattern
        /// or
        /// eventPattern</exception>
        private static void OnNextListChanged<T>(EventPattern<ListChangedEventArgs> eventPattern, ObservableList<T> targetObservableList, bool includeItemChanges = false, bool includeMoves = false)
		{
			if (eventPattern == null) throw new ArgumentNullException(nameof(eventPattern));
			if (targetObservableList == null) throw new ArgumentNullException(nameof(targetObservableList));

			var senderAsBindingList = eventPattern.Sender as BindingList<T>;

			if (senderAsBindingList == null)
				throw new ArgumentOutOfRangeException(nameof(eventPattern));

			switch (eventPattern.EventArgs.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					{
						targetObservableList.Add(senderAsBindingList[eventPattern.EventArgs.NewIndex]);
						break;
					}
				case ListChangedType.ItemChanged:
					{
						if (includeItemChanges)
						{
							var itemAtPosition = targetObservableList[eventPattern.EventArgs.NewIndex];
							targetObservableList[eventPattern.EventArgs.NewIndex] = itemAtPosition;
						}
						// ToDo: .. for now.. do nothing?
						break;
					}
				case ListChangedType.ItemMoved:
					{
					    if (includeMoves)
					    {
					        targetObservableList.Move(eventPattern.EventArgs.OldIndex, eventPattern.EventArgs.NewIndex);
					    }
						break;
					}
				case ListChangedType.ItemDeleted:
					{
						var itemRemovedListChangedEventArgs = eventPattern.EventArgs as ItemDeletedListChangedEventArgs<T>;
						if (itemRemovedListChangedEventArgs == null)
							throw new ArgumentOutOfRangeException(nameof(eventPattern));

						targetObservableList.Remove(itemRemovedListChangedEventArgs.Item);
						break;
					}

				case ListChangedType.Reset:
					{
						using (targetObservableList.SuppressChangeNotifications(true))
						{
							targetObservableList.Clear();
							targetObservableList.AddRange(senderAsBindingList);
						}
						break;
					}
				default: // everything else..
					break;
			}
		}
	}
}