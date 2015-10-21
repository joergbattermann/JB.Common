// -----------------------------------------------------------------------
// <copyright file="SchedulerSynchronizedBindingList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>A synchronized (list acitivities AND events raised) version of BindingList.</summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace JB.Collections
{
	/// <summary>
	///     An <see cref="IBindingList" /> implementation that's synchronized via an underlying lock-based
	///     <see cref="IList{T}" />
	///     implementation, but also ensuring all events are raised on the same initially created or provided
	///     <see cref="IScheduler" />.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class SchedulerSynchronizedBindingList<T> : EnhancedBindingList<T>
	{
		private readonly IScheduler _scheduler;

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
			: base(syncRoot == null ? new SynchronizedCollection<T>(list ?? new List<T>()) : new SynchronizedCollection<T>(syncRoot, list ?? new List<T>()))
		{
			_scheduler = scheduler ?? Scheduler.Default;
		}
		
		#region Overrides of BindingList<T>

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.AddingNew" /> event.
		/// </summary>
		/// <param name="addingNewEventArgs">An <see cref="T:System.ComponentModel.AddingNewEventArgs" /> that contains the event data. </param>
		protected override void OnAddingNew(AddingNewEventArgs addingNewEventArgs)
		{
			_scheduler.Schedule(() => base.OnAddingNew(addingNewEventArgs));
		}

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event.
		/// </summary>
		/// <param name="listChangedEventArgs">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
		protected override void OnListChanged(ListChangedEventArgs listChangedEventArgs)
		{
			_scheduler.Schedule(() => base.OnListChanged(listChangedEventArgs));
		}

		#endregion
	}
}