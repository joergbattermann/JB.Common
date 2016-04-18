// -----------------------------------------------------------------------
// <copyright file="ObservableCacheExpirationType.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
namespace JB.Reactive.Cache
{
    /// <summary>
    /// Determines how cached items are handled upon expiration.
    /// </summary>
    public enum ObservableCacheExpirationType
    {
        /// <summary>
        /// Everything remains as-is.
        /// </summary>
        DoNothing,
        /// <summary>
        /// The item will be removed upon expiration.
        /// </summary>
        Remove,
        /// <summary>
        /// The item will be updated upon expiration.
        /// </summary>
        Update
    }
}