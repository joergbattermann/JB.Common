using System.Collections;
using System.Collections.Generic;
using JB.Reactive;

namespace JB.Collections.Reactive
{
    public interface IObservableDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        INotifyObservableDictionaryChanges<TKey, TValue>,
        INotifyObservableDictionaryItemChanges<TKey, TValue>,
        INotifyObservableResets,
        INotifyObserverExceptions
    {
	}
}