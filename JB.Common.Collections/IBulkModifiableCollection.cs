// -----------------------------------------------------------------------
// <copyright file="IBulkModifiableCollection.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace JB.Collections
{
    /// <summary>
    /// A <see cref="ICollection{T}"/> that provides bulk add- and remove modifications.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public interface IBulkModifiableCollection<T> : ICollection<T>, IBulkModifiable<T>
    {
    }
}