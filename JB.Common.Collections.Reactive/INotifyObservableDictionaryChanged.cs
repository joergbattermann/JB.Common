using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableDictionaryChanged<TKey, TValue> : INotifyObservableChanges
    {
        /// <summary>
        /// Gets the dictionary changes as an observable stream.
        /// </summary>
        /// <value>
        /// The dictionary changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryChanges { get; }

        /// <summary>
        /// Occurs when the corresponding <see cref="IObservableDictionary{TKey,TValue}"/> changed.
        /// </summary>
        event EventHandler<ObservableDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;
    }
}