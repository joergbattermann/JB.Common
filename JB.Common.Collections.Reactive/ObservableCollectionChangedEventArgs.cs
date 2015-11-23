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
        public IObservableCollectionChange<T> ObservableCollectionChange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="observableCollectionChange">The observable collection change.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ObservableCollectionChangedEventArgs(IObservableCollectionChange<T> observableCollectionChange)
        {
            if (observableCollectionChange == null)
                throw new ArgumentNullException(nameof(observableCollectionChange));

            ObservableCollectionChange = observableCollectionChange;
        }
    }
}