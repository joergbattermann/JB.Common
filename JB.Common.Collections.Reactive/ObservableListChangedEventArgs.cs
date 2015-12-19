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