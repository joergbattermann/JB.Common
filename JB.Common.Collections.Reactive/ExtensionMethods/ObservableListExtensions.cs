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
using System.Reactive.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    ///     Provides Extension Method(s) for <see cref="ObservableList{T}" /> instances.
    /// </summary>
    public static class ObservableListExtensions
    {
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
        public static IDisposable ForwardListChangesTo<T>(
            this IObservableList<T> source,
            IEnhancedBindingList<T> target,
            bool includeItemChanges = true,
            bool includeMoves = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (includeMoves && (source.Except(target).Any()
                                 || target.Except(source).Any()
                                 || source.Any(element => source.IndexOf(element) != target.IndexOf(element))))
            {
                throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
            }

            IObservable<IObservableListChange<T>> sourceObservable = scheduler != null
                ? source.ListChanges.ObserveOn(scheduler)
                : source.ListChanges;

            return ForwardListChangesTo(sourceObservable, target, includeItemChanges, includeMoves);
        }

        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="T">The type of element in the lists</typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target binding list.</param>
        /// <param name="filterPredicate">A filter function to test each <paramref name="source" /> change for whether or not to forward it to the <paramref name="target" />.</param>
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
        public static IDisposable ForwardListChangesTo<T>(
            this IObservableList<T> source,
            IEnhancedBindingList<T> target,
            Func<IObservableListChange<T>, bool> filterPredicate,
            bool includeItemChanges = true,
            bool includeMoves = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (includeMoves && (source.Except(target).Any()
                                 || target.Except(source).Any()
                                 || source.Any(element => source.IndexOf(element) != target.IndexOf(element))))
            {
                throw new InvalidOperationException("Source and Target Lists must contain exactly the same element(s) at the exact same index position(s)");
            }

            var sourceObservable = scheduler != null
                ? source.ListChanges.ObserveOn(scheduler)
                : source.ListChanges;

            if (filterPredicate != null)
            {
                sourceObservable = sourceObservable.Where(filterPredicate);
            }

            return ForwardListChangesTo(sourceObservable, target, includeItemChanges, includeMoves);
        }

        /// <summary>
        /// Forwards the <paramref name="sourceObservable" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="T">The type of the list item(s)</typeparam>
        /// <param name="sourceObservable">The source observable.</param>
        /// <param name="target">The target binding list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="target" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="target" />.</param>
        /// <returns></returns>
        private static IDisposable ForwardListChangesTo<T>(
            IObservable<IObservableListChange<T>> sourceObservable,
            IEnhancedBindingList<T> target,
            bool includeItemChanges = true,
            bool includeMoves = false)
        {
            if (sourceObservable == null)
                throw new ArgumentNullException(nameof(sourceObservable));

            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            return sourceObservable.Subscribe(observableListChange =>
            {
                switch (observableListChange.ChangeType)
                {
                    case ObservableListChangeType.ItemAdded:
                        {
                            if (includeMoves)
                                target.Insert(observableListChange.Index, observableListChange.Item);
                            else
                                target.Add(observableListChange.Item);
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
                                    throw new InvalidOperationException($"The source and and target lists are no longer in sync: target has a diffent item at index position {observableListChange.OldIndex} than expected.");
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
                                target.AddRange(observableListChange.List);
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