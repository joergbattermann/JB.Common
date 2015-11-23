using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IObservableBindingDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
    {

	}
}