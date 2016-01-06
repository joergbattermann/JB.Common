using System.ComponentModel;

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
        /// If <see cref="ChangeType"/> is a <see cref="ObservableDictionaryChangeType.ItemReplaced"/>
        /// or <see cref="ObservableDictionaryChangeType.ItemRemoved"/> this will hold the old, previous value.
        /// </summary>
        /// <value>
        /// The replaced value, if applicable
        /// </value>
        TValue OldValue { get; }

        /// <summary>
        /// If <see cref="ChangeType"/> is a <see cref="ObservableDictionaryChangeType.ItemChanged"/> one and <typeparamref name="TValue"/>
        /// implements <see cref="INotifyPropertyChanged"/> and the underlying item change originated from a <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// event, this will be the forwarded <see cref="PropertyChangedEventArgs.PropertyName"/> value.
        /// </summary>
        /// <value>
        /// The changed property name for <see cref="Value"/>, if applicable.
        /// </value>
        string ChangedPropertyName { get; }
    }
}