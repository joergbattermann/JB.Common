using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Threading;

namespace JB.Collections.Reactive
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>, IDisposable
    {
        /// <summary>
        /// Gets the actual, inner dictionary used.
        /// </summary>
        /// <value>
        /// The inner dictionary.
        /// </value>
        protected ConcurrentDictionary<TKey, TValue> InnerDictionary { get; }

        /// <summary>
        ///     Gets the used scheduler.
        /// </summary>
        /// <value>
        ///     The scheduler.
        /// </value>
        protected IScheduler Scheduler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ObservableDictionary" /> class that contains elements
        /// copied from the specified <paramref name="collection" /> and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
        /// </summary>
        /// <param name="collection">The elements that are copied to this instance.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys.</param>
        /// <param name="scheduler">The scheduler to raise events on, if none is provided <see cref="System.Reactive.Concurrency.Scheduler.CurrentThread"/> will be used.</param>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection = null, IEqualityComparer<TKey> comparer = null, IScheduler scheduler = null)
        {
            // ToDo: check whether scheduler shall / should be used for internall used RX notifications / Subjects etc
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;

            if (comparer != null)
            {
                InnerDictionary = collection != null
                    ? new ConcurrentDictionary<TKey, TValue>(collection, comparer)
                    : new ConcurrentDictionary<TKey, TValue>(comparer);
            }
            else
            {
                InnerDictionary = collection != null
                    ? new ConcurrentDictionary<TKey, TValue>(collection)
                    : new ConcurrentDictionary<TKey, TValue>();
            }
        }


        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisposed
        {
            get { return Interlocked.Read(ref _isDisposed) == 1; }
            protected set
            {
                lock (_isDisposedLocker)
                {
                    if (value == false && IsDisposed)
                        throw new InvalidOperationException("Once Disposed has been set, it cannot be reset back to false.");

                    Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
                }

                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisposing
        {
            get { return Interlocked.Read(ref _isDisposing) == 1; }
            protected set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                if (disposeManagedResources)
                {
                    //if (_collectionChangesAndResetsPropertyChangeForwarder != null)
                    //{
                    //    _collectionChangesAndResetsPropertyChangeForwarder.Dispose();
                    //    _collectionChangesAndResetsPropertyChangeForwarder = null;
                    //}
                }
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        ///     Checks whether this instance is currently or already has been disposed.
        /// </summary>
        protected virtual void CheckForAndThrowIfDisposed()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException(this.GetType().Name, "This instance is currently being disposed.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        ///     The actual <see cref="PropertyChanged" /> event.
        /// </summary>
        private PropertyChangedEventHandler _propertyChanged;

        /// <summary>
        ///     Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _propertyChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _propertyChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (IsDisposed || IsDisposing)
                return;

            var eventHandler = _propertyChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        #endregion
    }
}