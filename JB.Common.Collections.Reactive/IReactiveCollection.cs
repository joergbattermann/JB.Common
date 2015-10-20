using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections
{
	// ToDo: INotifyPropertyChanged > Raise property changed for Items[] and Count, see ObservableCollection
	public interface IReactiveCollection<T> : INotifyReactiveCollectionChanged<T>, IReadOnlyCollection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
	{
	}
}