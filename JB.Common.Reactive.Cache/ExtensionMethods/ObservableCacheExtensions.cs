// -----------------------------------------------------------------------
// <copyright file="ObservableCacheExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JB.Collections;
using JB.Collections.Reactive;
using JB.Reactive.ExtensionMethods;

namespace JB.Reactive.Cache.ExtensionMethods
{
    /// <summary>
    ///     Extension methods for <see cref="IObservableCache{TKey,TValue}" /> instances.
    /// </summary>
    public static class ObservableCacheExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="key" /> with the given <paramref name="value" /> to the <paramref name="cache"/>
        /// with its expiry set to <see cref="TimeSpan.MaxValue"/> and <see cref="ObservableCacheExpirationType"/> to <see cref="ObservableCacheExpirationType.DoNothing"/>.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <param name="scheduler">Scheduler to perform the add action on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> Add<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return cache.Add(new KeyValuePair<TKey, TValue>(key, value), scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        /// Adds the specified <paramref name="key"/> with the given <paramref name="value"/> to the <paramref name="cache"/>.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire.</param>
        /// <param name="scheduler">Scheduler to perform the add action on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> Add<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return cache.Add(new KeyValuePair<TKey, TValue>(key, value), expiry, expirationType, scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        /// Adds the specified <paramref name="keyValuePairs" /> to the <paramref name="cache"/>
        /// with their expiry set to <see cref="TimeSpan.MaxValue" /> and <see cref="ObservableCacheExpirationType" /> to <see cref="ObservableCacheExpirationType.DoNothing" />.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key/value pairs to add.</param>
        /// <param name="scheduler">Scheduler to perform the addrange action on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        /// An observable stream that returns an <see cref="Unit" /> for every added element of the <paramref name="keyValuePairs"/>.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddRange<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (keyValuePairs == null)
                throw new ArgumentNullException(nameof(keyValuePairs));

