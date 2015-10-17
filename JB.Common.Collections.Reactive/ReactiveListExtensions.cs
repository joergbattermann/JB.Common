// -----------------------------------------------------------------------
// <copyright file="ReactiveListExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using ReactiveUI;

namespace JB.Common.Collections
{
	/// <summary>
	///     Provides Extension Method(s) for <see cref="ReactiveList{T}" /> instances.
	/// </summary>
	public static class ReactiveListExtensions
	{
		/// <summary>
		///     Forwards the <paramref name="sourceReactiveList" /> changes to the <paramref name="targetBindingLists" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceReactiveList">The source reactive list.</param>
		/// <param name="targetBindingLists">The target binding lists.</param>
		/// <param name="includeItemChanges">
		///     if set to <c>true</c> individual items' changes will be propagated to the
		///     <paramref name="targetBindingLists" /> via replacing the item completely.
		/// </param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     sourceReactiveList
		///     or
		///     targetBindingList
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		///     Source and Target Lists must contain exactly the same element(s) at
		///     the exact same index position(s)
		/// </exception>
		public static IDisposable ForwardListChangesTo<T>(this ReactiveList<T> sourceReactiveList, bool includeItemChanges = false, params EnhancedBindingList<T>[] targetBindingLists)
		{
			if (sourceReactiveList == null) throw new ArgumentNullException("sourceReactiveList");
			if (targetBindingLists == null) throw new ArgumentNullException("targetBindingLists");

			if (targetBindingLists.Length <= 0) throw new ArgumentOutOfRangeException("targetBindingLists");

			return new CompositeDisposable(targetBindingLists.Select(targetBindingList => sourceReactiveList.ForwardListChangesTo(targetBindingList, includeItemChanges)));
		}

		/// <summary>
		///     Forwards the <paramref name="sourceReactiveList" /> changes to the <paramref name="targetBindingList" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceReactiveList">The source reactive list.</param>
		/// <param name="targetBindingList">The target binding list.</param>
		/// <param name="includeItemChanges">
		///     if set to <c>true</c> individual items' changes will be propagated to the
		///     <paramref name="targetBindingList" /> via replacing the item completely.
		/// </param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     sourceReactiveList
		///     or
		///     targetBindingList
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		///     Source and Target Lists must contain exactly the same element(s) at
		///     the exact same index position(s)
		/// </exception>
		public static IDisposable ForwardListChangesTo<T>(this ReactiveList<T> sourceReactiveList, EnhancedBindingList<T> targetBindingList, bool includeItemChanges = false)
		{
			if (sourceReactiveList == null) throw new ArgumentNullException("sourceReactiveList");
			if (targetBindingList == null) throw new ArgumentNullException("targetBindingList");

			if (sourceReactiveList.Except(targetBindingList, EqualityComparer<T>.Default).Any()
			    || targetBindingList.Except(sourceReactiveList, EqualityComparer<T>.Default).Any()
			    || sourceReactiveList.Any(element => sourceReactiveList.IndexOf(element) != targetBindingList.IndexOf(element)))
			{
				throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
			}

			var itemsAddedSubscription = sourceReactiveList.ItemsAdded.Subscribe(item => targetBindingList.Add(item));
			var itemsRemovedSubscription = sourceReactiveList.ItemsRemoved.Subscribe(item => targetBindingList.Remove(item));
			var itemsChangedSubscription = sourceReactiveList.ItemChanged.Subscribe(reactivePropertyChangedEventArgs =>
			{
				if (includeItemChanges)
				{
					var originalRaiseListChangedEvents = targetBindingList.RaiseListChangedEvents;
					var changedItemPosition = -1;
					try
					{
						changedItemPosition = targetBindingList.IndexOf(reactivePropertyChangedEventArgs.Sender);
						targetBindingList.RaiseListChangedEvents = false;
						targetBindingList[changedItemPosition] = reactivePropertyChangedEventArgs.Sender;
					}
					finally
					{
						targetBindingList.RaiseListChangedEvents = originalRaiseListChangedEvents;

						if (originalRaiseListChangedEvents)
						{
							targetBindingList.ResetItem(changedItemPosition);
						}
					}
				}
			});
			var itemsMovedSubscription = sourceReactiveList.ItemsMoved.Subscribe(moveInfo =>
			{
				var moveTargetindex = moveInfo.To;
				var currentTargetIndex = moveTargetindex;

				foreach (var movedItem in moveInfo.MovedItems)
				{
					targetBindingList.Move(movedItem, currentTargetIndex);
					currentTargetIndex++;
				}
			});

			var listResetSubscription = sourceReactiveList.ShouldReset.Subscribe(_ =>
			{
				var originalBindingRaiseListChangedEvents = targetBindingList.RaiseListChangedEvents;
				try
				{
					targetBindingList.RaiseListChangedEvents = false;
					targetBindingList.Clear();
					targetBindingList.AddRange(sourceReactiveList);
				}
				finally
				{
					targetBindingList.RaiseListChangedEvents = originalBindingRaiseListChangedEvents;
					targetBindingList.ResetBindings();
				}
			});

			return new CompositeDisposable(itemsAddedSubscription, itemsChangedSubscription, itemsMovedSubscription, itemsRemovedSubscription, listResetSubscription);
		}
	}
}