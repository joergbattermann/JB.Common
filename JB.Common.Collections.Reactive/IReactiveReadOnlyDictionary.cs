using System.Collections.Generic;

namespace JB.Collections.Reactive
{
    public interface IReactiveReadOnlyDictionary<TKey, TValue> : IReactiveReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
    {
        
    }
}