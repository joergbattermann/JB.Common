using System.ComponentModel;

namespace JB.Collections.Reactive
{
    public enum ObservableDictionaryChangeType
    {
        /// <summary>
        /// One Item has been added.
        /// </summary>
        ItemAdded,
        /// <summary>
        /// A key that implements <see cref="INotifyPropertyChanged"/> has changed.
        /// </summary>
        KeyChanged,
        /// <summary>
        /// A value that implements <see cref="INotifyPropertyChanged"/> has changed.
        ///  </summary>
        /// <remarks>The same value can be used by different keys.</remarks>
        ValueChanged,
        /// <summary>
        /// The value for a key has been replaced.
        /// </summary>
        ValueReplaced,
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