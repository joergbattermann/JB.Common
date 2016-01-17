﻿// -----------------------------------------------------------------------
// <copyright file="ObservableCacheChange.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace JB.Reactive.Cache
{
    public class ObservableCacheChange<TKey, TValue> : IObservableCacheChange<TKey, TValue>
    {
        /// <summary>
        /// The <typeparam name="TKey">key type</typeparam> is a value type. or not.
        /// </summary>
        private static readonly Lazy<bool> KeyIsValueType = new Lazy<bool>(() => typeof(TKey).IsValueType);

        /// <summary>
        /// The <typeparam name="TValue">value type</typeparam> is a.. err.. value type. or not.
        /// </summary>
        private static readonly Lazy<bool> ValueIsValueType = new Lazy<bool>(() => typeof(TValue).IsValueType);

        #region Implementation of IObservableCacheChange<out TKey,out TValue>

        /// <summary>
        ///     Gets the type of the change.
        /// </summary>
        /// <value>
        ///     The type of the change.
        /// </value>
        public ObservableCacheChangeType ChangeType { get; }

        /// <summary>
        ///     Gets the expiration <see cref="DateTime" />.
        ///     This may be in the past if the <see cref="Key" /> has already expired.
        /// </summary>
        /// <value>
        ///     The expiration <see cref="DateTime"/>.
        /// </value>
        public DateTime? ExpiresAt { get; }

        /// <summary>
        /// Gets the type of the expiration associated with the <see cref="Key" />
        /// in the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <value>
        /// The type of the expiration.
        /// </value>
        public ObservableCacheExpirationType? ExpirationType { get; }

        /// <summary>
        ///     Gets the key for the affected item.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        public TKey Key { get; }

        /// <summary>
        ///     Gets the previous / old value for the <see cref="IObservableCacheChange{TKey,TValue}.Key" />.
        /// </summary>
        /// <value>
        ///     The old value.
        /// </value>
        public TValue OldValue { get; }

        /// <summary>
        ///     Gets the current / new value for the <see cref="IObservableCacheChange{TKey,TValue}.Key" />.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public TValue Value { get; }

        /// <summary>
        /// If <see cref="IObservableCacheChange{TKey,TValue}.ChangeType"/> is a <see cref="ObservableCacheChangeType.ItemChanged"/> one and <typeparamref name="TValue"/>
        /// implements <see cref="INotifyPropertyChanged"/> and the underlying item change originated from a <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// event, this will be the forwarded <see cref="PropertyChangedEventArgs.PropertyName"/> value.
        /// </summary>
        /// <value>
        /// The changed property name for <see cref="IObservableCacheChange{TKey,TValue}.Value"/>, if applicable.
        /// </value>
        public string ChangedPropertyName { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCacheChange{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="key">The key of the changed value.</param>
        /// <param name="value">The added, removed or changed, new value.</param>
        /// <param name="oldValue">The replaced value, only applicable if <paramref name="changeType" /> is <see cref="ObservableCacheChangeType.ItemChanged" />.</param>
        /// <param name="changedPropertyName">The changed property name, only applicable if <paramref name="changeType" /> is <see cref="ObservableCacheChangeType.ItemChanged" />.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime"/> the <paramref name="key"/> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        protected ObservableCacheChange(ObservableCacheChangeType changeType, TKey key = default(TKey), TValue value = default(TValue), TValue oldValue = default(TValue), string changedPropertyName = "", DateTime? expiresAt = default(DateTime?), ObservableCacheExpirationType? expirationType = default(ObservableCacheExpirationType?))
        {
            if ((changeType != ObservableCacheChangeType.Reset)
                && (expiresAt.HasValue == false))
                throw new ArgumentOutOfRangeException(nameof(expiresAt), $"{nameof(expiresAt)} must be provided for all non-Reset changes");

            if ((changeType != ObservableCacheChangeType.Reset)
                && (expirationType.HasValue == false))
                throw new ArgumentOutOfRangeException(nameof(expirationType), $"{nameof(expirationType)} must be provided for all non-Reset changes");
            
            if ((changeType != ObservableCacheChangeType.Reset)
                && (KeyIsValueType.Value == false && Equals(key, default(TKey))))
                throw new ArgumentOutOfRangeException(nameof(key), $"Item Adds, Changes, Expires or Removes must have a (non-default) {nameof(key)}");

            if (changeType == ObservableCacheChangeType.Reset && (ValueIsValueType.Value == false && !Equals(value, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(value), $"Resets must not have a {nameof(value)}");

            if ((changeType != ObservableCacheChangeType.ItemReplaced && changeType != ObservableCacheChangeType.ItemRemoved)
                && (ValueIsValueType.Value == false && !Equals(oldValue, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(oldValue), $"Only Changes may have a {nameof(oldValue)}");

            if (changeType != ObservableCacheChangeType.ItemChanged && !string.IsNullOrWhiteSpace(changedPropertyName))
                throw new ArgumentOutOfRangeException(nameof(changedPropertyName), $"Only Changes may have a {nameof(changedPropertyName)}");

            ChangeType = changeType;

            ExpiresAt = expiresAt;
            ExpirationType = expirationType;

            Key = key;

            Value = value;
            OldValue = oldValue;

            ChangedPropertyName = changedPropertyName ?? string.Empty;
        }


        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.Reset" />.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing a <see cref="ObservableCacheChangeType.Reset" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> Reset()
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.Reset);

        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.ItemAdded" />
        /// for the given <paramref name="key" /> and <see cref="value" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime"/> the <paramref name="key"/> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing a <see cref="ObservableCacheChangeType.ItemAdded" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> ItemAdded(TKey key, TValue value, DateTime expiresAt, ObservableCacheExpirationType expirationType)
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.ItemAdded, key, value, expiresAt: expiresAt, expirationType: expirationType);

        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.ItemRemoved" />
        /// for the given <paramref name="key" /> and <see cref="value" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime"/> the <paramref name="key"/> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing a <see cref="ObservableCacheChangeType.ItemRemoved" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> ItemRemoved(TKey key, TValue value, DateTime expiresAt, ObservableCacheExpirationType expirationType)
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.ItemRemoved, key, value, expiresAt: expiresAt, expirationType: expirationType);

        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.ItemReplaced" />,
        /// more particularly one for an item replacement inside the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="replacedOldValue">The replaced old value.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime"/> the <paramref name="key"/> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing an item replacement <see cref="ObservableCacheChangeType.ItemReplaced" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> ItemReplaced(TKey key, TValue newValue, TValue replacedOldValue, DateTime expiresAt, ObservableCacheExpirationType expirationType)
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.ItemReplaced, key, newValue, replacedOldValue, expiresAt: expiresAt, expirationType: expirationType);

        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.ItemExpired" />,
        /// more particularly one for an item replacement inside the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime" /> the <paramref name="key" /> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing an item replacement <see cref="ObservableCacheChangeType.ItemExpired" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> ItemExpired(TKey key, TValue value, DateTime expiresAt, ObservableCacheExpirationType expirationType)
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.ItemExpired, key, value, expiresAt: expiresAt, expirationType: expirationType);

        /// <summary>
        /// Gets a <see cref="IObservableCacheChange{TKey,TValue}" /> representing a <see cref="ObservableCacheChangeType.ItemChanged" />,
        /// more particularly one for an item's property change.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expiresAt">The expires <see cref="DateTime"/> the <paramref name="key"/> expires / expired at.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableCacheChange{TKey,TValue}">instance</see> representing an item property changed <see cref="ObservableCacheChangeType.ItemChanged" />.
        /// </value>
        public static IObservableCacheChange<TKey, TValue> ItemChanged(TKey key, TValue value, string propertyName, DateTime expiresAt, ObservableCacheExpirationType expirationType)
            => new ObservableCacheChange<TKey, TValue>(ObservableCacheChangeType.ItemChanged, key, value, changedPropertyName: propertyName, expiresAt: expiresAt, expirationType: expirationType);
    }
}