using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive;

namespace JB.Collections.Reactive
{
    public interface INotifyReactiveCollectionChanged<T> : INotifyCollectionChanged, IProvideReactiveCollectionItemChangedObservation<T>
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
        /// Gets a value indicating whether this instance is tracking <see cref="ReactiveCollectionChangeType.Reset">resets</see>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingResets { get; }

        /// <summary>
		/// (Temporarily) suppresses change notifications for <see cref="ReactiveCollectionChangeType.Reset"/> events until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled, if applicable.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets all collection change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The collection changes.
        /// </value>
        IObservable<IReactiveCollectionChange<T>> CollectionChanges { get; }

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