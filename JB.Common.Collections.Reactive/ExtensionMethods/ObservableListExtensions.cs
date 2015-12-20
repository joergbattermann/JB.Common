// -----------------------------------------------------------------------
// <copyright file="ObservableListExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    ///     Provides Extension Method(s) for <see cref="ObservableList{T}" /> instances.
    /// </summary>
    public static class ObservableListExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="sourceObservableList" /> changes to the <paramref name="targetBindingLists" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObservableList">The source observable list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="targetBindingLists" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetBindingLists" />.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="targetBindingLists">The target binding lists.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">sourceObservableList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ObservableList<T> sourceObservableList,
            bool includeItemChanges = false,
            bool includeMoves = false,
            IScheduler scheduler = null,
            params EnhancedBindingList<T>[] targetBindingLists)
        {
            if (sourceObservableList == null)
                throw new ArgumentNullException(nameof(sourceObservableList));

            if (targetBindingLists == null)
                throw new ArgumentNullException(nameof(targetBindingLists));

            if (targetBindingLists.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetBindingLists));

            return new CompositeDisposable(targetBindingLists.Select(targetBindingList => sourceObservableList.ForwardListChangesTo(targetBindingList, includeItemChanges, includeMoves, scheduler)));
        }

        /// <summary>
        /// Forwards the <paramref name="sourceObservableList" /> changes to the <paramref name="targetBindingList" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObservableList">The source observable list.</param>
        /// <param name="targetBindingList">The target binding list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="targetBindingList" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetBindingList" />.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">sourceObservableList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ObservableList<T> sourceObservableList,
            EnhancedBindingList<T> targetBindingList,
            bool includeItemChanges = false,
            bool includeMoves = false,
            IScheduler scheduler = null)
        {
            if (sourceObservableList == null)
                throw new ArgumentNullException(nameof(sourceObservableList));

            if (targetBindingList == null)
                throw new ArgumentNullException(nameof(targetBindingList));

            if (includeMoves && (sourceObservableList.Except(targetBindingList, EqualityComparer<T>.Default).Any()
                                 || targetBindingList.Except(sourceObservableList, EqualityComparer<T>.Default).Any()
                                 || sourceObservableList.Any(element => sourceObservableList.IndexOf(element) != targetBindingList.IndexOf(element))))
            {
                throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
            }

            IObservable<IObservableListChange<T>> sourceObservable = scheduler != null
                ? sourceObservableList.ListChanges.ObserveOn(scheduler)
                : sourceObservableList.ListChanges;

            return sourceObservable.Subscribe(observableListChange =>
            {
                switch (observableListChange.ChangeType)
                {
                    case ObservableListChangeType.ItemAdded:
                    {
                        targetBindingList.Insert(observableListChange.Index, observableListChange.Item);
                        break;
                    }
                    case ObservableListChangeType.ItemChanged:
                    {
                        if (includeItemChanges)
                        {
                            // check whether target list contains the moved element at its expected index position
                            if (targetBindingList.IndexOf(observableListChange.Item) != observableListChange.Index)
                            {
                                throw new InvalidOperationException($"{nameof(sourceObservableList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {observableListChange.Index} than expected.");
                            }

                            targetBindingList.ResetItem(observableListChange.Index);
                        }
                        break;
                    }
                    case ObservableListChangeType.ItemMoved:
                    {
                        if (includeMoves)
                        {
                            // check whether target list contains the moved element at its expected index position
                            if (targetBindingList.IndexOf(observableListChange.Item) != observableListChange.OldIndex)
                            {
                                throw new InvalidOperationException($"{nameof(sourceObservableList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {observableListChange.OldIndex} than expected.");
                            }

                            targetBindingList.Move(observableListChange.Item, observableListChange.Index);
                        }
                        break;
                    }
                    case ObservableListChangeType.ItemRemoved:
                    {
                        // check whether target list contains the moved element at its expected index position
                        if (targetBindingList.IndexOf(observableListChange.Item) != observableListChange.OldIndex)
                        {
                            throw new InvalidOperationException($"{nameof(sourceObservableList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {observableListChange.OldIndex} than expected.");
                        }

                        targetBindingList.RemoveAt(observableListChange.OldIndex);
                        break;
                    }
                    case ObservableListChangeType.Reset:
                    {
                        var originalBindingRaiseListChangedEvents = targetBindingList.RaiseListChangedEvents;
                        try
                        {
                            targetBindingList.RaiseListChangedEvents = false;
                            targetBindingList.Clear();
                            targetBindingList.AddRange(sourceObservableList);
                        }
                        finally
                        {
                            targetBindingList.RaiseListChangedEvents = originalBindingRaiseListChangedEvents;
                            if (originalBindingRaiseListChangedEvents)
                                targetBindingList.ResetBindings();
                        }

                        break;
                    }
                    default:
                        break;
                }
            });
        }
    }
}