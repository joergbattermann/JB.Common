// -----------------------------------------------------------------------
// <copyright file="ObservableListExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
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

            return sourceObservable.ForwardListChangesTo(target, includeItemChanges, includeMoves);
        }

        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="T">The type of element in the lists</typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target binding list.</param>
        /// <param name="filterPredicate">A filter function to test each <paramref name="source" /> change for whether or not to forward it to the <paramref name="target" />.</param>
        /// <param name="addRangePredicateForResets">This filter predicate tests which elements of the source <see cref="IObservableListChange{T}"/> to add
        /// whenever a <see cref="ObservableListChangeType.Reset"/> is received. A reset is forwarded by clearing the <paramref name="target"/> completely and re-filling it with
        /// the source's values, and this predicate determines which ones are added. If no filter predicate is provided, all source values will be re-added to the <paramref name="target"/>.</param>
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
            Func<T, bool> addRangePredicateForResets = null,
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

            return sourceObservable.ForwardListChangesTo(target, includeItemChanges, includeMoves);
        }
    }
}