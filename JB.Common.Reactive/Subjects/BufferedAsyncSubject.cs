// -----------------------------------------------------------------------
// <copyright file="BufferedAsyncSubject.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
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
    /// The same as <see cref="AsyncSubject{T}"/> but returning all observed sequence elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BufferedAsyncSubject<T> : SubjectBase<T>, INotifyCompletion, IDisposable
    {
        #region Fields

        private readonly object _gate = new object();

        private ImmutableList<IObserver<T>> _observers;
        private bool _isDisposed;
        private bool _isStopped;
        private T _value;
        private bool _hasValue;
        private Exception _exception;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a subject that buffers all received values and those values are cached for all future observations.
        /// </summary>
        public BufferedAsyncSubject()
        {
            // ToDo: Observable.Replay? / Buffer?
            _observers = ImmutableList<IObserver<T>>.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether the subject has observers subscribed to it.
        /// </summary>
        public override bool HasObservers
        {
            get
            {
                var observers = _observers;
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
                lock (_gate)
                {
                    return _isDisposed;
                }
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

            var value = default(T);
            var hasValue = false;
            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    observers = _observers.ToArray();
                    _observers = ImmutableList<IObserver<T>>.Empty;
                    _isStopped = true;
                    value = _value;
                    hasValue = _hasValue;
                }
            }

            if (observers != null)
            {
                if (hasValue)
                {
                    foreach (var observer in observers)
                    {
                        observer.OnNext(value);
                        observer.OnCompleted();
                    }
                }
                else
                    foreach (var observer in observers)
                    {
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
            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    observers = _observers.ToArray();
                    _observers = ImmutableList<IObserver<T>>.Empty;
                    _isStopped = true;
                    _exception = error;
                }
            }

            if (observers != null)
            {
                foreach (var observer in observers)
                {
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
                CheckDisposed();

                if (!_isStopped)
                {
                    _value = value;
                    _hasValue = true;
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
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            var exception = default(Exception);
            var value = default(T);
            var hasValue = false;

            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    _observers = _observers.Add(observer);
                    return new BufferedAsyncSubjectSubscription(this, observer);
                }

                exception = _exception;
                hasValue = _hasValue;
                value = _value;
            }

            if (exception != null)
            {
                observer.OnError(exception);
            }
            else if (hasValue)
            {
                observer.OnNext(value);
                observer.OnCompleted();
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        private class BufferedAsyncSubjectSubscription : IDisposable
        {
            private readonly BufferedAsyncSubject<T> _subject;
            private IObserver<T> _observer;

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedAsyncSubjectSubscription"/> class.
            /// </summary>
            /// <param name="subject">The subject.</param>
            /// <param name="observer">The observer.</param>
            public BufferedAsyncSubjectSubscription(BufferedAsyncSubject<T> subject, IObserver<T> observer)
            {
                _subject = subject;
                _observer = observer;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (_observer != null)
                {
                    lock (_subject._gate)
                    {
                        if (!_subject._isDisposed && _observer != null)
                        {
                            _subject._observers = _subject._observers.Remove(_observer);
                            _observer = null;
                        }
                    }
                }
            }
        }

        #endregion

        #region IDisposable implementation

        void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Unsubscribe all observers and release resources.
        /// </summary>
        public override void Dispose()
        {
            lock (_gate)
            {
                _isDisposed = true;
                _observers = null;
                _exception = null;
                _value = default(T);
            }
        }

        #endregion

        #region Await support

        /// <summary>
        /// Gets an awaitable object for the current BufferedAsyncSubject.
        /// </summary>
        /// <returns>Object that can be awaited.</returns>
        public BufferedAsyncSubject<T> GetAwaiter()
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
        /// Called when [completed].
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        /// <param name="originalContext">if set to <c>true</c> [original context].</param>
        private void OnCompleted(Action continuation, bool originalContext)
        {
            this.Subscribe(new AwaitObserver(continuation, originalContext));
        }

        class AwaitObserver : IObserver<T>
        {
            private readonly SynchronizationContext _context;
            private readonly Action _callback;
            public AwaitObserver(Action callback, bool originalContext)
            {
                if (originalContext)
                {
                    _context = SynchronizationContext.Current;
                }
                
                _callback = callback;
            }

            public void OnCompleted()
            {
                InvokeOnOriginalContext();
            }

            public void OnError(Exception error)
            {
                InvokeOnOriginalContext();
            }

            public void OnNext(T value)
            {
            }

            private void InvokeOnOriginalContext()
            {
                if (_context != null)
                {
                    //
                    // No need for OperationStarted and OperationCompleted calls here;
                    // this code is invoked through await support and will have a way
                    // to observe its start/complete behavior, either through returned
                    // Task objects or the async method builder's interaction with the
                    // SynchronizationContext object.
                    //
                    _context.Post(c => ((Action)c)(), _callback);
                }
                else
                {
                    _callback();
                }
            }
        }

        /// <summary>
        /// Gets whether the <see cref="BufferedAsyncSubject{T}"/> has completed.
        /// </summary>
        public bool IsCompleted => _isStopped;

        /// <summary>
        /// Gets the last element of the subject, potentially blocking until the subject completes successfully or exceptionally.
        /// </summary>
        /// <returns>The last element of the subject. Throws an InvalidOperationException if no element was received.</returns>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Await pattern for C# and VB compilers.")]
        public T GetResult()
        {
            if (!_isStopped)
            {
                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    OnCompleted(() => manualResetEvent.Set(), false);
                    manualResetEvent.WaitOne();
                }
            }

            _exception.ThrowIfNotNull();

            if (!_hasValue)
                throw new InvalidOperationException("Sequence contains no elements");

            return _value;
        }

        #endregion
    }
}