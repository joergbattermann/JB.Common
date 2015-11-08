using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;

namespace JB.Collections
{
    public interface INotifyReactiveCollectionChanged<T> : INotifyCollectionChanged
	{
		/// <summary>
		/// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		IDisposable SuppressCollectionChangedNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets a value indicating whether this instance is currently suppressing reactive collection changed notifications.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suppressing reactive collection changed notifications; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingCollectionChanges { get; }

        /// <summary>
		/// (Temporarily) suppresses change notifications for <see cref="ReactiveCollectionChangeType.ItemChanged"/> events until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled, if applicable.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to <typeparam name="T"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if the interface is implemented.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingItemChanges { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is tracking <see cref="ReactiveCollectionChangeType.Reset">resets</see>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingResets { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is tracking <see cref="IReadOnlyCollection{T}.Count"/> changes.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingCountChanges { get; }

        /// <summary>
		/// (Temporarily) suppresses change notifications for <see cref="ReactiveCollectionChangeType.Reset"/> events until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled, if applicable.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true);

        /// <summary>
		/// (Temporarily) suppresses item count change notification until the returned <see cref="IDisposable" />
		/// has been Disposed.
		/// </summary>
		/// <param name="signalCountWhenFinished">if set to <c>true</c> signals a the <see cref="IReadOnlyCollection{T}.Count"/> when finished.</param>
		/// <returns></returns>
        IDisposable SuppressCountChangeNotifications(bool signalCountWhenFinished = true);
        
        /// <summary>
        /// Gets the minimum amount of items that have been changed to be notified / considered a <see cref="ReactiveCollectionChangeType.Reset"/> rather than indivudal <see cref="ReactiveCollectionChangeType"/> notifications.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered reset.
        /// </value>
        int ThresholdOfItemChangesToNotifyAsReset { get; set; }

        /// <summary>
        /// Gets all collection change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The collection changes.
        /// </value>
        IObservable<IReactiveCollectionChange<T>> CollectionChanges { get; }

        /// <summary>
        /// Gets the observable streams of (<see cref="INotifyPropertyChanged"/> implementing) items inside this collection that have changed.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IReactiveCollectionChange<T>> ItemChanges { get; }

        /// <summary>
        /// Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The count changes.
        /// </value>
        IObservable<int> CountChanges { get; }

		/// <summary>
		/// Gets the reset notifications as an observable stream.  Whenever signaled,
		/// observers should reset any knowledge / state etc about the list.
		/// </summary>
		/// <value>
		/// The resets.
		/// </value>
		IObservable<Unit> Resets { get; }

        /// <summary>
        /// Gets the thrown exceptions for the <see cref="CollectionChanges"/> stream. Ugly, but oh well.
        /// </summary>
        /// <value>
        /// The thrown exceptions.
        /// </value>
        IObservable<Exception> ThrownExceptions { get; }

        /// <summary>
        /// Occurs when the corresponding <see cref="IReactiveCollection{T}"/> changed.
        /// </summary>
        event EventHandler<ReactiveCollectionChangedEventArgs<T>> ReactiveCollectionChanged;
	}
}