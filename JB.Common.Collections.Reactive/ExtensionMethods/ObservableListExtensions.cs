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
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// target</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ObservableList<T> sourceObservableList,
            bool includeItemChanges = true,
            bool includeMoves = false,
            IScheduler scheduler = null,
            params IEnhancedBindingList<T>[] targetBindingLists)
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
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target binding list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="target" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// target</exception>
        /// <exception cref="System.InvalidOperationException">Source and Target Lists must contain exactly the same element(s) at
        /// the exact same index position(s)</exception>
        public static IDisposable ForwardListChangesTo<T>(this ObservableList<T> source,
            IEnhancedBindingList<T> target,
            bool includeItemChanges = true,
            bool includeMoves = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (includeMoves && (source.Except(target, EqualityComparer<T>.Default).Any()
                                 || target.Except(source, EqualityComparer<T>.Default).Any()
                                 || source.Any(element => source.IndexOf(element) != target.IndexOf(element))))
            {
                throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
            }

            IObservable<IObservableListChange<T>> sourceObservable = scheduler != null
                ? source.ListChanges.ObserveOn(scheduler)
                : source.ListChanges;

            return sourceObservable.Subscribe(observableListChange =>
            {
                switch (observableListChange.ChangeType)
                {
                    case ObservableListChangeType.ItemAdded:
                    {
                        target.Insert(observableListChange.Index, observableListChange.Item);
                        break;
                    }
                    case ObservableListChangeType.ItemChanged:
                    {
                        if (includeItemChanges)
                        {
                            // check whether target list contains the moved element at its expected index position
                            var targetIndex = target.IndexOf(observableListChange.Item);
                            if (targetIndex == -1)
                                return;

                            target.ResetItem(targetIndex);
                        }
                        break;
                    }
                    case ObservableListChangeType.ItemMoved:
                    {
                        if (includeMoves)
                        {
                            // check whether target list contains the moved element at its expected index position
                            if (target.IndexOf(observableListChange.Item) != observableListChange.OldIndex)
                            {
                                throw new InvalidOperationException($"{nameof(source)} and {nameof(target)} are no longer in sync: {nameof(target)} has a diffent item at index position {observableListChange.OldIndex} than expected.");
                            }

                            target.Move(observableListChange.Item, observableListChange.Index);
                        }
                        break;
                    }
                    case ObservableListChangeType.ItemRemoved:
                    {
                        // check whether target list contains the removed item, and delete if so
                        if (target.Contains(observableListChange.Item))
                        {
                            target.Remove(observableListChange.Item);
                        }
                        break;
                    }
                    case ObservableListChangeType.Reset:
                    {
                        var originalBindingRaiseListChangedEvents = target.RaiseListChangedEvents;
                        try
                        {
                            target.RaiseListChangedEvents = false;
                                ((ICollection<T>)target).Clear();
                            target.AddRange(source);
                        }
                        finally
                        {
                            target.RaiseListChangedEvents = originalBindingRaiseListChangedEvents;
                            if (originalBindingRaiseListChangedEvents)
                                target.ResetBindings();
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