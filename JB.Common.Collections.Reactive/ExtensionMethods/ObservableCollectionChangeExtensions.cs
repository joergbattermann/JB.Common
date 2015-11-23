// -----------------------------------------------------------------------
// <copyright file="ObservableCollectionChangeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableCollectionChange{T}"/> instances.
    /// </summary>
    public static class ObservableCollectionChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="observableCollectionChange"/> to its <see cref="ListChangedEventArgs"/> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observableCollectionChange">The <see cref="IObservableCollectionChange{T}"/> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ListChangedEventArgs ToListChangedEventArgs<T>(this IObservableCollectionChange<T> observableCollectionChange)
        {
            if (observableCollectionChange == null)
                throw new ArgumentNullException(nameof(observableCollectionChange));

            switch (observableCollectionChange.ChangeType)
            {
                    case ObservableCollectionChangeType.ItemAdded:
                        return new ListChangedEventArgs(ListChangedType.ItemAdded, observableCollectionChange.Index);
                    case ObservableCollectionChangeType.ItemChanged:
                        return new ListChangedEventArgs(ListChangedType.ItemChanged, observableCollectionChange.Index, observableCollectionChange.OldIndex);
                    case ObservableCollectionChangeType.ItemMoved:
                        return new ListChangedEventArgs(ListChangedType.ItemMoved, observableCollectionChange.Index, observableCollectionChange.OldIndex);
                    case ObservableCollectionChangeType.ItemRemoved:
                        return new ListChangedEventArgs(ListChangedType.ItemDeleted, observableCollectionChange.Index, observableCollectionChange.OldIndex);
                    case ObservableCollectionChangeType.Reset:
                        return new ListChangedEventArgs(ListChangedType.Reset, observableCollectionChange.Index);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableCollectionChange), "This should not happen.");
            }
        }

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
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, observableCollectionChange.Item, observableCollectionChange.Index);
                case ObservableCollectionChangeType.ItemChanged:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, observableCollectionChange.Item, observableCollectionChange.Index);
                case ObservableCollectionChangeType.ItemMoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, observableCollectionChange.Item, observableCollectionChange.Index, observableCollectionChange.OldIndex);
                case ObservableCollectionChangeType.ItemRemoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, observableCollectionChange.Item, observableCollectionChange.Index, observableCollectionChange.OldIndex);
                case ObservableCollectionChangeType.Reset:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableCollectionChange), "This should not happen.");
            }
        }
    }
}