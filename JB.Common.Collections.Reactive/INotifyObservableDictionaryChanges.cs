using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableDictionaryChanges<TKey, TValue> : INotifyObservableChanges
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
        [Obsolete("This shall be removed pre 1.0")]
        event EventHandler<ObservableDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;
    }
}