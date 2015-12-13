using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="DictionaryItemChanges">observable stream</see> of item changes IF
    /// <typeparam name="TValue" /> implements the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableDictionaryItemChanged<out TKey, out TValue> : INotifyObservableItemChanged
    {
        /// <summary>
        /// Gets the observable streams of item changes, however these will only have their
        /// <see cref="IObservableDictionaryChange{TKey, TValue}.ChangeType" /> set to <see cref="ObservableDictionaryChangeType.ItemChanged" />.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryItemChanges { get; }
    }
}