using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IObservableCollection<T> :
        IObservableReadOnlyCollection<T>,
        INotifyObservableCollectionChanges<T>,
        IBulkModifiableCollection<T>,
        IReadOnlyCollection<T>,
        ICollection<T>,
        IEnumerable<T>,
        IEnumerable,
        INotifyCollectionChanged,
        INotifyPropertyChanged
	{
		/// <summary>
		/// Signals subscribers that they should reset their and local state about this instance by
		/// signaling a <see cref="ObservableCollectionChangeType.Reset"/> message and event.
		/// </summary>
		void Reset();
    }
}