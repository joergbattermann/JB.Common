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
        INotifyUnhandledObserverExceptions,
        IReadOnlyDictionary<TKey, TValue>,
        IEnumerable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the keys for the given <paramref name="value"/>, if any.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IEnumerable<TKey> GetKeysForValue(TValue value);
    }
}