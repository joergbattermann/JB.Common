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
        /// Clears this instance for every <see cref="Unit"/> signaled via the <paramref name="clearTriggers"/> observable.
        /// </summary>
        /// <param name="clearTriggers">The clear triggers.</param>
        /// <param name="scheduler">Scheduler to perform the clear action on.</param>
        /// <returns>
        /// An observable stream that signals each clear with an <see cref="Unit" />.
        /// </returns>
        IObservable<Unit> Clear(IObservable<Unit> clearTriggers, IScheduler scheduler = null);

        /// <summary>
        /// Determines whether this instance contains the keys provided by the observable <paramref name="keys"/> sequence.
        /// </summary>
        /// <param name="keys">The observable sequence of keys to check.</param>
        /// <param name="scheduler">Scheduler to perform the check(s) on.</param>
        /// <returns>
        /// An observable stream that returns [true] for each provided key that is is contained in this instance, [false] if not.
        /// </returns>
        IObservable<bool> Contains(IObservable<TKey> keys, IScheduler scheduler = null);

        /// <summary>
        /// Determines the <see cref="DateTime"/> (UTC) the <paramref name="keys"/> expire.
        /// </summary>
        /// <param name="keys">The expire to check.</param>
        /// <param name="scheduler"><see cref="IScheduler"/> to perform the check on.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expire in the same chronological order they were provided.
        /// </returns>
        IObservable<DateTime> ExpiresAt(IObservable<TKey> keys, IScheduler scheduler = null);

        /// <summary>
        /// Determines the <see cref="TimeSpan"/> in which the <paramref name="keys"/> expire.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <param name="scheduler"><see cref="IScheduler"/> to perform the check on.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TimeSpan"/> in which the <paramref name="keys"/> expire in the same chronological order they were provided.
        /// </returns>
        IObservable<TimeSpan> ExpiresIn(IObservable<TKey> keys, IScheduler scheduler = null);

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to retrieve the <typeparamref name="TValue"/> for.</param>
        /// <param name="throwIfExpired">
        ///     If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="keys"/> have expired before retrieval.</param>
        /// <param name="scheduler">Scheduler to perform the retrieval on.</param>
        /// <returns>
        /// An observable stream that returns the <typeparamref name="TValue"/> for the provided <paramref name="keys"/> in the same order they were provided.
        /// </returns>
        IObservable<TValue> Get(IObservable<TKey> keys, bool throwIfExpired = true, IScheduler scheduler = null);
        
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
        /// Observers the source observable stream of <paramref name="keyValuePairs"/> and updates the <see cref="KeyValuePair{TKey,TValue}.Key"/>
        /// in this instance with the provided <see cref="KeyValuePair{TKey,TValue}.Value"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key value pairs to observe.</param>
        /// <param name="throwIfExpired">
        ///     If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}" /> will be thrown if the <see cref="KeyValuePair{TKey,TValue}.Key"/> has expired upon updating.
        /// </param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that returns the updated <paramref name="keyValuePairs"/>.
        /// </returns>
        IObservable<KeyValuePair<TKey, TValue>> Update(IObservable<KeyValuePair<TKey, TValue>> keyValuePairs, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Updates a range of <paramref name="keyValuePairs"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs that each contain the key to update and the value to update it with.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="keyValuePairs"/> has at least one expired item key upon subscription.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that returns the updated <paramref name="keyValuePairs"/>.
        /// </returns>
        IObservable<KeyValuePair<TKey, TValue>> UpdateRange(IObservable<ICollection<KeyValuePair<TKey, TValue>>> keyValuePairs, bool throwIfExpired = true, IScheduler scheduler = null);

        /// <summary>
        /// Updates the expiration for the specified <paramref name="keysAndNewExpiry"/>.
        /// </summary>
        /// <param name="keysAndNewExpiry">The keys to update with the corresponding new expiry <see cref="TimeSpan"/>.</param>
        /// <param name="throwIfExpired">If set to <c>true</c>, a <see cref="KeyHasExpiredException{TKey}"/> will be thrown if the <paramref name="key"/> has expired upon subscription.</param>
        /// <param name="scheduler">Scheduler to perform the update on.</param>
        /// <returns>
        /// An observable stream that returns the updated key-value pairs.
        /// </returns>
        IObservable<KeyValuePair<TKey, TValue>> UpdateExpiry(IObservable<KeyValuePair<TKey, TimeSpan>> keysAndNewExpiry, bool throwIfExpired = true, IScheduler scheduler = null);
    }
}