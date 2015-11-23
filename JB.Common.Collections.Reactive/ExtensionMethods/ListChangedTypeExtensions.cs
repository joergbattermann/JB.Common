// -----------------------------------------------------------------------
// <copyright file="ListChangedTypeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="ListChangedType"/>.
    /// </summary>
    public static class ListChangedTypeExtensions
    {
        /// <summary>
        /// Converts the <see cref="ListChangedType"/> to its <see cref="ObservableCollectionChangeType"/> counterpart.
        /// </summary>
        /// <param name="listChangedType">Type of the list changed.</param>
        /// <returns></returns>
        public static ObservableCollectionChangeType ToObservableCollectionChangeType(this ListChangedType listChangedType)
        {
            switch (listChangedType)
            {
                case ListChangedType.ItemAdded:
                    return ObservableCollectionChangeType.ItemAdded;
                case ListChangedType.ItemChanged:
                    return ObservableCollectionChangeType.ItemChanged;
                case ListChangedType.ItemMoved:
                    return ObservableCollectionChangeType.ItemMoved;
                case ListChangedType.ItemDeleted:
                    return ObservableCollectionChangeType.ItemRemoved;
                case ListChangedType.Reset:
                    return ObservableCollectionChangeType.Reset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listChangedType), $"Only {nameof(ListChangedType.ItemAdded)}, {nameof(ListChangedType.ItemChanged)}, {nameof(ListChangedType.ItemMoved)}, {nameof(ListChangedType.ItemDeleted)} and finally {nameof(ListChangedType.Reset)} are supported.");
            }
        }
    }
}