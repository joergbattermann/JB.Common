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
        /// <summary>
        /// Adds or if it already exists updates the <paramref name="key"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache to use.</param>
        /// <param name="key">The key for the <paramref name="value"/>.</param>
        /// <param name="value">The value to add or update.</param>
        /// <param name="expiry">The expiry. If none is provided, <see cref="TimeSpan.MaxValue"/> will be used.</param>
        /// <returns></returns>
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