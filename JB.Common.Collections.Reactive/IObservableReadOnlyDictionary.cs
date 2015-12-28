using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
    public interface IObservableReadOnlyDictionary<TKey, TValue> :
        IObservableReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        INotifyObservableDictionaryChanged<TKey, TValue>,
        INotifyObservableDictionaryItemChanged<TKey, TValue>,
        INotifyObservableResets,
        INotifyUnhandledObserverExceptions,
        IReadOnlyDictionary<TKey, TValue>,
        IEnumerable,
        ICollection
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