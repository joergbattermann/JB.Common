using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace JB.Collections.Reactive
{
	/// <summary>
	///     An <see cref="IBindingList" /> implementation that's synchronized via an underlying lock-based
	///     <see cref="IList{T}" />
	///     implementation, but also ensuring all events are raised on the same initially created or provided
	///     <see cref="IScheduler" />.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class SchedulerSynchronizedBindingList<T> : SchedulerCoordinatedBindingList<T>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SchedulerSynchronizedBindingList{T}" /> class.
		/// </summary>
		/// <param name="list">
		///     An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the
		///     <see cref="T:System.ComponentModel.BindingList`1" />.
		/// </param>
		/// <param name="scheduler">The scheduler.</param>
		/// <param name="syncRoot">The object used to synchronize access the thread-safe collection.</param>
		public SchedulerSynchronizedBindingList(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
			: base(syncRoot == null ? new SynchronizedCollection<T>(list ?? new List<T>()) : new SynchronizedCollection<T>(syncRoot, list ?? new List<T>()), scheduler)
		{
		}
	}
}