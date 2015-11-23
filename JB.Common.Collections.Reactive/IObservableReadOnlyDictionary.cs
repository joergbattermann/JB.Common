using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
    public interface IObservableReadOnlyDictionary<TKey, TValue> : IObservableReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IEnumerable, ICollection
    {
        
    }
}