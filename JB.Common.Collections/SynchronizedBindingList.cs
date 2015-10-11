// -----------------------------------------------------------------------
// <copyright file="SynchronizedBindingList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>A synchronized (list acitivities AND events raised) version of BindingList.</summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace JB.Common.Collections
{
	/// <summary>
	///     An <see cref="IBindingList" /> implementation that's synchronized via an underlying lock-based
	///     <see cref="IList{T}" />
	///     implementation, but also ensuring all events are raised on the same initially created or provided
	///     <see cref="SynchronizationContext" />.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class SynchronizedBindingList<T> : EnhancedBindingList<T>
	{
		private readonly SendOrPostCallback _onAddingNewInvoker;
		private readonly SendOrPostCallback _onListChangedInvoker;
		private readonly SynchronizationContext _synchronizationContext;

		/// <summary>
		///     Gets or sets a value indicating whether asynchronuous <see cref="SynchronizationContext.Post" /> or the
		///     synchronuous <see cref="SynchronizationContext.Send" /> will be used (default is the later)
		///     to raise the <see cref="BindingList{T}.ListChanged" /> or <see cref="BindingList{T}.AddingNew" /> events.
		/// </summary>
		/// <value>
		///     <c>true</c> if [post synchronized messages]; otherwise, <c>false</c>.
		/// </value>
		public bool PostSynchronizedMessages { get; set; }

		/// <summary>
		///     Initializes a new instance of the <see cref="SynchronizedBindingList{T}" /> class.
		/// </summary>
		/// <param name="list">
		///     An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the
		///     <see cref="T:System.ComponentModel.BindingList`1" />.
		/// </param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="syncRoot">The object used to synchronize access the thread-safe collection.</param>
		public SynchronizedBindingList(IList<T> list = null, object syncRoot = null, SynchronizationContext synchronizationContext = null)
			: base(syncRoot == null ? new SynchronizedCollection<T>(list ?? new List<T>()) : new SynchronizedCollection<T>(syncRoot, list ?? new List<T>()))
		{
			_synchronizationContext = synchronizationContext ?? (SynchronizationContext.Current ?? new SynchronizationContext());

			_onAddingNewInvoker = SynchronizedOnAddingNew;
			_onListChangedInvoker = SynchronizedOnListChanged;
		}

		/// <summary>
		///     Synchronized version of <see cref="OnAddingNew" /> as this gets called on the initially provided or captured
		///     <see cref="SynchronizationContext" />.
		/// </summary>
		/// <param name="addingNewEventArgs">The addingNewEventArgs.</param>
		private void SynchronizedOnAddingNew(object addingNewEventArgs)
		{
			var e = (AddingNewEventArgs) addingNewEventArgs;
			base.OnAddingNew(e);
		}

		/// <summary>
		///     Synchronized version of <see cref="OnListChanged" /> as this gets called on the initially provided or captured
		///     <see cref="SynchronizationContext" />.
		/// </summary>
		/// <param name="listChangedEventArgs">The listChangedEventArgs.</param>
		private void SynchronizedOnListChanged(object listChangedEventArgs)
		{
			var e = (ListChangedEventArgs) listChangedEventArgs;
			base.OnListChanged(e);
		}

		#region Overrides of BindingList<T>

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.AddingNew" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.ComponentModel.AddingNewEventArgs" /> that contains the event data. </param>
		protected override void OnAddingNew(AddingNewEventArgs e)
		{
			var synchronizationContext = (_synchronizationContext ?? SynchronizationContext.Current);
			if (PostSynchronizedMessages)
			{
				synchronizationContext.Post(_onAddingNewInvoker, e);
			}
			else
			{
				synchronizationContext.Send(_onAddingNewInvoker, e);
			}
		}

		/// <summary>
		///     Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
		protected override void OnListChanged(ListChangedEventArgs e)
		{
			var synchronizationContext = (_synchronizationContext ?? SynchronizationContext.Current);
			if (PostSynchronizedMessages)
			{
				synchronizationContext.Post(_onListChangedInvoker, e);
			}
			else
			{
				synchronizationContext.Send(_onListChangedInvoker, e);
			}
		}

		#endregion
	}
}