﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace JB.Collections
{
    /// <summary>
    ///     An <see cref="IBindingList" /> implementation that's synchronized via an underlying lock-based
    ///     <see cref="IList{T}" />
    ///     implementation, but also ensuring all events are raised on the same initially created or provided
    ///     <see cref="SynchronizationContext" />.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class SynchronizationContextSynchronizedBindingList<T> : SynchronizationContextCoordinatedBindingList<T>
	{	
		/// <summary>
		///     Initializes a new instance of the <see cref="SynchronizationContextSynchronizedBindingList{T}" /> class.
		/// </summary>
		/// <param name="list">
		///     An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the
		///     <see cref="T:System.ComponentModel.BindingList`1" />.
		/// </param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="syncRoot">The object used to synchronize access the thread-safe collection.</param>
		public SynchronizationContextSynchronizedBindingList(IList<T> list = null, object syncRoot = null, SynchronizationContext synchronizationContext = null)
			: base(syncRoot == null ? new SynchronizedCollection<T>(list ?? new List<T>()) : new SynchronizedCollection<T>(syncRoot, list ?? new List<T>()), synchronizationContext)
		{
		}
	}
}