using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IObservableCollection<T> :
        IObservableReadOnlyCollection<T>,
        INotifyObservableCollectionChanged<T>,
        IBulkModifiableCollection<T>,
        IReadOnlyCollection<T>,
        ICollection<T>,
        ICollection,
        IEnumerable<T>,
        IEnumerable,
        INotifyCollectionChanged,
        INotifyPropertyChanged
	{
		/// <summary>
		/// Resets this instance and signals subscribers / binding consumers accordingly.
		/// </summary>
		void Reset();
    }
}