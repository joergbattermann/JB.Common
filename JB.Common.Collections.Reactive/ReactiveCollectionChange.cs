// -----------------------------------------------------------------------
// <copyright file="ReactiveCollectionChange.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB.Collections.Reactive
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
        public int Index { get; }

        /// <summary>
        /// Gets the previous, pre-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
        /// </value>
        public int OldIndex { get; }

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
        /// <param name="index">New starting index, after the add, change or move, -1 of not applicable.</param>
        /// <param name="oldIndex">Old starting index, before the add, change or move, -1 of not applicable.</param>
        public ReactiveCollectionChange(ReactiveCollectionChangeType changeType, T item = default(T), int index = -1, int oldIndex = -1)
        {
            if (index < -1) throw new ArgumentOutOfRangeException(nameof(index), "Value cannot be less than -1");
            if (oldIndex < -1) throw new ArgumentOutOfRangeException(nameof(oldIndex), "Value cannot be less than -1");

            if (changeType == ReactiveCollectionChangeType.ItemAdded && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item adds must not have an {nameof(index)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemAdded && oldIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item adds must have an {nameof(oldIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemRemoved && index != -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item removals must have an {nameof(index)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemRemoved && oldIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item removals must nothave an {nameof(oldIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemMoved && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item moves must not have an {nameof(index)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemMoved && oldIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item moves must not have an {nameof(oldIndex)} of -1.");

            if (changeType == ReactiveCollectionChangeType.ItemChanged && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item changes must not have an {nameof(index)} of -1 but the index of the changed item.");

            if (changeType == ReactiveCollectionChangeType.ItemChanged && oldIndex != index)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item changes must have the same index position for both, {nameof(index)} and {nameof(oldIndex)}.");

            if (changeType == ReactiveCollectionChangeType.Reset && index != -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Resets must have an {nameof(index)} of -1.");

            if (changeType == ReactiveCollectionChangeType.Reset && oldIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Resets must have an {nameof(oldIndex)} of -1.");

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

            Index = index;
            OldIndex = oldIndex;
        }

        /// <summary>
        /// Gets a <see cref="IReactiveCollectionChange{T}"/> representing a <see cref="ReactiveCollectionChangeType.Reset"/>.
        /// </summary>
        /// <value>
        /// The reset.
        /// </value>
        public static IReactiveCollectionChange<T> Reset => new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.Reset);
    }
}