// -----------------------------------------------------------------------
// <copyright file="AsyncReaderWriterLock.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>
// A TPL Task and RX.Net compatible (Async)ReaderWriterLock.
// Initially based on https://gist.github.com/paulcbetts/9515910 with a few additions here and there.
// </summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
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
		private long _isDisposed;
		private long _isDisposing;
		private long _scheduledOperationId;

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
		/// Acquires reader permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// Please note - if you're using async and particularly await inside the lock (using block or prior to calling <see cref="ReaderWriterLock.Dispose"/>))
		/// it might not work as expected, see http://stackoverflow.com/questions/12068645/how-do-i-create-a-scheduler-which-never-executes-more-than-one-task-at-a-time-us#comment16125533_12069460
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireConcurrentReaderLockAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return AcquireConcurrentReaderLock().ToTask(cancellationToken);
		}

		/// <summary>
		/// Acquires writer permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// Please note - if you're using async and particularly await inside the lock (using block or prior to calling <see cref="ReaderWriterLock.Dispose"/>))
		/// it might not work as expected, see http://stackoverflow.com/questions/12068645/how-do-i-create-a-scheduler-which-never-executes-more-than-one-task-at-a-time-us#comment16125533_12069460
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireExclusiveWriterLockAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return AcquireExclusiveWriterLock().ToTask(cancellationToken);
		}

		/// <summary>
		/// Returns a (future) reader lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireConcurrentReaderLock()
		{
			return AcquireReadOrWriteLockObservable(this.ConcurrentNonExclusiveTaskFactory);
		}

		/// <summary>
		/// Returns a (future) exclusive writer lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireExclusiveWriterLock()
		{
			return AcquireReadOrWriteLockObservable(this.ExclusiveTaskFactory);
		}

		/// <summary>
		/// Acquires the read or write lock on the scheduler, as observable.
		/// </summary>
		/// <param name="schedulingTaskFactory">The task factory.</param>
		/// <returns></returns>
		private IObservable<ReaderWriterLock> AcquireReadOrWriteLockObservable(TaskFactory schedulingTaskFactory)
		{
			// check for incorrect entry once we have already been disposed
			if (IsDisposed)
				return Observable.Throw<ReaderWriterLock>(new ObjectDisposedException(this.GetType().Name));

			// basically what happens here is we use the (concurrent) reader or (exclusive) handed in
			// and schedule a new tpl task on it which merely wraps or contains chained IDisposable creation.
			// What happens then is, we return a 'future' promise or timeslot for whenever the ConcurrentExclusiveSchedulerPair
			// does actually start working on that TPL task (meaning that by its own, internal r/w locking, the task was up next).

			// this is the result we hand back to the caller
			var asyncSubject = new AsyncSubject<ReaderWriterLock>();
			var gate = new AsyncSubject<Unit>();

			schedulingTaskFactory.StartNew(async () =>
			{
				// this asyncSubject is the one actually handed back to the method's caller as IDisposable instance
				// & whenever that one is disposed, the gate gets unblocked \o/
				asyncSubject.OnNext(
					new ReaderWriterLock(
						Interlocked.Increment(ref _scheduledOperationId),
						schedulingTaskFactory == ExclusiveTaskFactory,
						Disposable.Create(() =>
						{
							gate.OnNext(Unit.Default);
							gate.OnCompleted();
						})));
				asyncSubject.OnCompleted();

				// once the asyncSubject's ticket has been disposed, this gate gets unlocked, too
				await gate;

                // cleanup
                gate.Dispose();
			    gate = null;
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
			if (IsDisposing || IsDisposed)
				return;

			IsDisposing = true;

			// Just grab the write lock one last time to schedule it's final, exclusive activity
			var finalWriterObservable = AcquireExclusiveWriterLock();

			_disposingDisposable = finalWriterObservable.Subscribe(disposable =>
			{
				try
				{
					this.ConcurrentExclusiveSchedulerPair.Complete(); // make sure the underlying scheduler also is marked as complete
					IsDisposed = true; // mark this entire instance as disposed
				}
				finally
				{
					disposable.Dispose();
					IsDisposing = false;

					_disposingDisposable.Dispose();
					_disposingDisposable = null;
				}
			});
		}
		#endregion
	}
}