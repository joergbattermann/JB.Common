// -----------------------------------------------------------------------
// <copyright file="INotifyObservableCacheChanges.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
using System;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache
{
    /// <summary>
    /// Classes implementing this interface provide an observable stream of <see cref="Changes"/>.
    /// </summary>
    public interface INotifyObservableCacheChanges<out TKey, out TValue> : INotifyObservableChanges
    {
        /// <summary>
        ///     Gets an observable stream of changes to the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <value>
        ///     The changes.
        /// </value>
        IObservable<IObservableCacheChange<TKey, TValue>> Changes { get; }        
    }
}