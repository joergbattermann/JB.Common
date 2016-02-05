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
        INotifyObservableCacheItemExpirations<TKey, TValue>,
        INotifyObservableCountChanges,
        INotifyObservableResets,
        INotifyObserverExceptions
    {
        /// <summary>
        /// Gets an <see cref="T:IObservable{TKey}"/> containing the current and future added keys of the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:IObservable{TKey}"/> containing the current and future added keys of the object that implements <see cref="IObservableCache{TKey,TValue}"/>.
        /// </returns>
        IObservable<TKey> Keys { get; }

        /// <summary>
        /// Gets an <see cref="T:IObservable{TValue}"/> containing the the current and future added or replaced values of the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:IObservable{TValue}"/> containing the current and future added or replaced values of the object that implements <see cref="IObservableCache{TKey,TValue}"/>.
        /// </returns>
        IObservable<TValue> Values { get; }

        /// <summary>
        /// Gets an <see cref="T:ICollection{TKey}"/> containing the current keys inside the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:IObservable{TKey}"/> containing the current keys of the object that implements <see cref="IObservableCache{TKey,TValue}"/>.
        /// </returns>
        ICollection<TKey> CurrentKeys { get; }

        /// <summary>
        /// Gets an <see cref="T:ICollection{TValue}"/> containing the current values inside the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:IObservable{TValue}"/> containing the current values of the object that implements <see cref="IObservableCache{TKey,TValue}"/>.
        /// </returns>
        ICollection<TValue> CurrentValues { get; }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        int CurrentCount { get; }

        /// <summary>
        /// Subscribes to the <paramref name="source"/> and adds its provided key/value pairs to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source">The observable sequence of key/value pairs to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="source"/> key/value pairs.</param>
        /// <param name="expirationType">Defines how the <paramref name="source" /> key/value pairs shall expire.</param>
        /// <param name="scheduler">Scheduler to perform the add action on.</param>
        /// <returns>
        /// An observable stream of added element from the <paramref name="source"/>.
        /// </returns>
        IObservable<KeyValuePair<TKey, TValue>> Add(IObservable<KeyValuePair<TKey, TValue>> source, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null);

        /// <summary>
        /// Subscribes to the <paramref name="source"/> and adds its provided range of key/value pairs to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source">The observable sequence of range of key/value pairs to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="source"/> key/value pairs.</param>
        /// <param name="expirationType">Defines how the <paramref name="source" /> key/value pairs shall expire.</param>
        /// <param name="scheduler">Scheduler to perform the add action on.</param>
        /// <returns>
        /// An observable stream of added element from the <paramref name="source"/>.
        /// </returns>
        IObservable<KeyValuePair<TKey, TValue>> AddRange(IObservable<IList<KeyValuePair<TKey, TValue>>> source, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.DoNothing, IScheduler scheduler = null);

        /// <summary>
        /// Clears this instance for every <see cref="Unit"/> signaled via the <paramref name="source"/> observable.
        /// </summary>
        /// <param name="source">The source triggers.</param>
        /// <param name="scheduler">Scheduler to perform the clear action on.</param>
        /// <returns>
        /// An observable stream that signals each clear with an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Clear(IObservable<Unit> source, IScheduler scheduler = null);

        /// <summary>
        /// Determines whether this instance contains the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="scheduler">Scheduler to perform the check on.</param>
        /// <returns>
        /// An observable stream that returns [true] if the <paramref name="key"/> is is contained in this instance, [false] if not.
        /// </returns>
        IObservable<bool> Contains(TKey key, IScheduler scheduler = null);

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
        /// <param name="scheduler">Scheduler to run the checks on.</param>
        /// <returns>
        /// An observable stream that returns the subset of keys of the provided <paramref name="keys"/> that are contained in this instance.
        /// </returns>
        IObservable<TKey> ContainsWhich(IEnumerable<TKey> keys, IScheduler scheduler = null);

        /// <summary>
        /// Determines the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="scheduler"><see cref="IScheduler"/> to perform the check on.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </returns>
        IObservable<DateTime> ExpiresAt(TKey key, IScheduler scheduler = null);

        /// <summary>
        /// Determines the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="scheduler"><see cref="IScheduler"/> to perform the check on.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </returns>
        IObservable<TimeSpan> ExpiresIn(TKey key, IScheduler scheduler = null);

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to retrieve the <typeparamref name="TValue"/> for.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="key"/> has expired before retrieval.</param>
        /// <param name="scheduler">Scheduler to perform the retrieval on.</param>
        /// <returns>
        /// An observable stream that returns the <typeparamref name="TValue"/> for the provided <paramref name="key"/>.
        /// </returns>
        IObservable<TValue> Get(TKey key, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Gets the values for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to retrieve the values for.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if one of the elements has expired before retrieval.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent retrievals.</param>
        /// <param name="scheduler">Scheduler to run the retrievals on.</param>
        /// <returns>
        /// An observable stream that returns the values for the provided <paramref name="keys"/>.
        /// </returns>
        IObservable<TValue> Get(IEnumerable<TKey> keys, bool throwIfExpired = true, int maxConcurrent = 1, IScheduler scheduler = null);

        /// <summary>
        /// Subscribes to the observable <paramref name="source"/> stream of keys and removes them from the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source">The observable stream of key(s) to remove.</param>
        /// <param name="scheduler">Scheduler to perform the removal on.</param>
        /// <returns>
        /// An observable stream that returns an observable stream of either [true] or [false] for every element provided by the <paramref name="source"/> observable
        /// and whether it was successfully found and removed.. or not.
        /// </returns>
        /// <remarks>
        /// The returned observable stream of [true] or [false] has the same order as the <paramref name="source"/> observable.
        /// </remarks>
        IObservable<bool> Remove(IObservable<TKey> source, IScheduler scheduler = null);

        /// <summary>
        /// Subscribes to the observable <paramref name="source"/> stream of range of keys and removes them from the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source">The observable stream of range of key(s) to remove.</param>
        /// <param name="scheduler">Scheduler to perform the removal on.</param>
        /// <returns>
        /// An observable stream that returns an observable stream of either [true] or [false] for every element provided by the <paramref name="source"/> observable
        /// and whether it was successfully found and removed.. or not.
        /// </returns>
        /// <remarks>
        /// The returned observable stream of [true] or [false] has the same order as the <paramref name="source"/> observable.
        /// </remarks>
        IObservable<bool> RemoveRange(IObservable<IList<TKey>> source, IScheduler scheduler = null);

        /// <summary>
        /// Updates the specified <paramref name="key"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The value to update the <paramref name="key"/> with.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="key"/> has expired upon subscription.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Update(TKey key, TValue value, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Updates a range of <paramref name="keyValuePairs"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs that each contain the key to update and the value to update it with.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="keyValuePairs"/> has at least one expired item key upon subscription.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> UpdateRange(IDictionary<TKey, TValue> keyValuePairs, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="key"/> has expired upon subscription.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> UpdateExpiration(TKey key, TimeSpan expiry, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="keys"/>.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if (one of) the <paramref name="keys"/> has expired item key upon subscription.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent updates.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> UpdateExpiration(IEnumerable<TKey> keys, TimeSpan expiry, bool throwIfExpired = true, int maxConcurrent = 1, IScheduler scheduler = null);
    }
}