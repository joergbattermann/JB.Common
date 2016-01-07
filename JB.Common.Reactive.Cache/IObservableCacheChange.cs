// -----------------------------------------------------------------------
// <copyright file="IObservableCacheChange.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB.Reactive.Cache
{
    public interface IObservableCacheChange<out TKey, out TValue>
    {
        /// <summary>
        ///     Gets the type of the change.
        /// </summary>
        /// <value>
        ///     The type of the change.
        /// </value>
        ObservableCacheChangeType ChangeType { get; }

        /// <summary>
        ///     Gets the expiration <see cref="DateTime" /> in UTC format.
        ///     This may be in the past if the <see cref="Key" /> has already expired.
        /// </summary>
        /// <value>
        ///     The expiration UTC.
        /// </value>
        DateTime ExpirationUtc { get; }

        /// <summary>
        ///     Gets the key for the affected item.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        TKey Key { get; }

        /// <summary>
        ///     Gets the previous / old value for the <see cref="Key" />.
        /// </summary>
        /// <value>
        ///     The old value.
        /// </value>
        TValue OldValue { get; }

        /// <summary>
        ///     Gets the current / new value for the <see cref="Key" />.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        TValue Value { get; }
    }
}