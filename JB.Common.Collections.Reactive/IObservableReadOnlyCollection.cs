using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using JB.Reactive;

namespace JB.Collections.Reactive
{
	public interface IObservableReadOnlyCollection<out T> :
        INotifyObservableCollectionItemChanges<T>,
        INotifyObservableCountChanges,
        INotifyCollectionChanged,
        INotifyObservableResets,
        INotifyObserverExceptions,
        INotifyPropertyChanged,
        IReadOnlyCollection<T>,
        IEnumerable<T>,
        IEnumerable
    {
	}
}