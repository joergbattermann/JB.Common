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