using System;
using System.Collections.Specialized;
using System.Reactive;

namespace JB.Collections
{
	public interface INotifyReactiveCollectionChanged<T> : INotifyCollectionChanged
	{
		IDisposable SuppressReactiveCollectionChangedNotifications();

		double ChangesToResetThreshold { get; }
		
		IObservable<IReactiveCollectionChange<T>> CollectionChanges { get; }
		
		IObservable<int> CountChanges { get; }

		IObservable<Unit> Resets { get; }

		
	}
}