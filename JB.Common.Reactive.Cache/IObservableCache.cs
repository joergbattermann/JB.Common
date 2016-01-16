// -----------------------------------------------------------------------
// <copyright file="IObservableCache.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache
{
    public interface IObservableCache<TKey, TValue> :
        INotifyObservableCacheChanges<TKey, TValue>,
        INotifyObservableCacheItemChanges<TKey, TValue>,
        INotifyObservableResets,
        INotifyUnhandledObserverExceptions
    {
        /// <summary>
        /// Gets the count of keys in this instance.
        /// </summary>
        /// <value>
        /// The count of keys in this instance.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Adds the specified <paramref name="key"/> with the given <paramref name="value"/> to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>. If none is provided the <paramref name="key"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Add(TKey key, TValue value, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove);

        /// <summary>
        /// Adds the specified <paramref name="keyValuePairs"/> to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="keyValuePairs"/>. If none is provided the <paramref name="keyValuePairs"/> will virtually never expire.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Clear();

        /// <summary>
        /// Determines whether this instance contains the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns [true] if the <paramref name="key"/> is is contained in this instance, [false] if not.
        /// </returns>
        IObservable<bool> Contains(TKey key);

        /// <summary>
        /// Determines whether this instance contains the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent <see cref="Contains"/> checks.</param>
        /// <param name="scheduler">Scheduler to run the concurrent <see cref="Contains"/> checks on.</param>
        /// <returns>
        /// An observable stream that returns [true] if all <paramref name="keys"/> are contained in this instance, [false] if not.
        /// </returns>
        IObservable<bool> ContainsAll(ICollection<TKey> keys, int maxConcurrent = 1, IScheduler scheduler = null);

        /// <summary>
        /// Determines whether which ones of the specified <paramref name="keys"/> are contained in this instance.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <returns>
        /// An observable stream that returns the subset of keys of the provided <paramref name="keys"/> that are contained in this instance.
        /// </returns>
        IObservable<TKey> ContainsWhich(IEnumerable<TKey> keys);

        /// <summary>
        /// Determines the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </returns>
        IObservable<DateTime> ExpiresWhen(TKey key);

        /// <summary>
        /// Determines the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </returns>
        IObservable<TimeSpan> ExpiresIn(TKey key);

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to retrieve the <typeparamref name="TValue"/> for.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TValue"/> for the provided <paramref name="key"/>.
        /// </returns>
        IObservable<TValue> Get(TKey key);

        /// <summary>
        /// Gets the values for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to retrieve the values for.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent retrievals.</param>
        /// <param name="scheduler">Scheduler to run the concurrent retrievals on.</param>
        /// <returns>
        /// An observable stream that returns the values for the provided <paramref name="keys"/>.
        /// </returns>
        IObservable<TValue> Get(IEnumerable<TKey> keys, int maxConcurrent = 1, IScheduler scheduler = null);

        /// <summary>
        /// Removes the specified <paramref name="key"/> from this instance.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Remove(TKey key);

        /// <summary>
        /// Removes the specified <paramref name="keys"/> from this instance.
        /// </summary>
        /// <param name="keys">The keys to remove.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> RemoveRange(IEnumerable<TKey> keys);

        /// <summary>
        /// Updates the specified <paramref name="key"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The value to update the <paramref name="key"/> with.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Update(TKey key, TValue value);

        /// <summary>
        /// Updates a range of <paramref name="keyValuePairs"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs that each contain the key to update and the value to update it with.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Update(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs);

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> UpdateExpiration(TKey key, TimeSpan expiry, ObservableCacheExpirationType expirationType);

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="keys"/>.</param>
        /// <param name="expirationType">Defines how the <paramref name="keys" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> UpdateExpiration(IEnumerable<TKey> keys, TimeSpan expiry, ObservableCacheExpirationType expirationType);
    }
}