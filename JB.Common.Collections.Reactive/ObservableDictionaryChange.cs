using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    public class ObservableDictionaryChange<TKey, TValue> : IObservableDictionaryChange<TKey, TValue>
    {
        /// <summary>
        /// The <typeparamref name="TKey">key type</typeparamref> is a value type. or not.
        /// </summary>
        private static readonly Lazy<bool> KeyIsValueType = new Lazy<bool>(() => typeof(TKey).IsValueType);

        /// <summary>
        /// The <typeparamref name="TValue"> name="TValue">value type</typeparamref> is a.. err.. value type. or not.
        /// </summary>
        private static readonly Lazy<bool> ValueIsValueType = new Lazy<bool>(() => typeof(TValue).IsValueType);

        #region Implementation of IObservableCollectionChange<out T>

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableDictionaryChangeType ChangeType { get; }

        /// <summary>
        /// Gets the key of the (changed) item.
        /// </summary>
        /// <value>
        /// The key of the (changed) item.
        /// </value>
        public TKey Key { get; }

        /// <summary>
        /// Gets the value that was added or if it was a <see cref="F:JB.Collections.Reactive.ObservableDictionaryChangeType.ItemChanged" />, this is the new value.
        /// </summary>
        /// <value>
        /// The affected value.
        /// </value>
        public TValue Value { get; }

        /// <summary>
        /// If <see cref="P:JB.Collections.Reactive.IObservableDictionaryChange`2.ChangeType" /> is a <see cref="F:JB.Collections.Reactive.ObservableDictionaryChangeType.ItemChanged" /> one and the underlying change
        /// is a full item replacement rather than a single sub-property change, this is the old, replaced value.
        /// </summary>
        /// <value>
        /// The replaced value, if applicable
        /// </value>
        public TValue OldValue { get; }

        /// <summary>
        /// If <see cref="IObservableDictionaryChange{TKey,TValue}.ChangeType"/> is a <see cref="ObservableDictionaryChangeType.ValueChanged"/> one and <typeparamref name="TValue"/>
        /// implements <see cref="INotifyPropertyChanged"/> and the underlying item change originated from a <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// event, this will be the forwarded <see cref="PropertyChangedEventArgs.PropertyName"/> value.
        /// </summary>
        /// <value>
        /// The changed property name for <see cref="IObservableDictionaryChange{TKey,TValue}.Value"/>, if applicable.
        /// </value>
        public string ChangedPropertyName { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionaryChange{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="key">The key of the changed value.</param>
        /// <param name="value">The added, removed or changed, new value.</param>
        /// <param name="oldValue">The replaced value, only applicable if <paramref name="changeType" /> is <see cref="ObservableDictionaryChangeType.ValueChanged" />.</param>
        /// <param name="changedPropertyName">The changed property name, only applicable if <paramref name="changeType" /> is <see cref="ObservableDictionaryChangeType.ValueChanged" />.</param>
        protected ObservableDictionaryChange(ObservableDictionaryChangeType changeType, TKey key = default(TKey), TValue value = default(TValue), TValue oldValue = default(TValue), string changedPropertyName = "")
        {
            if ((changeType != ObservableDictionaryChangeType.Reset)
                && (KeyIsValueType.Value == false && Equals(key, default(TKey))))
                throw new ArgumentOutOfRangeException(nameof(key), $"Item Adds, Key-/Value Changes or Removes must have a (non-default) '{nameof(key)}'");

            if (changeType == ObservableDictionaryChangeType.Reset && (ValueIsValueType.Value == false && !Equals(value, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(value), $"Resets must not have a '{nameof(value)}'");

            if (changeType != ObservableDictionaryChangeType.ValueReplaced && changeType != ObservableDictionaryChangeType.ItemRemoved
                && (ValueIsValueType.Value == false && !Equals(oldValue, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(oldValue), $"Only Changes may have a '{nameof(oldValue)}'");

            if (changeType != ObservableDictionaryChangeType.ValueChanged && changeType != ObservableDictionaryChangeType.KeyChanged && !string.IsNullOrWhiteSpace(changedPropertyName))
                throw new ArgumentOutOfRangeException(nameof(changedPropertyName), $"Only '{ObservableDictionaryChangeType.ValueChanged}' and '{ObservableDictionaryChangeType.KeyChanged}' Changes may have a '{nameof(changedPropertyName)}'");

            ChangeType = changeType;

            Key = key;

            Value = value;
            OldValue = oldValue;

            ChangedPropertyName = changedPropertyName ?? string.Empty;
        }

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.Reset" />.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing a <see cref="ObservableDictionaryChangeType.Reset" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> Reset()
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.Reset);

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.ItemAdded" />
        /// for the given <paramref name="key" /> and <paramref name="value" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing a <see cref="ObservableDictionaryChangeType.ItemAdded" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> ItemAdded(TKey key, TValue value)
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.ItemAdded, key, value);

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.ItemRemoved" />
        /// for the given <paramref name="key" /> and <paramref name="value" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing a <see cref="ObservableDictionaryChangeType.ItemRemoved" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> ItemRemoved(TKey key, TValue value)
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.ItemRemoved, key, value);

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.KeyChanged" />,
        /// meaning that a <paramref name="key" />, more precisely one of its properties, has changed.
        /// </summary>
        /// <param name="key">The key that changed.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing an item property changed <see cref="ObservableDictionaryChangeType.ValueChanged" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> KeyChanged(TKey key, string propertyName)
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.KeyChanged, key, changedPropertyName: propertyName);

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.ValueChanged" />,
        /// meaning that a given <paramref name="value"/>, more precisely one of its properties, for one or more key(s) has changed.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing an item property changed <see cref="ObservableDictionaryChangeType.ValueChanged" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> ValueChanged(TValue value, string propertyName)
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.ValueChanged, value: value, changedPropertyName: propertyName);

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}" /> representing a <see cref="ObservableDictionaryChangeType.ValueReplaced" />,
        /// more particularly one for an item replacement inside the <see cref="IObservableDictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="replacedOldValue">The replaced old value.</param>
        /// <returns></returns>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing an item replacement <see cref="ObservableDictionaryChangeType.ValueReplaced" />.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> ValueReplaced(TKey key, TValue newValue, TValue replacedOldValue)
            => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.ValueReplaced, key, newValue, replacedOldValue);
    }
}