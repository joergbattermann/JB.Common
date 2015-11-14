using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IReactiveBindingDictionary<TKey, TValue> : IReactiveDictionary<TKey, TValue>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
    {

	}
}