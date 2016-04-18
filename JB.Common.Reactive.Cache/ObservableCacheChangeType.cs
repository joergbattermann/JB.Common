// -----------------------------------------------------------------------
// <copyright file="ObservableCacheChangeType.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

namespace JB.Reactive.Cache
{
    /// <summary>
    /// Determines the possible cache changes.
    /// </summary>
    public enum ObservableCacheChangeType
    {
        /// <summary>
        ///     An item was added to the cache.
        /// </summary>
        ItemAdded,

        /// <summary>
        /// The property of a key has changed.
        /// </summary>
        ItemKeyChanged,

        /// <summary>
        ///     The property of a value has changed.
        /// </summary>
        ItemValueChanged,

        /// <summary>
        ///     The item for a key was replaced in the cache.
        /// </summary>
        ItemValueReplaced,

        /// <summary>
        ///     The item for a key has expired.
        ///     Depending on the <see cref="ObservableCacheExpirationType"/> for the affected element,
        ///     this may be followed by either an <see cref="ItemRemoved"/> or <see cref="ItemValueReplaced"/>.
        /// </summary>
        ItemExpired,

        /// <summary>
        ///     The item for a key has been removed.
        /// </summary>
        ItemRemoved,

        /// <summary>
        /// Indicates a major change and observers should, if required, reset any existing knowledge and state about the affected cache.
        /// </summary>
        Reset
    }
}