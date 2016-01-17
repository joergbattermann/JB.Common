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

        /// <summary>
        /// Occurs when the corresponding <see cref="IObservableCollection{T}"/> changed.
        /// </summary>
        [Obsolete("This will/shall be removed again, soon")]
        event EventHandler<ObservableCollectionChangedEventArgs<T>> CollectionChanged;
	}
}