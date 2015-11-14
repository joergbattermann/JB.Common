using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
	public interface IReactiveDictionary<TKey, TValue> : IReactiveReadOnlyDictionary<TKey, TValue>, IReactiveCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary
    {
		
	}
}