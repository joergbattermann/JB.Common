using System;
using System.ComponentModel;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="ValueChanges">observable stream</see> of item changes IF
    /// <typeparam name="TKey" /> and/or <typeparam name="TValue" /> implement the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableCacheItemChanges<out TKey, out TValue> : INotifyObservableItemChanges
    {
        /// <summary>
        /// Gets the observable streams of cached items' value changes or value replacements.
        /// </summary>
        /// <value>
        /// The items' value changes.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> ValueChanges { get; }

        /// <summary>
        /// Gets the observable streams of cached items' value changes.
        /// </summary>
        /// <value>
        /// The items' value changes.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> KeyChanges { get; }
    }
}