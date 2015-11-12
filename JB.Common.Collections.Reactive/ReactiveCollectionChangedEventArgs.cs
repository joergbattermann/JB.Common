using System;

namespace JB.Collections.Reactive
{
    public class ReactiveCollectionChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the reactive collection change data.
        /// </summary>
        /// <value>
        /// The reactive collection change data.
        /// </value>
        public IReactiveCollectionChange<T> ReactiveCollectionChange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveCollectionChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="reactiveCollectionChange">The reactive collection change.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ReactiveCollectionChangedEventArgs(IReactiveCollectionChange<T> reactiveCollectionChange)
        {
            if (reactiveCollectionChange == null) throw new ArgumentNullException(nameof(reactiveCollectionChange));

            ReactiveCollectionChange = reactiveCollectionChange;
        }

    }
}