using System;
using System.Collections.Specialized;
using System.Reactive;

namespace JB.Collections
{
	public interface INotifyReactiveCollectionChanged<out T> : INotifyCollectionChanged
	{
		/// <summary>
		/// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		IDisposable SuppressReactiveCollectionChangedNotifications(bool signalResetWhenFinished = true);

		/// <summary>
		/// Indicates at what percentage / fraction bulk changes are signaled as a Reset rather than individual change()s.
		/// [0] = Always, [1] = Never.
		/// </summary>
		/// <value>
		/// The changes to reset threshold.
		/// </value>
		double ChangesToResetThreshold { get; }

		/// <summary>
		/// Gets the collection change notifications as an observable stream.
		/// </summary>
		/// <value>
		/// The collection changes.
		/// </value>
		IObservable<IReactiveCollectionChange<T>> CollectionChanges { get; }

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
	}
}