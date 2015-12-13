using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
    public interface IObservableReadOnlyDictionary<TKey, TValue> :
        IObservableReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        INotifyObservableDictionaryChanged<TKey, TValue>,
        INotifyObservableDictionaryItemChanged<TKey, TValue>,
        INotifyObservableResets,
        INotifyObservableExceptionsThrown,
        IReadOnlyDictionary<TKey, TValue>,
        IEnumerable,
        ICollection
    {
        
    }
}