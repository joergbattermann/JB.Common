// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionary{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable dictionary.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            this IObservableDictionary<TKey,TValue> source,
            IEnhancedBindingList<TValue> target,
            bool includeItemChanges = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sourceObservable = scheduler != null
                ? source.DictionaryChanges.ObserveOn(scheduler)
                : source.DictionaryChanges;

            return sourceObservable.ForwardDictionaryChangesTo(target, includeItemChanges);
        }

        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable dictionary.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="dictionaryChangesFilterPredicate">A filter function to test each <paramref name="source" /> change for whether or not to forward it to the <paramref name="target" />.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="addRangePredicateForResets">This filter predicate tests which elements of the source <see cref="IObservableDictionary{TKey,TValue}"/> to add
        /// whenever a <see cref="ObservableDictionaryChangeType.Reset"/> is received. A reset is forwarded by clearing the <paramref name="target"/> completely and re-filling it with
        /// the source's values, and this predicate determines which ones are added. If no filter predicate is provided, all source values will be re-added to the <paramref name="target"/>.</param>
        /// <param name="addDistinctValuesOnResetOnly">if set to <c>true</c> only distinct values will be re-added on <see cref="ObservableDictionaryChangeType.Reset" /> changes.</param>
        /// <param name="valueComparerForResets">The value equality comparer to use for reset changes and if <paramref name="valueComparerForResets"/> is set to [true]. If none is provided, the default one for the value type will be used</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            this IObservableDictionary<TKey, TValue> source,
            IEnhancedBindingList<TValue> target,
            Func<IObservableDictionaryChange<TKey, TValue>, bool> dictionaryChangesFilterPredicate,
            bool includeItemChanges = false,
            Func<KeyValuePair<TKey, TValue>, bool> addRangePredicateForResets = null,
            bool addDistinctValuesOnResetOnly = true,
            IEqualityComparer<TValue> valueComparerForResets = null,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sourceObservable = scheduler != null
                ? source.DictionaryChanges.ObserveOn(scheduler)
                : source.DictionaryChanges;

            if (dictionaryChangesFilterPredicate != null)
            {
                sourceObservable = sourceObservable.Where(dictionaryChangesFilterPredicate);
            }

            return sourceObservable.ForwardDictionaryChangesTo(target, includeItemChanges, addRangePredicateForResets, addDistinctValuesOnResetOnly, valueComparerForResets);
        }
    }
}
