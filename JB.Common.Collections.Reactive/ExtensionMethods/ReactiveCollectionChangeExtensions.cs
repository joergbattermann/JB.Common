// -----------------------------------------------------------------------
// <copyright file="ReactiveCollectionChangeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IReactiveCollectionChange{T}"/> instances.
    /// </summary>
    public static class ReactiveCollectionChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="reactiveCollectionChange"/> to its <see cref="ListChangedEventArgs"/> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reactiveCollectionChange">The <see cref="IReactiveCollectionChange{T}"/> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ListChangedEventArgs ToListChangedEventArgs<T>(this IReactiveCollectionChange<T> reactiveCollectionChange)
        {
            if (reactiveCollectionChange == null) throw new ArgumentNullException(nameof(reactiveCollectionChange));

            switch (reactiveCollectionChange.ChangeType)
            {
                    case ReactiveCollectionChangeType.ItemAdded:
                        return new ListChangedEventArgs(ListChangedType.ItemAdded, reactiveCollectionChange.Index);
                    case ReactiveCollectionChangeType.ItemChanged:
                        return new ListChangedEventArgs(ListChangedType.ItemChanged, reactiveCollectionChange.Index, reactiveCollectionChange.OldIndex);
                    case ReactiveCollectionChangeType.ItemMoved:
                        return new ListChangedEventArgs(ListChangedType.ItemMoved, reactiveCollectionChange.Index, reactiveCollectionChange.OldIndex);
                    case ReactiveCollectionChangeType.ItemRemoved:
                        return new ListChangedEventArgs(ListChangedType.ItemDeleted, reactiveCollectionChange.Index, reactiveCollectionChange.OldIndex);
                    case ReactiveCollectionChangeType.Reset:
                        return new ListChangedEventArgs(ListChangedType.Reset, reactiveCollectionChange.Index);
                default:
                    throw new ArgumentOutOfRangeException(nameof(reactiveCollectionChange), "This should not happen.");
            }
        }

        /// <summary>
        /// Converts the given <paramref name="reactiveCollectionChange"/> to its <see cref="NotifyCollectionChangedEventArgs"/> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reactiveCollectionChange">The <see cref="IReactiveCollectionChange{T}"/> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static NotifyCollectionChangedEventArgs ToNotifyCollectionChangedEventArgs<T>(this IReactiveCollectionChange<T> reactiveCollectionChange)
        {
            if (reactiveCollectionChange == null) throw new ArgumentNullException(nameof(reactiveCollectionChange));

            switch (reactiveCollectionChange.ChangeType)
            {
                case ReactiveCollectionChangeType.ItemAdded:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, reactiveCollectionChange.Item, reactiveCollectionChange.Index);
                case ReactiveCollectionChangeType.ItemChanged:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, reactiveCollectionChange.Item, reactiveCollectionChange.Index);
                case ReactiveCollectionChangeType.ItemMoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, reactiveCollectionChange.Item, reactiveCollectionChange.Index, reactiveCollectionChange.OldIndex);
                case ReactiveCollectionChangeType.ItemRemoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, reactiveCollectionChange.Item, reactiveCollectionChange.Index, reactiveCollectionChange.OldIndex);
                case ReactiveCollectionChangeType.Reset:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(reactiveCollectionChange), "This should not happen.");
            }
        }
    }
}