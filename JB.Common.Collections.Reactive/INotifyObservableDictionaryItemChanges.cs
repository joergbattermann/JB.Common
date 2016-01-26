using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="DictionaryValueChanges">observable stream</see> of item changes if
    /// <typeparamref name="TKey"/> and/or <typeparamref name="TValue"/> implement the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableDictionaryItemChanges<out TKey, out TValue> : INotifyObservableItemChanges
    {
        /// <summary>
        /// Gets the observable streams of value changes being either a <see cref="ObservableDictionaryChangeType.ValueChanged" />
        /// or <see cref="ObservableDictionaryChangeType.ValueReplaced" /> event.
        /// </summary>
        /// <value>
        /// The value changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryValueChanges { get; }

        /// <summary>
        /// Gets the observable streams of key changes that are <see cref="ObservableDictionaryChangeType.KeyChanged" /> events.
        /// </summary>
        /// <value>
        /// The key changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryKeyChanges { get; }
    }
}