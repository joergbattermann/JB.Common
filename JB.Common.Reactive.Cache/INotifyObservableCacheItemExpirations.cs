using System;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="ItemExpirations">observable stream</see> of items
    /// that signaled <see cref="ObservableCacheChangeType.ItemExpired"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableCacheItemExpirations<out TKey, out TValue>
    {
        /// <summary>
        /// Gets the observable streams of item expirations.
        /// </summary>
        /// <value>
        /// The item item expirations.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> ItemExpirations { get; }
    }
}