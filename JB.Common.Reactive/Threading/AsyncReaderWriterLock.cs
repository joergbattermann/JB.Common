// -----------------------------------------------------------------------
// <copyright file="AsyncReaderWriterLock.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>A TPL Task and RX.Net compatible (Async)ReaderWriterLock. Largely based on and extended by https://gist.github.com/paulcbetts/9515910. </summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
		protected TaskFactory ReaderScheduler { get; private set; }

		/// <summary>
		/// Gets the <see cref="TaskFactory"/> to schedule exclusive, writer tasks.
		/// </summary>
		/// <value>
		/// The writer scheduler.
		/// </value>
		protected TaskFactory WriterScheduler { get; private set; }

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
			// - reader/writer handling
			ConcurrentExclusiveSchedulerPair = new ConcurrentExclusiveSchedulerPair();

			ReaderScheduler = new TaskFactory(this.ConcurrentExclusiveSchedulerPair.ConcurrentScheduler);
			WriterScheduler = new TaskFactory(this.ConcurrentExclusiveSchedulerPair.ExclusiveScheduler);
		}

		#region TPL Reader / Writer methods

		/// <summary>
		/// Acquires reader permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireReaderLockAsync()
		{
			return AcquireReadOrWriteLockAsync(this.ReaderScheduler);
		}

		/// <summary>
		/// Acquires writer permissions sometimes in the future and returns the corresponding <see cref="ReaderWriterLock">ticket</see>.
		/// </summary>
		/// <returns></returns>
		public Task<ReaderWriterLock> AcquireWriterLockAsync()
		{
			return AcquireReadOrWriteLockAsync(this.WriterScheduler);
		}

		/// <summary>
		/// Acquires the read or write lock on the scheduler, asynchronously.
		/// </summary>
		/// <param name="taskFactory">The task factory.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		private Task<ReaderWriterLock> AcquireReadOrWriteLockAsync(TaskFactory taskFactory)
		{
			// this shouldn't happen but wth
			if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));

			// check for incorrect entry once we have already been disposed
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(AsyncReaderWriterLock));

			var asyncSubject = new TaskCompletionSource<ReaderWriterLock>();
			var gate = new TaskCompletionSource<object>();

			taskFactory.StartNew(async () =>
			{
				// the asyncSubject holds the task actually being handed back to the method's caller
				// & whenever that ReaderWriterLock is disposed, the gate's .Result gets set and the await gate.Task etc
				// can continue and end the entire method/task handed over to the outer ConcurrentSchedulerPair
				asyncSubject.SetResult(
					new ReaderWriterLock(
						Interlocked.Increment(ref _scheduledOperationId),
						Disposable.Create(() =>
						{
							gate.SetResult(new object());
						})));

				// The outer .Dispose() call on the returned asyncSubject.Task will allow this gate to run be set to completed, too
				// and therefore finish this taskFactory scheduled task altogether, freeing up the queue.
				await gate.Task.ConfigureAwait(false);
			});

			return asyncSubject.Task;
		}

		#endregion

		#region RX Reader / Writer methods


		/// <summary>
		/// Returns a (future) reader lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireReaderLock()
		{
			return AcquireReadOrWriteLockObservable(this.ReaderScheduler);
		}

		/// <summary>
		/// Returns a (future) exclusive writer lock, as an observable.
		/// </summary>
		/// <returns></returns>
		public IObservable<ReaderWriterLock> AcquireWriterLock()
		{
			return AcquireReadOrWriteLockObservable(this.WriterScheduler);
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

		#endregion

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