using System.ComponentModel;

namespace JB.Collections
{
	public interface IReactiveBindingList<T> : IReactiveList<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
	{
		
	}
}