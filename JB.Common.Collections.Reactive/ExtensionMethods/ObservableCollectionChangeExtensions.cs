// -----------------------------------------------------------------------
// <copyright file="ObservableCollectionChangeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Specialized;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableCollectionChange{T}"/> instances.
    /// </summary>
    public static class ObservableCollectionChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="observableCollectionChange"/> to its <see cref="NotifyCollectionChangedEventArgs"/> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observableCollectionChange">The <see cref="IObservableCollectionChange{T}"/> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static NotifyCollectionChangedEventArgs ToNotifyCollectionChangedEventArgs<T>(this IObservableCollectionChange<T> observableCollectionChange)
        {
            if (observableCollectionChange == null) throw new ArgumentNullException(nameof(observableCollectionChange));

            switch (observableCollectionChange.ChangeType)
            {
                case ObservableCollectionChangeType.ItemAdded:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, observableCollectionChange.Item);
                case ObservableCollectionChangeType.ItemChanged:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, observableCollectionChange.Item, observableCollectionChange.Item);
                case ObservableCollectionChangeType.ItemRemoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, observableCollectionChange.Item);
                case ObservableCollectionChangeType.Reset:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableCollectionChange), "This should not happen.");
            }
        }
    }
}