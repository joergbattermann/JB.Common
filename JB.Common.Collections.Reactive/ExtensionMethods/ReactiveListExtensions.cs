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
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    ///     Provides Extension Method(s) for <see cref="ReactiveList{T}" /> instances.
    /// </summary>
    public static class ReactiveListExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="sourceReactiveList" /> changes to the <paramref name="targetBindingLists" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceReactiveList">The source reactive list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="targetBindingLists" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetBindingLists" />.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="targetBindingLists">The target binding lists.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">sourceReactiveList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ReactiveList<T> sourceReactiveList, bool includeItemChanges = false, bool includeMoves = false, IScheduler scheduler = null, params EnhancedBindingList<T>[] targetBindingLists)
        {
            if (sourceReactiveList == null) throw new ArgumentNullException(nameof(sourceReactiveList));
            if (targetBindingLists == null) throw new ArgumentNullException(nameof(targetBindingLists));

            if (targetBindingLists.Length <= 0) throw new ArgumentOutOfRangeException(nameof(targetBindingLists));

            return new CompositeDisposable(targetBindingLists.Select(targetBindingList => sourceReactiveList.ForwardListChangesTo(targetBindingList, includeItemChanges, includeMoves, scheduler)));
        }

        /// <summary>
        /// Forwards the <paramref name="sourceReactiveList" /> changes to the <paramref name="targetBindingList" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceReactiveList">The source reactive list.</param>
        /// <param name="targetBindingList">The target binding list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="targetBindingList" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="targetBindingList" />.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">sourceReactiveList
        /// or
        /// targetBindingList</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ReactiveList<T> sourceReactiveList, EnhancedBindingList<T> targetBindingList, bool includeItemChanges = false, bool includeMoves = false, IScheduler scheduler = null)
        {
            if (sourceReactiveList == null) throw new ArgumentNullException(nameof(sourceReactiveList));
            if (targetBindingList == null) throw new ArgumentNullException(nameof(targetBindingList));

            if (includeMoves && (sourceReactiveList.Except(targetBindingList, EqualityComparer<T>.Default).Any()
                                 || targetBindingList.Except(sourceReactiveList, EqualityComparer<T>.Default).Any()
                                 || sourceReactiveList.Any(element => sourceReactiveList.IndexOf(element) != targetBindingList.IndexOf(element))))
            {
                throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
            }

            IObservable<IReactiveCollectionChange<T>> sourceObservable = scheduler != null
                ? sourceReactiveList.ItemChanges.ObserveOn(scheduler)
                : sourceReactiveList.ItemChanges;

            return sourceObservable.Subscribe(reactiveCollectionChange =>
            {
                switch (reactiveCollectionChange.ChangeType)
                {
                    case ReactiveCollectionChangeType.ItemAdded:
                    {
                        targetBindingList.Insert(reactiveCollectionChange.Index, reactiveCollectionChange.Item);
                        break;
                    }
                    case ReactiveCollectionChangeType.ItemChanged:
                    {
                        if (includeItemChanges)
                        {
                            // check whether target list contains the moved element at its expected index position
                            if (targetBindingList.IndexOf(reactiveCollectionChange.Item) != reactiveCollectionChange.Index)
                            {
                                throw new InvalidOperationException($"{nameof(sourceReactiveList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {reactiveCollectionChange.Index} than expected.");
                            }

                            targetBindingList.ResetItem(reactiveCollectionChange.Index);
                        }
                        break;
                    }
                    case ReactiveCollectionChangeType.ItemMoved:
                    {
                        if (includeMoves)
                        {
                            // check whether target list contains the moved element at its expected index position
                            if (targetBindingList.IndexOf(reactiveCollectionChange.Item) != reactiveCollectionChange.OldIndex)
                            {
                                throw new InvalidOperationException($"{nameof(sourceReactiveList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {reactiveCollectionChange.OldIndex} than expected.");
                            }

                            targetBindingList.Move(reactiveCollectionChange.Item, reactiveCollectionChange.Index);
                        }
                        break;
                    }
                    case ReactiveCollectionChangeType.ItemRemoved:
                    {
                        // check whether target list contains the moved element at its expected index position
                        if (targetBindingList.IndexOf(reactiveCollectionChange.Item) != reactiveCollectionChange.OldIndex)
                        {
                            throw new InvalidOperationException($"{nameof(sourceReactiveList)} and {nameof(targetBindingList)} are no longer in sync: {nameof(targetBindingList)} has a diffent item at index position {reactiveCollectionChange.OldIndex} than expected.");
                        }

                        targetBindingList.RemoveAt(reactiveCollectionChange.OldIndex);
                        break;
                    }
                    case ReactiveCollectionChangeType.Reset:
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