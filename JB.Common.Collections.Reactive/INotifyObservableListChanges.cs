using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableListChanges<T> : INotifyObservableChanges
    {
        /// <summary>
        /// Gets the list changes as an observable stream.
        /// </summary>
        /// <value>
        /// The list changes.
        /// </value>
        IObservable<IObservableListChange<T>> ListChanges { get; }

        /// <summary>
        /// Occurs when the corresponding <see cref="IObservableList{T}"/> changed.
        /// </summary>
        [Obsolete("This shall be removed pre 1.0")]
        event EventHandler<ObservableListChangedEventArgs<T>> ObservableListChanged;
    }
}