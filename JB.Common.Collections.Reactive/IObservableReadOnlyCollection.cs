using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IObservableReadOnlyCollection<out T> :
        INotifyObservableCollectionItemChanged<T>,
        INotifyObservableCountChanged,
        INotifyCollectionChanged,
        INotifyObservableResets,
        INotifyObservableExceptionsThrown,
        INotifyPropertyChanged,
        IReadOnlyCollection<T>,
        IEnumerable<T>,
        IEnumerable
    {
	}
}