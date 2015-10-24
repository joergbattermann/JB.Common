// -----------------------------------------------------------------------
// <copyright file="AsyncReaderWriterLock.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>
// A TPL Task and RX.Net compatible (Async)ReaderWriterLock.
// Initially based on https://gist.github.com/paulcbetts/9515910 with a few additions here and there.
// </summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace JB.Reactive.Threading
{
	/// <summary>
	/// A TPL <see cref="Task"/> and RX.Net compatible (Async)ReaderWriterLock.
	/// </summary>
	public class AsyncReaderWriterLock : IDisposable
	{
		private long _isDisposed = 0;
		private long _isDisposing = 0;
		private long _scheduledOperationId = 0;

		private IDisposable _disposingDisposable;

		/// <summary>
		/// Gets the underlying <see cref="ConcurrentExclusiveSchedulerPair"/>.
		/// </summary>
		/// <value>
		/// The concurrent exclusive scheduler pair.
		/// </value>
		protected ConcurrentExclusiveSchedulerPair ConcurrentExclusiveSchedulerPair { get; private set; }

		/// <summary>
		/// Gets the <see cref="TaskFactory"/> to schedule concurrent, reader tasks.
		/// </summary>
		/// <value>
		/// The reader scheduler.
		/// </value>
		protected TaskFactory ConcurrentNonExclusiveTaskFactory { get; private set; }

		/// <summary>
		/// Gets the concurrent, non-exclusive task scheduler.
		/// </summary>
		/// <value>
		/// The concurrent non exclusive task scheduler.
		/// </value>
		protected TaskScheduler ConcurrentNonExclusiveTaskScheduler => ConcurrentExclusiveSchedulerPair.ConcurrentScheduler;

		/// <summary>
		/// Gets the <see cref="TaskFactory"/> to schedule exclusive, writer tasks.
		/// </summary>
		/// <value>
		/// The writer scheduler.
		/// </value>
		protected TaskFactory ExclusiveTaskFactory { get; private set; }

		/// <summary>
		/// Gets the exclusive task scheduler.
		/// </summary>
		/// <value>
		/// The concurrent exclusive task scheduler.
		/// </value>
		protected TaskScheduler ExclusiveTaskScheduler => ConcurrentExclusiveSchedulerPair.ExclusiveScheduler;

		/// <summary>
		/// Gets a value indicating whether this instance has been disposed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has been disposed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposed
		{
			get { return Interlocked.Read(ref _isDisposed) == 1; }
			private set { Interlocked.Exchange(ref _isDisposed, value ? 1 : 0); }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is currently disposing.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is currently disposing; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposing
		{
			get { return Interlocked.Read(ref _isDisposing) == 1; }
			private set { Interlocked.Exchange(ref _isDisposing, value ? 1 : 0); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncReaderWriterLock"/> class.
		/// </summary>
		public AsyncReaderWriterLock()
		{
			// Built upon ConcurrentExclusiveSchedulerPair is a .Net TPL construct that provides exactly what we need
			// - reader/writer orchestration
			ConcurrentExclusiveSchedulerPair = new ConcurrentExclusiveSchedulerPair();

			ConcurrentNonExclusiveTaskFactory = new TaskFactory(this.ConcurrentExclusiveSchedulerPair.ConcurrentScheduler);
			ExclusiveTaskFactory = new TaskFactory(this.ConcurrentExclusiveSchedulerPair.ExclusiveScheduler);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task<T> AddExclusiveWork<T>(Func<Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireWriterLock(), Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task AddExclusiveWork(Func<Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireWriterLock(), Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task AddExclusiveWork(Func<CancellationToken, Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireWriterLock(), Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task<T> AddExclusiveWork<T>(Func<CancellationToken, Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireWriterLock(), Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public IObservable<Unit> AddExclusiveWork(IObservable<Unit> workerObservable, IScheduler scheduler = null)
		{
			return this.AddOperationOnScheduler(this.AcquireWriterLock(), workerObservable, scheduler);
		}

		/// <summary>
		/// Adds the exclusive, non-concurrent work to the execution queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public IObservable<T> AddExclusiveWork<T>(IObservable<T> workerObservable, IScheduler scheduler = null)
		{
			return this.AddOperationOnScheduler(this.AcquireWriterLock(), workerObservable, scheduler);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task<T> AddConcurrentNonExclusiveWork<T>(Func<Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireReaderLock(), Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task AddConcurrentNonExclusiveWork(Func<Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireReaderLock(), Observable.FromAsync(action), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task AddConcurrentNonExclusiveWork(Func<CancellationToken, Task> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireReaderLock(), Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="action">The action.</param>
		/// <param name="synchronizationContext">The synchronization context to execute the work on. If none is provided, <see cref="SynchronizationContext.Current"/> is used.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task<T> AddConcurrentNonExclusiveWork<T>(Func<CancellationToken, Task<T>> action, SynchronizationContext synchronizationContext = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return this.AddOperationOnSynchronizationContext(this.AcquireReaderLock(), Observable.FromAsync(token => action.Invoke(CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken).Token)), synchronizationContext).ToTask(cancellationToken);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public IObservable<Unit> AddConcurrentNonExclusiveWork(IObservable<Unit> workerObservable, IScheduler scheduler = null)
		{
			return this.AddOperationOnScheduler(this.AcquireReaderLock(), workerObservable, scheduler);
		}

		/// <summary>
		/// Adds the concurrent non-exclusive work to the execution queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="workerObservable">The worker observable.</param>
		/// <param name="scheduler">The scheduler. If none is provided, <see cref="Scheduler.Default"/> is used.</param>
		/// <returns></returns>
		public IObservable<T> AddConcurrentNonExclusiveWork<T>(IObservable<T> workerObservable, IScheduler scheduler = null)
		{
			return this.AddOperationOnScheduler(this.AcquireReaderLock(), workerObservable, scheduler);
		}

		/// <summary>
		/// Adds the operation on the scheduler.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="readerWriterLockObservable">The reader writer lock observable.</param>
		/// <param name="work">The work.</param>
		/// <param name="scheduler">The scheduler.</param>
		/// <returns></returns>
		private IObservable<T> AddOperationOnScheduler<T>(IObservable<ReaderWriterLock> readerWriterLockObservable, IObservable<T> work, IScheduler scheduler = null)
		{
			return readerWriterLockObservable.SelectMany(readerWriterLock => work.SubscribeOn(scheduler ?? Scheduler.Default));
		}

		/// <summary>
		/// Adds the operation on the scheduler.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="readerWriterLockObservable">The reader writer lock observable.</param>
		/// <param name="work">The work.</param>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <returns></returns>
		private IObservable<T> AddOperationOnSynchronizationContext<T>(IObservable<ReaderWriterLock> readerWriterLockObservable, IObservable<T> work, SynchronizationContext synchronizationContext = null)
		{
			return readerWriterLockObservable.SelectMany(readerWriterLock => work.SubscribeOn(synchronizationContext ?? SynchronizationContext.Current));
		}

		/// <summary>
		/// Acquires reader permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// Please note - if you're using async and particularly await inside the lock (using block or prior to calling <see cref="ReaderWriterLock.Dispose"/>))
		/// it might not work as expected, see http://stackoverflow.com/questions/12068645/how-do-i-create-a-scheduler-which-never-executes-more-than-one-task-at-a-time-us#comment16125533_12069460
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireReaderLockAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return AcquireReaderLock().ToTask(cancellationToken);
		}

		/// <summary>
		/// Acquires writer permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// Please note - if you're using async and particularly await inside the lock (using block or prior to calling <see cref="ReaderWriterLock.Dispose"/>))
		/// it might not work as expected, see http://stackoverflow.com/questions/12068645/how-do-i-create-a-scheduler-which-never-executes-more-than-one-task-at-a-time-us#comment16125533_12069460
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireWriterLockAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return AcquireWriterLock().ToTask(cancellationToken);
		}

		/// <summary>
		/// Returns a (future) reader lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireReaderLock()
		{
			return AcquireReadOrWriteLockObservable(this.ConcurrentNonExclusiveTaskFactory);
		}

		/// <summary>
		/// Returns a (future) exclusive writer lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireWriterLock()
		{
			return AcquireReadOrWriteLockObservable(this.ExclusiveTaskFactory);
		}

		/// <summary>
		/// Acquires the read or write lock on the scheduler, as observable.
		/// </summary>
		/// <param name="taskFactory">The task factory.</param>
		/// <returns></returns>
		private IObservable<ReaderWriterLock> AcquireReadOrWriteLockObservable(TaskFactory taskFactory)
		{
			// check for incorrect entry once we have already been disposed
			if (IsDisposed)
				return Observable.Throw<ReaderWriterLock>(new ObjectDisposedException(nameof(AsyncReaderWriterLock)));

			// basically what happens here is we use the (concurrent) reader or (exclusive) handed in
			// and schedule a new tpl task on it which merely wraps or contains chained IDisposable creation.
			// What happens then is, we return a 'future' promise or timeslot for whenever the ConcurrentExclusiveSchedulerPair
			// does actually start working on that TPL task (meaning that by its own, internal r/w locking, the task was up next).

			// this is the result we hand back to the caller
			var asyncSubject = new AsyncSubject<ReaderWriterLock>();
			var gate = new AsyncSubject<Unit>();

			taskFactory.StartNew(async () =>
			{
				// this asyncSubject is the one actually handed back to the method's caller as IDisposable instance
				// & whenever that one is disposed, the gate gets unblocked \o/
				asyncSubject.OnNext(
					new ReaderWriterLock(
						Interlocked.Increment(ref _scheduledOperationId),
						taskFactory == ExclusiveTaskFactory,
						Disposable.Create(() =>
						{
							gate.OnNext(Unit.Default);
							gate.OnCompleted();
						})));
				asyncSubject.OnCompleted();

				// once the asyncSubject's ticket has been disposed, this gate gets unlocked, too
				await gate;
			});

			return asyncSubject;
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			// prevent re-entry
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(AsyncReaderWriterLock));

			if (IsDisposing)
				return;

			IsDisposing = true;

			// Just grab the write lock one last time to schedule it's final, exclusive activity
			var finalWriterObservable = AcquireWriterLock();

			_disposingDisposable = finalWriterObservable.Subscribe(disposable =>
			{
				try
				{
					disposable.Dispose();
					this.ConcurrentExclusiveSchedulerPair.Complete(); // make sure the underlying scheduler also is marked as complete
					IsDisposed = true; // mark this entire instance as disposed
				}
				finally
				{
					IsDisposing = false;
				}
			});
		}
		#endregion
	}
}