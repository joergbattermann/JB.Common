using System;

namespace JB.Collections.Reactive
{
    public class ObservableListChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the observable list change data.
        /// </summary>
        /// <value>
        /// The observable list change data.
        /// </value>
        public IObservableListChange<T> Change { get; }

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableListChangeType ChangeType => Change.ChangeType;

        /// <summary>
        /// Gets the item that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected item, if any.
        /// </value>
        public T Item => Change.Item;

        /// <summary>
        /// Gets the new, post-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The post-change starting index, -1 for removals, otherwise 0 or greater.
        /// </value>
        public int Index => Change.Index;

        /// <summary>
        /// Gets the previous, pre-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
        /// </value>
        public int OldIndex => Change.OldIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableListChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="observableListChange">The observable list change.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ObservableListChangedEventArgs(IObservableListChange<T> observableListChange)
        {
            if (observableListChange == null)
                throw new ArgumentNullException(nameof(observableListChange));

            Change = observableListChange;
        }
    }
}