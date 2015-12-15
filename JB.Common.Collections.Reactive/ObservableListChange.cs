using System;

namespace JB.Collections.Reactive
{
    public class ObservableListChange<T> : IObservableListChange<T>
    {
        /// <summary>
        /// The type is a value type.. or not.. let's find out, lazily.
        /// </summary>
        private static readonly Lazy<bool> TypeIsValueType = new Lazy<bool>(() => typeof(T).IsValueType);

        #region Implementation of IObservableListChange<out T>

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableListChangeType ChangeType { get; }

        /// <summary>
        /// Gets the item that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected item, if any.
        /// </value>
        public T Item { get; }

        /// <summary>
        /// Gets the new, post-change (starting) index for the <see cref="IObservableListChange{T}.Item"/>.
        /// </summary>
        /// <value>
        /// The post-change starting index, -1 for removals, otherwise 0 or greater.
        /// </value>
        public int Index { get; }

        /// <summary>
        /// Gets the previous, pre-change (starting) index for the <see cref="IObservableListChange{T}.Item"/>.
        /// </summary>
        /// <value>
        /// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
        /// </value>
        public int OldIndex { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListChange{T}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">New starting index, after the add, change or move, -1 of not applicable.</param>
        /// <param name="oldIndex">Old starting index, before the add, change or move, -1 of not applicable.</param>
        public ObservableListChange(ObservableListChangeType changeType, T item = default(T), int index = -1, int oldIndex = -1)
        {
            if (index < -1) throw new ArgumentOutOfRangeException(nameof(index), "Value cannot be less than -1");
            if (oldIndex < -1) throw new ArgumentOutOfRangeException(nameof(oldIndex), "Value cannot be less than -1");

            if (changeType == ObservableListChangeType.ItemAdded && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item adds must not have an {nameof(index)} of -1.");

            if (changeType == ObservableListChangeType.ItemAdded && oldIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item adds must have an {nameof(oldIndex)} of -1.");

            if (changeType == ObservableListChangeType.ItemRemoved && index != -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item removals must have an {nameof(index)} of -1.");

            if (changeType == ObservableListChangeType.ItemRemoved && oldIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item removals must nothave an {nameof(oldIndex)} of -1.");

            if (changeType == ObservableListChangeType.ItemMoved && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item moves must not have an {nameof(index)} of -1.");

            if (changeType == ObservableListChangeType.ItemMoved && oldIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Item moves must not have an {nameof(oldIndex)} of -1.");

            if (changeType == ObservableListChangeType.ItemChanged && index == -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item changes must not have an {nameof(index)} of -1 but the index of the changed item.");

            if (changeType == ObservableListChangeType.ItemChanged && oldIndex != index)
                throw new ArgumentOutOfRangeException(nameof(index), $"Item changes must have the same index position for both, {nameof(index)} and {nameof(oldIndex)}.");

            if (changeType == ObservableListChangeType.Reset && index != -1)
                throw new ArgumentOutOfRangeException(nameof(index), $"Resets must have an {nameof(index)} of -1.");

            if (changeType == ObservableListChangeType.Reset && oldIndex != -1)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Resets must have an {nameof(oldIndex)} of -1.");

            if (changeType == ObservableListChangeType.Reset && (TypeIsValueType.Value == false && !Equals(item, default(T))))
                throw new ArgumentOutOfRangeException(nameof(item), $"Resets must not have an {nameof(item)}");

            if ((changeType == ObservableListChangeType.ItemAdded
                 || changeType == ObservableListChangeType.ItemChanged
                 || changeType == ObservableListChangeType.ItemMoved)
                && (TypeIsValueType.Value == false && Equals(item, default(T))))
                throw new ArgumentOutOfRangeException(nameof(item), $"Item Adds, Moves and Changes MUST have have an {nameof(item)}, Removes SHOULD have one, if available.");

            ChangeType = changeType;
            Item = item;

            Index = index;
            OldIndex = oldIndex;
        }

        /// <summary>
        /// Gets a <see cref="IObservableListChange{T}"/> representing a <see cref="ObservableListChangeType.Reset"/>.
        /// </summary>
        /// <value>
        /// The reset change type.
        /// </value>
        public static IObservableListChange<T> Reset => new ObservableListChange<T>(ObservableListChangeType.Reset);
    }
}