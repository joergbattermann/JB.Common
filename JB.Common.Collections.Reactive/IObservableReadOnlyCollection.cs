using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IObservableReadOnlyCollection<T> : INotifyObservableCollectionChanged<T>, INotifyObservableCountChanged, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
	}
}