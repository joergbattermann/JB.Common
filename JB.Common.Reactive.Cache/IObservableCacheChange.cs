// -----------------------------------------------------------------------
// <copyright file="IObservableCacheChange.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;

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
        ///     Gets the expiration <see cref="DateTime" />.
        ///     This may be in the past if the <see cref="Key" /> has already expired.
        /// </summary>
        /// <value>
        ///     The expiration <see cref="DateTime"/>.
        /// </value>
        DateTime? ExpiresAt { get; }

        /// <summary>
        /// Gets the type of the expiration associated with the <see cref="Key" />
        /// in the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <value>
        /// The type of the expiration.
        /// </value>
        ObservableCacheExpirationType? ExpirationType { get; }

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

        /// <summary>
        /// If <see cref="IObservableCacheChange{TKey,TValue}.ChangeType"/> is a <see cref="ObservableCacheChangeType.ItemChanged"/> one and <typeparamref name="TValue"/>
        /// implements <see cref="INotifyPropertyChanged"/> and the underlying item change originated from a <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// event, this will be the forwarded <see cref="PropertyChangedEventArgs.PropertyName"/> value.
        /// </summary>
        /// <value>
        /// The changed property name for <see cref="IObservableCacheChange{TKey,TValue}.Value"/>, if applicable.
        /// </value>
        string ChangedPropertyName { get; }
    }
}