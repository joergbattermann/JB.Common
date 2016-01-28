// -----------------------------------------------------------------------
// <copyright file="ObservableCacheExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JB.Collections;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache.ExtensionMethods
{
    /// <summary>
    ///     Extension methods for <see cref="IObservableCache{TKey,TValue}" /> instances.
    /// </summary>
    public static class ObservableCacheExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{T}"/>.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
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

                                ((ICollection<TValue>)target).Clear();
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
        /// Adds the specified <paramref name="keyValuePair" /> to the <paramref name="cache"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="keyValuePair" />. If none is provided the <paramref name="keyValuePair" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> Add<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds or, if it already exists, updates the <paramref name="key" /> with the given <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add or update.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="key"/>/<paramref name="value"/> pair will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="key"/>/<paramref name="value"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a <paramref name="value"/> for the given <paramref name="key"/> or, if it already exists, sorts out which value to use using the <paramref name="valueSelector"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add or update.</param>
        /// <param name="valueSelector">The value selector that gets provided the <see cref="key"/>, the existing value and the new value (in that order) and is expected to return which value to use.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="key"/>/<paramref name="value"/> pair will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="key"/>/<paramref name="value"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the <paramref name="keyValuePair"/> or, if it already exists, updates the existing key with the value of the <paramref name="keyValuePair"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key value pair to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a <paramref name="keyValuePair"/> or, if the key already exists, sorts out which value to use utilizing the <paramref name="valueSelector"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key value pair to add.</param>
        /// <param name="valueSelector">The value selector that gets provided the key, the existing value and the new value (in that order) and is expected to return which value to use.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a range of <paramref name="keyValuePairs"/> or, if the corresponding key already exists, updates it with the given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key value pairs to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePairs"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a range of <paramref name="keyValuePairs"/> or, if the corresponding key already exists, updates it with the given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePairs">The key value pairs to add.</param>
        /// <param name="valueSelector">The value selector that gets provided the key, the existing value and the new value (in that order) and is expected to return which value to use.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePairs"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs"/> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether which ones of the specified <paramref name="keys"/> are not contained in the <paramref name="cache"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to check.</param>
        /// <returns>
        /// An observable stream that returns the subset of keys of the provided <paramref name="keys"/> that are not contained in the <paramref name="cache"/>.
        /// </returns>
        public static IObservable<TKey> ContainsWhichNot<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value for the given <paramref name="key"/> or, if it doesn't exist in the <paramref name="cache"/>, calls the <paramref name="producer"/> to build the corresponding value, adds it to the <paramref name="cache"/> and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to get or produce and add.</param>
        /// <param name="producer">The producer function that gets handed in the <paramref name="key"/> and is expected to return a <typeparamref name="TValue"/> instance.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name=","/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name=","/> shall expire.</param>
        /// <returns>
        /// An observable stream containing the corresponding <typeparamref name="TValue"/> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, Func<TKey, TValue> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value for the given <paramref name="keys"/> or, if they don't exist in the <paramref name="cache"/>, calls the <paramref name="producer"/> to build the corresponding value, adds it to the <paramref name="cache"/> and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to get or produce and add.</param>
        /// <param name="producer">The producer function that gets handed in the <paramref name="keys"/> one by one and is expected to return a <typeparamref name="TValue"/> instance per key.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name=","/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name=","/> shall expire.</param>
        /// <returns>
        /// An observable stream containing the corresponding <typeparamref name="TValue"/> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, Func<TKey, TValue> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value for the given <paramref name="keys"/> or, if they don't exist in the <paramref name="cache"/>, calls the <paramref name="producer"/> to build the corresponding value, adds it to the <paramref name="cache"/> and returns it back to the caller.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keys">The keys to get or produce and add.</param>
        /// <param name="producer">The producer function that gets handed in the <paramref name="keys"/> in bulk and is expected to return a corresponding .</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name=","/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name=","/> shall expire.</param>
        /// <returns>
        /// An observable stream containing the corresponding <typeparamref name="TValue"/> instance(s).
        /// </returns>
        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, Func<IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>> producer, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the given <paramref name="key" /> does not exist in the <paramref name="cache"/> yet it will be added with the given <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value" />.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="key"/>/<paramref name="value"/> pair will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="key"/>/<paramref name="value"/> shall expire.</param>
        /// <returns>
        /// An observable stream that returns [true] if successful, [false] if not.
        /// </returns>
        public static IObservable<bool> TryAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the given <paramref name="keyValuePair" />'s key does not exist in the <paramref name="cache"/> yet it will be added with the given value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair to add.</param>
        /// <param name="expiry">The expiry. If none is provided the <paramref name="keyValuePair" /> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePair" /> shall expire.</param>
        /// <returns>
        /// An observable stream that returns [true] if successful, [false] if not.
        /// </returns>
        public static IObservable<bool> TryAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to remove the <paramref name="key"/> from the <see cref="cache"/> - if it exists in it.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to attempt to remove.</param>
        /// <returns>
        /// An observable stream that returns [true] if the <paramref name="key"/> was in the <paramref name="cache"/> and removed from it, [false] if not.
        /// </returns>
        public static IObservable<bool> TryRemove<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to update the <paramref name="key"/> with the <paramref name="value"/> in the <paramref name="cache"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key to use.</param>
        /// <param name="value">The value to use.</param>
        /// <param name="expiry">The expiry. If none is provided the existing expiry will be left as-is.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire. If none is provided the existing type will be left as-is.</param>
        /// <returns>
        /// An observable stream that returns [true] if the <paramref name="key"/> was in the <paramref name="cache"/> and updated, [false] if not.
        /// </returns>
        public static IObservable<bool> TryUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType? expirationType = ObservableCacheExpirationType.Remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the key for the provided <paramref name="keyValuePair"/> with its value in the specified <paramref name="cache"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="keyValuePair">The key/value pair that contains the key to update and the value to update it with.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public static IObservable<Unit> Update<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair)
        {
            throw new NotImplementedException();
        }
    }
}