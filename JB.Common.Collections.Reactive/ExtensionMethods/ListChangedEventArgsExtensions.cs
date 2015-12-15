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
        /// Converts the given <paramref name="listChangedEventArgs"/> and converts it to its <see cref="IObservableCollectionChange{T}"/> counterpart.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listChangedEventArgs">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservableCollectionChange<T> ToObservableCollectionChange<T>(this ListChangedEventArgs listChangedEventArgs, IEnhancedBindingList<T> sender)
        {
            if (listChangedEventArgs == null) throw new ArgumentNullException(nameof(listChangedEventArgs));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            IObservableCollectionChange<T> observableCollectionChange;
            var senderAsList = sender as IList<T>;

            switch (listChangedEventArgs.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    observableCollectionChange = new ObservableCollectionChange<T>(
                        ObservableCollectionChangeType.ItemAdded,
                        senderAsList[listChangedEventArgs.NewIndex]);
                    break;
                case ListChangedType.ItemChanged:
                    observableCollectionChange = new ObservableCollectionChange<T>(
                        ObservableCollectionChangeType.ItemChanged,
                        senderAsList[listChangedEventArgs.NewIndex]);
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        var itemDeletedListChangedEventArgs = (listChangedEventArgs as ItemDeletedListChangedEventArgs<T>);
                        observableCollectionChange = itemDeletedListChangedEventArgs != null
                            ? new ObservableCollectionChange<T>(ObservableCollectionChangeType.ItemRemoved, itemDeletedListChangedEventArgs.Item)
                            : new ObservableCollectionChange<T>(ObservableCollectionChangeType.ItemRemoved);

                        break;
                    }
                case ListChangedType.Reset:
                    observableCollectionChange = new ObservableCollectionChange<T>(ObservableCollectionChangeType.Reset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listChangedEventArgs),
                        $"Only {ListChangedType.ItemAdded}, {ListChangedType.ItemChanged}, {ListChangedType.ItemDeleted} and {ListChangedType.Reset} are supported.");

            }

            return observableCollectionChange;
        }

        /// <summary>
        /// Converts the given <paramref name="listChangedEventArgs"/> and converts it to its <see cref="IObservableListChange{T}"/> counterpart.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listChangedEventArgs">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservableListChange<T> ToObservableListChange<T>(this ListChangedEventArgs listChangedEventArgs, IEnhancedBindingList<T> sender)
        {
            if (listChangedEventArgs == null) throw new ArgumentNullException(nameof(listChangedEventArgs));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            IObservableListChange<T> observableListChange;
            var senderAsList = sender as IList<T>;

            switch (listChangedEventArgs.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    observableListChange = new ObservableListChange<T>(
                        ObservableListChangeType.ItemAdded,
                        senderAsList[listChangedEventArgs.NewIndex],
                        listChangedEventArgs.NewIndex);
                    break;
                case ListChangedType.ItemChanged:
                    observableListChange = new ObservableListChange<T>(
                        ObservableListChangeType.ItemChanged,
                        senderAsList[listChangedEventArgs.NewIndex],
                        listChangedEventArgs.NewIndex,
                        listChangedEventArgs.OldIndex);
                    break;
                case ListChangedType.ItemMoved:
                    observableListChange = new ObservableListChange<T>(
                        ObservableListChangeType.ItemMoved,
                        senderAsList[listChangedEventArgs.NewIndex],
                        listChangedEventArgs.NewIndex,
                        listChangedEventArgs.OldIndex);
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        var itemDeletedListChangedEventArgs = (listChangedEventArgs as ItemDeletedListChangedEventArgs<T>);
                        observableListChange = itemDeletedListChangedEventArgs != null
                            ? new ObservableListChange<T>(ObservableListChangeType.ItemRemoved, itemDeletedListChangedEventArgs.Item, listChangedEventArgs.NewIndex, listChangedEventArgs.OldIndex)
                            : new ObservableListChange<T>(ObservableListChangeType.ItemRemoved, default(T), -1, listChangedEventArgs.NewIndex);

                        break;
                    }
                case ListChangedType.Reset:
                    observableListChange = new ObservableListChange<T>(ObservableListChangeType.Reset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listChangedEventArgs),
                        $"Only {ListChangedType.ItemAdded}, {ListChangedType.ItemChanged}, {ListChangedType.ItemMoved}, {ListChangedType.ItemDeleted} and {ListChangedType.Reset} are supported.");

            }

            return observableListChange;
        }
    }
}