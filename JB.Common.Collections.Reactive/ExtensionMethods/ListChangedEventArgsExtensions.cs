// -----------------------------------------------------------------------
// <copyright file="ListChangedEventArgsExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JB.Collections.Reactive.ExtensionMethods
{
    public static class ListChangedEventArgsExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="listChangedEventArgs"/> and converts it to its <see cref="IReactiveCollectionChange{T}"/> counterpart..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listChangedEventArgs">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IReactiveCollectionChange<T> ToReactiveCollectionChange<T>(this ListChangedEventArgs listChangedEventArgs, IList<T> sender)
        {
            if (listChangedEventArgs == null) throw new ArgumentNullException(nameof(listChangedEventArgs));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            IReactiveCollectionChange<T> reactiveCollectionChange;
            switch (listChangedEventArgs.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    reactiveCollectionChange = new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.ItemAdded, sender[listChangedEventArgs.NewIndex], listChangedEventArgs.NewIndex);
                    break;
                case ListChangedType.ItemChanged:
                    reactiveCollectionChange = new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.ItemChanged, sender[listChangedEventArgs.NewIndex], listChangedEventArgs.NewIndex, listChangedEventArgs.OldIndex);
                    break;
                case ListChangedType.ItemMoved:
                    reactiveCollectionChange = new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.ItemMoved, sender[listChangedEventArgs.NewIndex], listChangedEventArgs.NewIndex, listChangedEventArgs.OldIndex);
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        var itemDeletedListChangedEventArgs = (listChangedEventArgs as ItemDeletedListChangedEventArgs<T>);
                        reactiveCollectionChange = itemDeletedListChangedEventArgs != null
                            ? new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.ItemRemoved, itemDeletedListChangedEventArgs.Item, listChangedEventArgs.NewIndex, listChangedEventArgs.OldIndex)
                            : new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.ItemRemoved, default(T), -1, listChangedEventArgs.NewIndex);

                        break;
                    }
                case ListChangedType.Reset:
                    reactiveCollectionChange = new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.Reset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listChangedEventArgs), $"Only {ListChangedType.ItemAdded}, {ListChangedType.ItemChanged}, {ListChangedType.ItemMoved}, {ListChangedType.ItemDeleted} and {ListChangedType.Reset} are supported.");

            }

            return reactiveCollectionChange;
        }
    }
}