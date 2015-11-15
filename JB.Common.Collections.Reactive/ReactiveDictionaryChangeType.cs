namespace JB.Collections.Reactive
{
    public enum ReactiveDictionaryChangeType
    {
        /// <summary>
        /// One Item has been added.
        /// </summary>
        ItemAdded,
        /// <summary>
        /// One Item has changed.
        /// </summary>
        ItemChanged,
        /// <summary>
        /// One Item has been removed.
        /// </summary>
        ItemRemoved,
        /// <summary>
        /// Indicates a major change and consumers should reset any existing knowledge and state about the affected collection and refetch its current state.
        /// </summary>
        Reset,
    }
}