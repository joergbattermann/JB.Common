namespace JB.Collections.Reactive
{
    public interface IObservableDictionaryChange<out TKey, out TValue>
    {
        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        ObservableDictionaryChangeType ChangeType { get; }

        /// <summary>
        /// Gets the key of the (changed) item.
        /// </summary>
        /// <value>
        /// The key of the (changed) item.
        /// </value>
        TKey Key { get; }

        /// <summary>
        /// Gets the value that was added or if it was a <see cref="ObservableDictionaryChangeType.ItemChanged"/>, this is the new value.
        /// </summary>
        /// <value>
        /// The affected value.
        /// </value>
        TValue Value { get; }

        /// <summary>
        /// Gets the old value, if any. If it was a <see cref="ObservableDictionaryChangeType.ItemChanged"/>, this is the old value,
        /// for <see cref="ObservableDictionaryChangeType.ItemRemoved"/>, this will contain the value removed for the <see cref="Key"/>.
        /// </summary>
        /// <value>
        /// The old value, if any
        /// </value>
        TValue OldValue { get; }
    }
}