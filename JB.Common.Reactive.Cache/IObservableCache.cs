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

namespace JB.Reactive.Cache
{
    public interface IObservableCache<TKey, TValue>
    {
        /// <summary>
        ///     Gets an observable stream of changes to this <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <value>
        ///     The changes.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> Changes { get; }

        IObservable<Unit> Add(TKey key, TValue value, TimeSpan? expiry = null);

        IObservable<Unit> Add(KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null);

        IObservable<Unit> Add(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null);

        IObservable<Unit> Clear();

        IObservable<bool> Contains(TKey key);

        IObservable<bool> ContainsAll(IEnumerable<TKey> keys);

        IObservable<TKey> ContainsWhich(IEnumerable<TKey> keys);

        IObservable<DateTime> ExpiresAt(TKey key);

        IObservable<TValue> Get(TKey key);

        IObservable<TValue> Get(IEnumerable<TKey> keys);

        IObservable<Unit> Remove(TKey key);

        IObservable<Unit> Remove(IEnumerable<TKey> keys);

        IObservable<Unit> Update(TKey key, TValue value, TimeSpan? expiry = null);

        IObservable<Unit> Update(KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null);

        IObservable<Unit> Update(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null);

        IObservable<Unit> UpdateExpiration(TKey key, TimeSpan expiry);
    }
}