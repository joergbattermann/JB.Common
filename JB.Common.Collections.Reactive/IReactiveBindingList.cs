using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
	public interface IReactiveBindingList<T> : IReactiveList<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
    {
        /// <summary>
        /// Gets a value indicating whether this instance is currently notifying event and observable subscribers about collection changed events.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is notifying observable and event subscribers; otherwise, <c>false</c>.
        /// </value>
        bool RaiseListChangedEvents { get; }

        /// <summary>
        /// Raises <see cref="INotifyReactiveCollectionChanged{T}.ReactiveCollectionChanged"/>,  <see cref="INotifyCollectionChanged.CollectionChanged"/>
        /// and <see cref="IBindingList.ListChanged"/> event(s) as well as notifies the <see cref="INotifyReactiveCollectionChanged{T}.CollectionChanges"/>
        /// and <see cref="INotifyReactiveCollectionChanged{T}.Resets"/> subscribers signalling an entire List / Collection Reset.
        /// </summary>
        void ResetBindings();

        /// <summary>
        /// Raises <see cref="INotifyReactiveCollectionChanged{T}.ReactiveCollectionChanged"/>,  <see cref="INotifyCollectionChanged.CollectionChanged"/>
        /// and <see cref="IBindingList.ListChanged"/> event(s) as well as notifies the <see cref="INotifyReactiveCollectionChanged{T}.CollectionChanges"/>
        /// subscribers signalling a single item change event.
        /// </summary>
        /// <param name="index">A zero-based index position of the item to be reset.</param>
        void ResetItem(int index);
    }
}