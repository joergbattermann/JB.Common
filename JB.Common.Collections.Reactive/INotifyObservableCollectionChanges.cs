using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableCollectionChanges<T> : INotifyObservableChanges
    {
        /// <summary>
        /// Gets the collection changes as an observable stream.
        /// </summary>
        /// <value>
        /// The collection changes.
        /// </value>
        IObservable<IObservableCollectionChange<T>> CollectionChanges { get; }
	}
}