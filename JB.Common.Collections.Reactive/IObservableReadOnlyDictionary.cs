using System.Collections;
using System.Collections.Generic;
using JB.Reactive;

namespace JB.Collections.Reactive
{
    public interface IObservableReadOnlyDictionary<TKey, TValue> :
        IObservableReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        INotifyObservableDictionaryChanges<TKey, TValue>,
        INotifyObservableDictionaryItemChanges<TKey, TValue>,
        INotifyObservableResets,
        INotifyObserverExceptions,
        IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }
    }
}