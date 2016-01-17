using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="CollectionItemChanges">observable stream</see> of item changes IF
    /// <typeparam name="TItem"/> implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface INotifyObservableCollectionItemChanges<out TItem> : INotifyObservableItemChanges
    {
        /// <summary>
        /// Gets the observable streams of collection item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IObservableCollectionChange<TItem>> CollectionItemChanges { get; }
    }
}