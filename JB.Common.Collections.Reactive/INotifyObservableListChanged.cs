using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableListChanged<T> : INotifyObservableChanges
    {
        /// <summary>
        /// Gets the collection changes as an observable stream.
        /// </summary>
        /// <value>
        /// The collection changes.
        /// </value>
        IObservable<IObservableListChange<T>> ListChanges { get; }

        /// <summary>
        /// Occurs when the corresponding <see cref="IObservableList{T}"/> changed.
        /// </summary>
        event EventHandler<ObservableListChangedEventArgs<T>> ObservableListChanged;
    }
}