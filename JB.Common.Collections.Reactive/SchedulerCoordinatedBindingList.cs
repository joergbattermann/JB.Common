// -----------------------------------------------------------------------
// <copyright file="SchedulerCoordinatedBindingList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>A Binding List that makes sure its events are raised on the proper Scheduler.</summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace JB.Collections.Reactive
{
	/// <summary>
	///     An <see cref="IBindingList" /> implementation that's raising its <see cref="BindingList{T}.AddingNew"/> and <see cref="BindingList{T}.ListChanged"/> events
	///		on the provided or <see cref="System.Reactive.Concurrency.Scheduler.Default">constructor created</see> <see cref="IScheduler" />.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class SchedulerCoordinatedBindingList<T> : EnhancedBindingList<T>
	{
		/// <summary>
		/// Gets the scheduler.
		/// </summary>
		/// <value>
		/// The scheduler.
		/// </value>
		protected IScheduler Scheduler { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="SchedulerCoordinatedBindingList{T}" /> class.
		/// </summary>
		/// <param name="list">
		///     An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the
		///     <see cref="T:System.ComponentModel.BindingList`1" />.
		/// </param>
		/// <param name="scheduler">The scheduler.</param>
		public SchedulerCoordinatedBindingList(IList<T> list = null, IScheduler scheduler = null)
			: base(list)
		{
			Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.Default;
		}
		
		#region Overrides of BindingList<T>

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.AddingNew" /> event.
		/// </summary>
		/// <param name="addingNewEventArgs">An <see cref="T:System.ComponentModel.AddingNewEventArgs" /> that contains the event data. </param>
		protected override void OnAddingNew(AddingNewEventArgs addingNewEventArgs)
		{
			Scheduler.Schedule(() => base.OnAddingNew(addingNewEventArgs));
		}

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event.
		/// </summary>
		/// <param name="listChangedEventArgs">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
		protected override void OnListChanged(ListChangedEventArgs listChangedEventArgs)
		{
			Scheduler.Schedule(() => base.OnListChanged(listChangedEventArgs));
		}

		#endregion
	}
}