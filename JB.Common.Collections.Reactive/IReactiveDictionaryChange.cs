namespace JB.Collections.Reactive
{
    public interface IReactiveDictionaryChange<out TKey, out TValue>
    {
        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        ReactiveDictionaryChangeType ChangeType { get; }

        /// <summary>
        /// Gets the key of the (changed) item.
        /// </summary>
        /// <value>
        /// The key of the (changed) item.
        /// </value>
        TKey Key { get; }

        /// <summary>
        /// Gets the value that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected value, if any.
        /// </value>
        TValue Value { get; }
    }
}