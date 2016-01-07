// -----------------------------------------------------------------------
// <copyright file="ObservableCacheChangeType.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

namespace JB.Reactive.Cache
{
    public enum ObservableCacheChangeType
    {
        /// <summary>
        ///     An item was added to the cache.
        /// </summary>
        ItemAdded,

        /// <summary>
        ///     The item for a key has had one of its value(s) changed.
        /// </summary>
        ItemChanged,

        /// <summary>
        ///     The item for a key was replaced in the cache.
        /// </summary>
        ItemReplaced,

        /// <summary>
        ///     The item for a key has expired.
        /// </summary>
        ItemExpired,

        /// <summary>
        ///     The item for a key has been removed.
        /// </summary>
        ItemRemoved
    }
}