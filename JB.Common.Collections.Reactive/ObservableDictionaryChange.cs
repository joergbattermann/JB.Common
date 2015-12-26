using System;

namespace JB.Collections.Reactive
{
    public class ObservableDictionaryChange<TKey, TValue> : IObservableDictionaryChange<TKey, TValue>
    {
        /// <summary>
        /// The <typeparam name="TKey">key type</typeparam> is a value type. or not.
        /// </summary>
        private static readonly Lazy<bool> KeyIsValueType = new Lazy<bool>(() => typeof(TKey).IsValueType);

        /// <summary>
        /// The <typeparam name="TValue">value type</typeparam> is a.. err.. value type. or not.
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
        /// Gets the old value, if any. If it was a <see cref="F:JB.Collections.Reactive.ObservableDictionaryChangeType.ItemChanged" />, this is the old value,
        /// for <see cref="F:JB.Collections.Reactive.ObservableDictionaryChangeType.ItemRemoved" />, this will contain the value removed for the <see cref="P:JB.Collections.Reactive.IObservableDictionaryChange`2.Key" />.
        /// </summary>
        /// <value>
        /// The old value, if any
        /// </value>
        public TValue OldValue { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionaryChange{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="key">The key of the changed value.</param>
        /// <param name="value">The added, removed or changed, new value.</param>
        /// <param name="oldValue">The changed, old value.</param>
        public ObservableDictionaryChange(ObservableDictionaryChangeType changeType, TKey key = default(TKey), TValue value = default(TValue), TValue oldValue = default(TValue))
        {
            if ((changeType != ObservableDictionaryChangeType.Reset)
                && (KeyIsValueType.Value == false && Equals(key, default(TKey))))
                throw new ArgumentOutOfRangeException(nameof(key), $"Item Adds, Changes or Removes must have a (non-default) {nameof(key)}");

            if (changeType == ObservableDictionaryChangeType.Reset && (ValueIsValueType.Value == false && !Equals(value, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(value), $"Resets must not have a {nameof(value)}");

            if (changeType == ObservableDictionaryChangeType.Reset && (ValueIsValueType.Value == false && !Equals(oldValue, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(oldValue), $"Resets must not have a {nameof(oldValue)}");

            ChangeType = changeType;

            Key = key;

            Value = value;
            OldValue = oldValue;
        }

        /// <summary>
        /// Gets a <see cref="IObservableDictionaryChange{TKey,TValue}"/> representing a <see cref="ObservableDictionaryChangeType.Reset"/>.
        /// </summary>
        /// <value>
        /// An <see cref="IObservableDictionaryChange{TKey,TValue}">instance</see> representing a <see cref="ObservableDictionaryChangeType.Reset"/>.
        /// </value>
        public static IObservableDictionaryChange<TKey, TValue> Reset => new ObservableDictionaryChange<TKey, TValue>(ObservableDictionaryChangeType.Reset);
    }
}