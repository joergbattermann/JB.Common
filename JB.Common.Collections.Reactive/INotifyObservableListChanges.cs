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
    }
}