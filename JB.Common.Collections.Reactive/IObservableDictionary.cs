using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
	public interface IObservableDictionary<TKey, TValue> : IObservableReadOnlyDictionary<TKey, TValue>, IObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary
    {
		
	}
}