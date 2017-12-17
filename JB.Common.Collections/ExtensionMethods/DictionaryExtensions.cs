// -----------------------------------------------------------------------
// <copyright file="DictionaryExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace JB.Collections.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/> instances
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the keys in the given <paramref name="dictionary" /> for the given <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to check in.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The optional comparer to use.</param>
        /// <returns></returns>
        public static IEnumerable<TKey> GetKeysForValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, IEqualityComparer<TValue> comparer = null)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (comparer == null)
                comparer = EqualityComparer<TValue>.Default;

            return from keyValuePair in dictionary
                   where comparer.Equals(keyValuePair.Value, value)
                   select keyValuePair.Key;
        }
    }
}