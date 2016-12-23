// -----------------------------------------------------------------------
// <copyright file="AsyncReaderWriterLockExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace JB.Reactive.Threading
{
	/// <summary>
	/// Extension Methods for <see cref="AsyncReaderWriterLock"/> instances.
	/// </summary>
	public static class AsyncReaderWriterLockExtensions
	{

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current" /> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// </exception>
		public static Task<T> AddExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, Func<Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock" />.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current" /> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// </exception>
		public static Task AddExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, Func<Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task AddExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, Func<CancellationToken, Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task<T> AddExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, Func<CancellationToken, Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public static IObservable<Unit> AddExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, IObservable<Unit> workerObservable, IScheduler scheduler = null)
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));

			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnScheduler(workerObservable, scheduler);
		}

		/// <summary>
		/// Adds exclusive, non-concurrent work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public static IObservable<T> AddExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, IObservable<T> workerObservable, IScheduler scheduler = null)
		{
			return asyncReaderWriterLock.AcquireExclusiveWriterLock().PerformReaderWriterLockedWorkOnScheduler(workerObservable, scheduler);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task<T> AddConcurrentNonExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, Func<Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task AddConcurrentNonExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, Func<Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task AddConcurrentNonExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, Func<CancellationToken, Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public static Task<T> AddConcurrentNonExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, Func<CancellationToken, Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnSynchronizationContext(Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public static IObservable<Unit> AddConcurrentNonExclusiveWork(this AsyncReaderWriterLock asyncReaderWriterLock, IObservable<Unit> workerObservable, IScheduler scheduler = null)
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnScheduler(workerObservable, scheduler);
		}

		/// <summary>
		/// Adds concurrent, non-exclusive work to the execution queue of the provided <see cref="AsyncReaderWriterLock"/>.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="asyncReaderWriterLock">The <see cref="AsyncReaderWriterLock"/> to schedule work on.</param>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public static IObservable<T> AddConcurrentNonExclusiveWork<T>(this AsyncReaderWriterLock asyncReaderWriterLock, IObservable<T> workerObservable, IScheduler scheduler = null)
		{
			if (asyncReaderWriterLock == null) throw new ArgumentNullException(nameof(asyncReaderWriterLock));
			return asyncReaderWriterLock.AcquireConcurrentReaderLock().PerformReaderWriterLockedWorkOnScheduler(workerObservable, scheduler);
		}

        /// <summary>
        /// Schedules the actual work on the scheduler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readerWriterLockObservable">The reader writer lock observable.</param>
        /// <param name="work">The work.</param>
        /// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.CurrentThread"/> will be used.</param>
        /// <returns></returns>
        private static IObservable<T> PerformReaderWriterLockedWorkOnScheduler<T>(this IObservable<ReaderWriterLock> readerWriterLockObservable, IObservable<T> work, IScheduler scheduler = null)
		{
			return readerWriterLockObservable.SelectMany(readerWriterLock => work.SubscribeOn(scheduler ?? Scheduler.CurrentThread));
		}

        /// <summary>
        /// Schedules the actual work on the synchronizationcontext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readerWriterLockObservable">The reader writer lock observable.</param>
        /// <param name="work">The work.</param>
        /// <param name="synchronizationContext">The synchronization context. If none is provided, <see cref="SynchronizationContext.Current"/> will be used.</param>
        /// <returns></returns>
        private static IObservable<T> PerformReaderWriterLockedWorkOnSynchronizationContext<T>(this IObservable<ReaderWriterLock> readerWriterLockObservable, IObservable<T> work, SynchronizationContext synchronizationContext = null)
		{
			return readerWriterLockObservable.SelectMany(readerWriterLock => work.SubscribeOn(synchronizationContext ?? SynchronizationContext.Current));
		}
	}
}