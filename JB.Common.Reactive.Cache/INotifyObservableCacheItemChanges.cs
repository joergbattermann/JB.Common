using System;
using System.ComponentModel;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="ItemChanges">observable stream</see> of item changes IF
    /// <typeparam name="TValue" /> implements the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableCacheItemChanges<out TKey, out TValue> : INotifyObservableItemChanges
    {
        /// <summary>
        /// Gets the observable streams of collection item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> ItemChanges { get; }
    }
}