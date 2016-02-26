using System.ComponentModel;

namespace JB.Collections.Reactive
{
    public interface IObservableDictionaryChange<TKey, TValue>
    {
        /// <summary>
        /// Gets the dictionary that notified about this change.
        /// </summary>
        /// <value>
        /// The dictionary / sender.
        /// </value>
        IObservableDictionary<TKey, TValue> Dictionary { get; }

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
        /// Gets the value that was added, removed or changed, and, if it is a <see cref="ObservableDictionaryChangeType.ItemValueReplaced"/> change, this will be the new value.
        /// </summary>
        /// <value>
        /// The affected value.
        /// </value>
        TValue Value { get; }

        /// <summary>
        /// If <see cref="ChangeType"/> is a <see cref="ObservableDictionaryChangeType.ItemValueReplaced"/> this will hold the old, previous value.
        /// </summary>
        /// <value>
        /// The replaced value, if applicable
        /// </value>
        TValue OldValue { get; }

        /// <summary>
        /// If <see cref="ChangeType"/> is a <see cref="ObservableDictionaryChangeType.ItemValueChanged"/> one and <typeparamref name="TValue"/>
        /// implements <see cref="INotifyPropertyChanged"/> and the underlying item change originated from a <see cref="E:INotifyPropertyChanged.PropertyChanged"/>
        /// event, this will be the forwarded <see cref="PropertyChangedEventArgs.PropertyName"/> value.
        /// </summary>
        /// <value>
        /// The changed property name for <see cref="Value"/>, if applicable.
        /// </value>
        string ChangedPropertyName { get; }
    }
}