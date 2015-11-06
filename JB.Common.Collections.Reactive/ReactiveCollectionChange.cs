// -----------------------------------------------------------------------
// <copyright file="ReactiveCollectionChange.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB.Collections
{
    public class ReactiveCollectionChange<T> : IReactiveCollectionChange<T>
    {
        /// <summary>
        /// The type is a value type.. or not.. let's find out, lazily.
        /// </summary>
        private static readonly Lazy<bool> TypeIsValueType = new Lazy<bool>(() => typeof(T).IsValueType);

        #region Implementation of IReactiveCollectionChange<out T>

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ReactiveCollectionChangeType ChangeType { get; }

        /// <summary>
        /// Gets the new, post-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The post-change starting index, -1 for removals, otherwise 0 or greater.
        /// </value>
        public int NewStartingIndex { get; }

        /// <summary>
        /// Gets the previous, pre-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
        /// </value>
        public int OldStartingIndex { get; }

        /// <summary>
        /// Gets the items that were changed or removed.
        /// </summary>
        /// <value>
        /// The affected items.
        /// </value>
        public T Item { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveCollectionChange{T}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="item">The item.</param>
        /// <param name="newStartingIndex">New starting index, after the add, change or move, -1 of not applicable.</param>
        /// <param name="oldStartingIndex">Old starting index, before the add, change or move, -1 of not applicable.</param>
        public ReactiveCollectionChange(ReactiveCollectionChangeType changeType, T item = default(T), int newStartingIndex = -1, int oldStartingIndex = -1)
        {
            if (newStartingIndex < -1) throw new ArgumentOutOfRangeException(nameof(newStartingIndex), "Value cannot be less than -1");
            if (oldStartingIndex < -1) throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), "Value cannot be less than -1");

            if (changeType == ReactiveCollectionChangeType.ItemAdded && newStartingIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(newStartingIndex), $"Item adds must not have an {nameof(newStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemAdded && oldStartingIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), $"Item adds must have an {nameof(oldStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemRemoved && newStartingIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(newStartingIndex), $"Item adds must have an {nameof(newStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemRemoved && oldStartingIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), $"Item adds must nothave an {nameof(oldStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemMoved && newStartingIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(newStartingIndex), $"Item moves must not have an {nameof(newStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemMoved && oldStartingIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), $"Item moves must not have an {nameof(oldStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemChanged && newStartingIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(newStartingIndex), $"Item changes must not have an {nameof(newStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemChanged && oldStartingIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), $"Item changes must have an {nameof(oldStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.Reset && newStartingIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(newStartingIndex), $"Resets must have an {nameof(newStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.Reset && oldStartingIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldStartingIndex), $"Resets must have an {nameof(oldStartingIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.Reset && (TypeIsValueType.Value == false && !Equals(item, default(T))))
                throw new ArgumentOutOfRangeException(nameof(item), $"Resets must not have an {nameof(item)}");

            if ((changeType == ReactiveCollectionChangeType.ItemAdded
                || changeType == ReactiveCollectionChangeType.ItemChanged
                || changeType == ReactiveCollectionChangeType.ItemMoved
                || changeType == ReactiveCollectionChangeType.ItemRemoved)
                && (TypeIsValueType.Value == false && Equals(item, default(T))))
                throw new ArgumentOutOfRangeException(nameof(item), $"Item Adds, Changes, Moves and Removes must have an {nameof(item)}");

            ChangeType = changeType;
            Item = item;

            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
        }
    }
}