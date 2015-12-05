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
        /// Gets the value that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected value, if any.
        /// </value>
        public TValue Value { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionaryChange{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="key">The key of the changed value.</param>
        /// <param name="value">The (changed) value.</param>
        public ObservableDictionaryChange(ObservableDictionaryChangeType changeType, TKey key = default(TKey), TValue value = default(TValue))
        {
            if ((changeType == ObservableDictionaryChangeType.ItemAdded
                 || changeType == ObservableDictionaryChangeType.ItemChanged
                 || changeType == ObservableDictionaryChangeType.ItemRemoved)
                && (KeyIsValueType.Value == false && Equals(key, default(TKey))))
                throw new ArgumentOutOfRangeException(nameof(key), $"Item Adds, Changes, Moves and Removes must have a (non-default) {nameof(key)}");

            if ((changeType == ObservableDictionaryChangeType.ItemAdded
                 || changeType == ObservableDictionaryChangeType.ItemChanged
                 || changeType == ObservableDictionaryChangeType.ItemRemoved)
                && (ValueIsValueType.Value == false && Equals(value, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(value), $"Item Adds, Changes, Moves and Removes must have a (non-default) {nameof(value)}");

            if (changeType == ObservableDictionaryChangeType.Reset && (ValueIsValueType.Value == false && !Equals(value, default(TValue))))
                throw new ArgumentOutOfRangeException(nameof(value), $"Resets must not have a {nameof(value)}");

            ChangeType = changeType;

            Key = key;
            Value = value;
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