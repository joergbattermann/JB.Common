// -----------------------------------------------------------------------
// <copyright file="ListChangedTypeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace JB.Collections.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="ListChangedType"/>.
    /// </summary>
    public static class ListChangedTypeExtensions
    {
        /// <summary>
        /// Converts the <see cref="ListChangedType"/> to its <see cref="ReactiveCollectionChangeType"/> counterpart.
        /// </summary>
        /// <param name="listChangedType">Type of the list changed.</param>
        /// <returns></returns>
        public static ReactiveCollectionChangeType ToReactiveCollectionChangeType(this ListChangedType listChangedType)
        {
            switch (listChangedType)
            {
                case ListChangedType.ItemAdded:
                    return ReactiveCollectionChangeType.ItemAdded;
                case ListChangedType.ItemChanged:
                    return ReactiveCollectionChangeType.ItemChanged;
                case ListChangedType.ItemMoved:
                    return ReactiveCollectionChangeType.ItemMoved;
                case ListChangedType.ItemDeleted:
                    return ReactiveCollectionChangeType.ItemRemoved;
                case ListChangedType.Reset:
                    return ReactiveCollectionChangeType.Reset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listChangedType), $"Only {nameof(ListChangedType.ItemAdded)}, {nameof(ListChangedType.ItemChanged)}, {nameof(ListChangedType.ItemMoved)}, {nameof(ListChangedType.ItemDeleted)} and finally {nameof(ListChangedType.Reset)} are supported.");
            }
        }
    }
}