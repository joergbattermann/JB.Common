using System;

namespace JB.Collections.Reactive
{
    public class ObservableDictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {
        /// <summary>
        /// Gets the observable dictionary change data.
        /// </summary>
        /// <value>
        /// The observable dictionary change data.
        /// </value>
        public IObservableDictionaryChange<TKey, TValue> Change { get; }

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableDictionaryChangeType ChangeType => Change.ChangeType;

        /// <summary>
        /// Gets the key of the (changed) item.
        /// </summary>
        /// <value>
        /// The key of the (changed) item.
        /// </value>
        public TKey Key => Change.Key;

        /// <summary>
        /// Gets the value that was added, changed or removed.
        /// </summary>
        /// <value>
        /// The affected value, if any.
        /// </value>
        public TValue Value => Change.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionaryChangedEventArgs{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ObservableDictionaryChangedEventArgs(IObservableDictionaryChange<TKey, TValue> observableDictionaryChange)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));

            Change = observableDictionaryChange;
        }
    }
}