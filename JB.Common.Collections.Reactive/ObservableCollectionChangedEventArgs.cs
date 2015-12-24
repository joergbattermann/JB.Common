using System;

namespace JB.Collections.Reactive
{
    public class ObservableCollectionChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the observable collection change data.
        /// </summary>
        /// <value>
        /// The observable collection change data.
        /// </value>
        public IObservableCollectionChange<T> Change { get; }

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableCollectionChangeType ChangeType => Change.ChangeType;

        /// <summary>
        /// Gets the item that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected item, if any.
        /// </value>
        public T Item => Change.Item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="observableCollectionChange">The observable collection change.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ObservableCollectionChangedEventArgs(IObservableCollectionChange<T> observableCollectionChange)
        {
            if (observableCollectionChange == null)
                throw new ArgumentNullException(nameof(observableCollectionChange));

            Change = observableCollectionChange;
        }
    }
}