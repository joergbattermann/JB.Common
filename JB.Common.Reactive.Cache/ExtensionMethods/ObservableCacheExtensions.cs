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

namespace JB.Reactive.Cache.ExtensionMethods
{
    /// <summary>
    ///     Extension methods for <see cref="IObservableCache{TKey,TValue}" /> instances.
    /// </summary>
    public static class ObservableCacheExtensions
    {
        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, KeyValuePair<TKey, TValue> keyValuePair, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> AddOrUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, Func<TKey, TValue, TValue, TValue> valueSelector, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, Func<TKey, TValue> producer, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<TValue> GetOrAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, IEnumerable<TKey> keys, Func<IEnumerable<TKey>, TValue> producer, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<bool> TryAdd<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<bool> TryRemove<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key)
        {
            throw new NotImplementedException();
        }

        public static IObservable<bool> TryUpdate<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, TValue value, TimeSpan? expiry = null)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> UpdateExpiration<TKey, TValue>(this IObservableCache<TKey, TValue> cache, TKey key, DateTime expiration)
        {
            throw new NotImplementedException();
        }
    }
}