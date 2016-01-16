// -----------------------------------------------------------------------
// <copyright file="AsyncBufferingSubject.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Runtime.CompilerServices;
using JB.ExtensionMethods;

namespace JB.Reactive.Subjects
{
    /// <summary>
    /// The same as <see cref="AsyncSubject{T}"/> but buffering and finally returning all observed sequence elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AsyncBufferingSubject<T> : SubjectBase<T>, INotifyCompletion, IDisposable
    {
        #region Fields

        private readonly object _gate = new object();
        private long _isDisposed = 0;
        private long _isStopped = 0;
        private Exception _exception;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a subject that buffers all received values and those values are cached for all future observations.
        /// </summary>
        public AsyncBufferingSubject()
        {
            // ToDo: Observable.Replay? / Buffer?
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the observers.
        /// </summary>
        /// <value>
        /// The observers.
        /// </value>
        private ImmutableList<IObserver<T>> Observers { get; set; } = ImmutableList<IObserver<T>>.Empty;

        /// <summary>
        /// Gets or sets the received values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        private List<T> Values { get; set; } = new List<T>();

        /// <summary>
        /// Indicates whether the subject has observers subscribed to it.
        /// </summary>
        public override bool HasObservers
        {
            get
            {
                var observers = Observers;
                return observers != null && observers.Count > 0;
            }
        }

        /// <summary>
        /// Indicates whether the subject has been disposed.
        /// </summary>
        public override bool IsDisposed
        {
            get
            {
                return Interlocked.Read(ref _isDisposed) == 1;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has stopped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has stopped; otherwise, <c>false</c>.
        /// </value>
        private bool IsStopped
        {
            get
            {
                return Interlocked.Read(ref _isStopped) == 1;
            }
            set
            {
                Interlocked.Exchange(ref _isStopped, value ? 1 : 0);
            }
        }

        #endregion

        #region IObserver<T> implementation

        /// <summary>
        /// Notifies all subscribed observers about the end of the sequence, also causing the last received value to be sent out (if any).
        /// </summary>
        public override void OnCompleted()
        {
            var observers = default(IObserver<T>[]);
            var values = new List<T>();

            lock (_gate)
            {
                CheckAndThrowIfDisposed();

                if (!IsStopped)
                {
                    values = Values ?? new List<T>();

                    observers = Observers.ToArray();
                    Observers = ImmutableList<IObserver<T>>.Empty;

                    IsStopped = true;
                }
            }

            if (observers != default(IObserver<T>[]))
            {
                foreach (var observer in observers)
                {
                    foreach (var value in values)
                    {
                        observer.OnNext(value);
                    }
                    
                    observer.OnCompleted();
                }
            }
        }

        /// <summary>
        /// Notifies all subscribed observers about the exception.
        /// </summary>
        /// <param name="error">The exception to send to all observers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public override void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var observers = default(IObserver<T>[]);
            var values = new List<T>();

            lock (_gate)
            {
                CheckAndThrowIfDisposed();

                if (!IsStopped)
                {
                    values = Values ?? new List<T>();

                    observers = Observers.ToArray();
                    Observers = ImmutableList<IObserver<T>>.Empty;

                    IsStopped = true;
                    _exception = error;
                }
            }

            if (observers != default(IObserver<T>[]))
            {
                foreach (var observer in observers)
                {
                    foreach (var value in values)
                    {
                        observer.OnNext(value);
                    }

                    observer.OnError(error);
                }
            }
        }

        /// <summary>
        /// Sends a value to the subject. The last value received before successful termination will be sent to all subscribed and future observers.
        /// </summary>
        /// <param name="value">The value to store in the subject.</param>
        public override void OnNext(T value)
        {
            lock (_gate)
            {
                CheckAndThrowIfDisposed();

                if (!IsStopped)
                {
                    Values.Add(value);
                }
            }
        }

        #endregion

        #region IObservable<T> implementation

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
        public override IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            var localObserver = observer;
            var exception = default(Exception);
            var values = new List<T>();

            lock (_gate)
            {
                CheckAndThrowIfDisposed();

                if (!IsStopped)
                {
                    Observers = Observers.Add(localObserver);

                    return Disposable.Create(() =>
                    {
                        lock (_gate)
                        {
                            if (!IsDisposed)
                            {
                                Observers = Observers.Remove(localObserver);
                                localObserver = null;
                            }
                        }
                    });
                }

                exception = _exception;
                values = Values ?? new List<T>();
            }

            foreach (var value in values)
            {
                observer.OnNext(value);
            }

            if (exception != null)
            {
                observer.OnError(exception);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Checks whether this instance has been disposed and if so, throws and <see cref="ObjectDisposedException"/>.
        /// </summary>
        void CheckAndThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Unsubscribe all observers and release resources.
        /// </summary>
        public override void Dispose()
        {
            lock (_gate)
            {
                Interlocked.Exchange(ref _isDisposed, 1); // IsDisposed = true;

                Observers = null;

                Values?.Clear();
                Values = null;

                _exception = null;
            }
        }

        #endregion

        #region Await support
        
        /// <summary>
        /// Gets an awaitable object for the current <see cref="AsyncBufferingSubject{T}"/>.
        /// </summary>
        /// <returns>Object that can be awaited.</returns>
        public AsyncBufferingSubject<T> GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// Specifies a callback action that will be invoked when the subject completes.
        /// </summary>
        /// <param name="continuation">Callback action that will be invoked when the subject completes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="continuation"/> is null.</exception>
        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            OnCompleted(continuation, true);
        }

        /// <summary>
        /// Specifies a callback action that will be invoked when the subject completes optionally on the original / returned-to context.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        /// <param name="invokeOnOriginalContext">if set to <c>true</c> [invoke on original context].</param>
        private void OnCompleted(Action continuation, bool invokeOnOriginalContext)
        {
            if (invokeOnOriginalContext)
            {
                var context = SynchronizationContext.Current ?? new SynchronizationContext();

                context.Post(state => continuation(), null);
            }
            else
            {
                continuation();
            }
        }
        
        /// <summary>
        /// Gets whether the <see cref="AsyncBufferingSubject{T}"/> has completed.
        /// </summary>
        public bool IsCompleted => IsStopped;

        /// <summary>
        /// Gets all buffered elements of the subject, potentially blocking until the subject completes successfully or exceptionally.
        /// </summary>
        /// <returns>The buffered elements of the subject.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Await pattern for C# and VB compilers.")]
        public List<T> GetResult()
        {
            if (!IsStopped)
            {
                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    OnCompleted(() => manualResetEvent.Set(), false);
                    manualResetEvent.WaitOne();
                }
            }

            // check if an exception occured and throw if  so
            _exception.ThrowIfNotNull();
            
            // otherwise return buffered values
            return Values ?? new List<T>();
        }

        #endregion
    }
}