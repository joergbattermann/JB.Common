using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="ValueChanges">observable stream</see> of item changes if
    /// <typeparamref name="TKey"/> and/or <typeparamref name="TValue"/> implement the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableDictionaryItemChanges<out TKey, out TValue> : INotifyObservableItemChanges
    {
        /// <summary>
        /// Gets the observable streams of value changes being either a <see cref="ObservableDictionaryChangeType.ItemValueChanged" />
        /// or <see cref="ObservableDictionaryChangeType.ItemValueReplaced" /> event.
        /// </summary>
        /// <value>
        /// The value changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> ValueChanges { get; }

        /// <summary>
        /// Gets the observable streams of key changes that are <see cref="ObservableDictionaryChangeType.ItemKeyChanged" /> events.
        /// </summary>
        /// <value>
        /// The key changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> KeyChanges { get; }
    }
}