            return cache.AddRange(keyValuePairs, TimeSpan.MaxValue, ObservableCacheExpirationType.DoNothing, scheduler);
        }

        /// <summary>
        /// Adds the specified <paramref name="keyValuePairs" /> to the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key/value pairs to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="keyValuePairs" />.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs" /> shall expire.</param>
        /// <param name="scheduler">Scheduler to perform the addrange action on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        /// An observable stream that returns an <see cref="Unit" /> for every added element of the <paramref name="keyValuePairs"/>.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddRange<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (keyValuePairs == null)
                throw new ArgumentNullException(nameof(keyValuePairs));

            var keyValuePairsAsList = keyValuePairs.ToList();
            if (keyValuePairsAsList.Count == 0)
                return Observable.Empty<KeyValuePair<TKey, TValue>>(scheduler ?? Scheduler.CurrentThread);

            return cache.AddRange(keyValuePairsAsList.AsObservable(scheduler ?? Scheduler.CurrentThread), expiry, expirationType, scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        ///     Adds the specified <paramref name="keyValuePair" /> to the <paramref name="cache" />
        ///     with its expiry set to <see cref="TimeSpan.MaxValue" /> and <see cref="ObservableCacheExpirationType" /> to <see cref="ObservableCacheExpirationType.DoNothing" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair to add.</param>
        /// <param name="scheduler">The scheduler to run the addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> Add<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            return cache.Add(keyValuePair,
                TimeSpan.MaxValue,
                ObservableCacheExpirationType.DoNothing,
                scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        ///     Adds the specified <paramref name="keyValuePair" /> to the <paramref name="cache" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair to add.</param>
        /// <param name="expiry">
        ///     The expiry of the <paramref name="keyValuePair" />. If none is provided the
        ///     <paramref name="keyValuePair" /> will virtually never expire.
        /// </param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> Add<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            return cache.Add(
                Observable.Return(keyValuePair, scheduler ?? Scheduler.CurrentThread),
                expiry,
                expirationType,
                scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        ///     Adds or, if it already exists, updates the <paramref name="key" /> with the given <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add or update.</param>
        /// <param name="expiry">
        ///     The expiry. If none is provided the <paramref name="key" />/<paramref name="value" /> pair will
        ///     virtually never expire.
        /// </param>
        /// <param name="expirationType">Defines how the <paramref name="key" />/<paramref name="value" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a <paramref name="value" /> for the given <paramref name="key" /> or, if it already exists, sorts out which
        ///     value to use using the <paramref name="valueSelector" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add or update.</param>
        /// <param name="valueSelector">
        ///     The value selector that gets provided the <paramref name="key" />, the existing value and
        ///     the new value (in that order) and is expected to return which value to use.
        /// </param>
        /// <param name="expiry">
        ///     The expiry. If none is provided the <paramref name="key" />/<paramref name="value" /> pair will
        ///     virtually never expire.
        /// </param>
        /// <param name="expirationType">Defines how the <paramref name="key" />/<paramref name="value" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds the <paramref name="keyValuePair" /> or, if it already exists, updates the existing key with the value of the
        ///     <paramref name="keyValuePair" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key value pair to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a <paramref name="keyValuePair" /> or, if the key already exists, sorts out which value to use utilizing the
        ///     <paramref name="valueSelector" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key value pair to add.</param>
        /// <param name="valueSelector">
        ///     The value selector that gets provided the key, the existing value and the new value (in
        ///     that order) and is expected to return which value to use.
        /// </param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a range of <paramref name="keyValuePairs" /> or, if the corresponding key already exists, updates it with the
        ///     given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key value pairs to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePairs" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a range of <paramref name="keyValuePairs" /> or, if the corresponding key already exists, updates it with the
        ///     given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key value pairs to add.</param>
        /// <param name="valueSelector">
        ///     The value selector that gets provided the key, the existing value and the new value (in
        ///     that order) and is expected to return which value to use.
        /// </param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePairs" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition or update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<KeyValuePair<TKey, TValue>> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all keys and values from the <paramref name="cache"/>.
        /// </summary>
        /// <param name="cache">The cache to clear.</param>
        /// <param name="scheduler">Scheduler to perform the clear action on.</param>
        /// <returns>
        /// An observable stream that signals when finished clear with an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> Clear<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            return cache.Clear(
                Observable.Return(Unit.Default, scheduler ?? Scheduler.CurrentThread),
                scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        ///     Determines whether which ones of the specified <paramref name="keys" /> are not contained in the
        ///     <paramref name="cache" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to check.</param>
        /// <param name="scheduler">The scheduler to run the check on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that returns the subset of keys of the provided <paramref name="keys" /> that are not
        ///     contained in the <paramref name="cache" />.
        /// </returns>
        public static IObservable<TKey> ContainsWhichNot<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the <paramref name="cache" />'s current count concatenated with future count changes as an observable stream.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache.</param>
        /// <param name="scheduler">The scheduler to observe count changes on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <value>
        ///     The count of keys in this instance.
        /// </value>
        public static IObservable<int> Count<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            return cache.CurrentCount.AsObservable(scheduler)
                .Concat(cache.CountChanges)
                .ObserveOn(scheduler ?? Scheduler.Default);
        }

        /// <summary>
        ///     Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{T}" />.</param>
        /// <param name="includeItemChanges">
        ///     if set to <c>true</c> individual items' changes will be propagated to the
        ///     <paramref name="target" />.
        /// </param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>
        ///     An <see cref="IDisposable" /> which will forward the changes to the <paramref name="target" /> as long as
        ///     <see cref="IDisposable.Dispose" /> hasn't been called.
        /// </returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(this IObservableCache<TKey, TValue> source,
                                                                           IEnhancedBindingList<TValue> target,
                                                                           bool includeItemChanges = false,
                                                                           IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            IObservable<IObservableCacheChange<TKey, TValue>> sourceObservable = scheduler != null
                ? source.Changes.ObserveOn(scheduler)
                : source.Changes;

            return sourceObservable.Subscribe(cacheChange =>
            {
                switch (cacheChange.ChangeType)
                {
                    case ObservableCacheChangeType.ItemAdded:
                    {
                        target.Add(cacheChange.Value);
                        break;
                    }
                    case ObservableCacheChangeType.ItemKeyChanged:
                    {
                        // nothing to do here
                        break;
                    }
                    case ObservableCacheChangeType.ItemValueChanged:
                    {
                        if (includeItemChanges)
                        {
                            // check whether target list contains the moved element at its expected index position
                            var targetIndex = target.IndexOf(cacheChange.Value);
                            if (targetIndex == -1)
                                return;

                            target.ResetItem(targetIndex);
                        }
                        break;
                    }
                    case ObservableCacheChangeType.ItemValueReplaced:
                    {
                        if (includeItemChanges)
                        {
                            if (target.Contains(cacheChange.OldValue))
                                target.Remove(cacheChange.OldValue);

                            var newValueTargetIndex = target.IndexOf(cacheChange.Value);
                            if (newValueTargetIndex != -1)
                                target.ResetItem(newValueTargetIndex);
                            else
                            {
                                target.Add(cacheChange.Value);
                            }
                        }
                        break;
                    }
                    case ObservableCacheChangeType.ItemRemoved:
                    {
                        // check whether target list contains the removed item, and delete if so
                        if (target.Contains(cacheChange.Value))
                        {
                            target.Remove(cacheChange.Value);
                        }
                        break;
                    }
                    case ObservableCacheChangeType.Reset:
                    {
                        var originalBindingRaiseListChangedEvents = target.RaiseListChangedEvents;
                        try
                        {
                            target.RaiseListChangedEvents = false;

                            ((ICollection<TValue>) target).Clear();
                            target.AddRange(source.CurrentValues);
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
                        throw new ArgumentOutOfRangeException(nameof(cacheChange),
                            $"Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemKeyChanged}, {ObservableDictionaryChangeType.ItemValueChanged}, {ObservableDictionaryChangeType.ItemValueReplaced}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.");
                }
            });
        }

        /// <summary>
        ///     Gets the value for the given <paramref name="key" /> or, if it doesn't exist in the <paramref name="cache" />,
        ///     calls the <paramref name="producer" /> to build the corresponding value, adds it to the <paramref name="cache" />
        ///     and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to get or produce and add.</param>
        /// <param name="producer">
        ///     The producer function that gets handed in the <paramref name="key" /> and is expected to return
        ///     a <typeparamref name="TValue" /> instance.
        /// </param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="key" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the retrieval or addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream containing the corresponding <typeparamref name="TValue" /> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, Func<TKey, TValue> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the value for the given <paramref name="keys" /> or, if they don't exist in the <paramref name="cache" />,
        ///     calls the <paramref name="producer" /> to build the corresponding value, adds it to the <paramref name="cache" />
        ///     and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to get or produce and add.</param>
        /// <param name="producer">
        ///     The producer function that gets handed in the <paramref name="keys" /> one by one and is
        ///     expected to return a <typeparamref name="TValue" /> instance per key.
        /// </param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keys" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keys" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the retrieval or addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream containing the corresponding <typeparamref name="TValue" /> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, Func<TKey, TValue> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the value for the given <paramref name="keys" /> or, if they don't exist in the <paramref name="cache" />,
        ///     calls the <paramref name="producer" /> to build the corresponding value, adds it to the <paramref name="cache" />
        ///     and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to get or produce and add.</param>
        /// <param name="producer">
        ///     The producer function that gets handed in the <paramref name="keys" /> in bulk and is expected
        ///     to return a corresponding .
        /// </param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keys" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keys" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the retrieval or addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream containing the corresponding <typeparamref name="TValue" /> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, Func<IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified <paramref name="key"/> from the <paramref name="cache" />.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to remove.</param>
        /// <param name="scheduler">Scheduler to perform the remove action on.</param>
        /// <returns>
        /// An observable stream that returns an observable stream of either [true] or [false] for every element provided by the <paramref name="source"/> observable
        /// and whether it was successfully found and removed.. or not.
        /// </returns>
        /// <remarks>
        /// The returned observable stream of [true] or [false] has the same order as the <paramref name="source"/> observable.
        /// </remarks>
        public static IObservable<bool> Remove<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            return cache.Remove(
                Observable.Return(key, scheduler ?? Scheduler.CurrentThread),
                scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        /// Removes the range of <paramref name="keys"/> from the <paramref name="cache" />.
        /// </summary>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to remove.</param>
        /// <param name="scheduler">Scheduler to perform the remove action on.</param>
        /// <returns>
        /// An observable stream that returns an observable stream of either [true] or [false] for every element provided by the <paramref name="source"/> observable
        /// and whether it was successfully found and removed.. or not.
        /// </returns>
        /// <remarks>
        /// The returned observable stream of [true] or [false] has the same order as the <paramref name="source"/> observable.
        /// </remarks>
        public static IObservable<bool> RemoveRange<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, IScheduler scheduler = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var keysAsList = keys.ToList();
            if (keysAsList.Count == 0)
                return Observable.Empty<bool>(scheduler ?? Scheduler.CurrentThread);

            return cache.RemoveRange(keysAsList.AsObservable(scheduler ?? Scheduler.CurrentThread), scheduler ?? Scheduler.CurrentThread);
        }

        /// <summary>
        ///     If the given <paramref name="key" /> does not exist in the <paramref name="cache" /> yet it will be added with the
        ///     given <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="expiry">
        ///     The expiry. If none is provided the <paramref name="key" />/<paramref name="value" /> pair will
        ///     virtually never expire.
        /// </param>
        /// <param name="expirationType">Defines how the <paramref name="key" />/<paramref name="value" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that returns [true] if successful, [false] if not.
        /// </returns>
        public static IObservable<bool> TryAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     If the given <paramref name="keyValuePair" />'s key does not exist in the <paramref name="cache" /> yet it will be
        ///     added with the given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <param name="scheduler">The scheduler to run the addition attempt on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that returns [true] if successful, [false] if not.
        /// </returns>
        public static IObservable<bool> TryAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Attempts to remove the <paramref name="key" /> from the <paramref name="cache" /> - if it exists in it.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to attempt to remove.</param>
        /// <param name="scheduler">The scheduler to run the removal attempt on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that returns [true] if the <paramref name="key" /> was in the <paramref name="cache" /> and
        ///     removed from it, [false] if not.
        /// </returns>
        public static IObservable<bool> TryRemove<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Attempts to update the <paramref name="key" /> with the <paramref name="value" /> in the <paramref name="cache" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to use.</param>
        /// <param name="value">The value to use.</param>
        /// <param name="expiry">The expiry. If none is provided the existing expiry will be left as-is.</param>
        /// <param name="expirationType">
        ///     Defines how the <paramref name="key" /> shall expire. If none is provided the existing
        ///     type will be left as-is.
        /// </param>
        /// <param name="scheduler">The scheduler to run the update attempt on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that returns [true] if the <paramref name="key" /> was in the <paramref name="cache" /> and
        ///     updated, [false] if not.
        /// </returns>
        public static IObservable<bool> TryUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType? expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Updates the key for the provided <paramref name="keyValuePair" /> with its value in the specified
        ///     <paramref name="cache" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair that contains the key to update and the value to update it with.</param>
        /// <param name="scheduler">The scheduler to run the update on. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns>
        ///     An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> Update<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, IScheduler scheduler = null)
        {
            throw new NotImplementedException();
        }
    }
}