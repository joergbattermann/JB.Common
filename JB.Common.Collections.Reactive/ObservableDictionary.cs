using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using JB.Collections.Reactive.ExtensionMethods;
using JB.Reactive.Linq;

namespace JB.Collections.Reactive
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>, IDisposable
    {
        protected Subject<IObservableDictionaryChange<TKey, TValue>> DictionaryChangesSubject = null;
        protected Subject<int> CountChangesSubject = null;
        protected Subject<Exception> ThrownExceptionsSubject = null;

        /// <summary>
        /// Gets the actual dictionary used - the rest in here is just fancy wrapping paper.
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

            ThresholdAmountWhenItemChangesAreNotifiedAsReset = 100;

            IsTrackingChanges = true;
            IsTrackingItemChanges = true;
            IsTrackingCountChanges = true;
            IsTrackingResets = true;

            SetupRxObservablesAndSubjects();
        }

        #region Helper Methods

        /// <summary>
        /// Prepares and sets up the observables and subjects used, particularly
        /// <see cref="DictionaryChangesSubject"/>, <see cref="CountChangesSubject"/> and <see cref="ThrownExceptionsSubject"/> and also notifications for
        /// 'Count' and 'Items[]' <see cref="INotifyPropertyChanged"/> events on <see cref="CountChanges"/> and <see cref="CollectionChanges"/>
        /// occurrences (for WPF / Binding)
        /// </summary>
        private void SetupRxObservablesAndSubjects()
        {
            // ToDo: check whether scheduler shall / should be used for internally used RX notifications / Subjects etc and if so, where

            // prepare subjects for RX
            ThrownExceptionsSubject = new Subject<Exception>();
            DictionaryChangesSubject = new Subject<IObservableDictionaryChange<TKey, TValue>>();
            CountChangesSubject = new Subject<int>();

            //// then connect to InnerList's ListChanged Event
            //_innerListChangedForwader = Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
            //    handler => InnerList.ListChanged += handler,
            //    handler => InnerList.ListChanged -= handler)
            //    .TakeWhile(_ => !IsDisposing && !IsDisposed)
            //    .SkipWhileContinuously(_ => !IsTrackingChanges)
            //    .Where(eventPattern => eventPattern?.EventArgs != null)
            //    .Select(eventPattern => eventPattern.EventArgs.ToObservableCollectionChange(InnerList))
            //    .ObserveOn(Scheduler)
            //.Subscribe(
            //NotifyObservableCollectionChangedSubscribersAndRaiseCollectionChangedEvents,
            //exception =>
            //{
            //    ThrownExceptionsSubject.OnNext(exception);
            //    // ToDo: at this point this instance is practically doomed / no longer forwarding any events & therefore further usage of the instance itself should be prevented, or the observable stream should re-connect/signal-and-swallow exceptions. Either way.. not ideal.
            //});


            //// 'Count' and 'Item[]' PropertyChanged events are used by WPF typically via / for ObservableCollections, see
            //// http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs,421
            //_countChangesPropertyChangeForwarder = CountChanges.ObserveOn(Scheduler).Subscribe(_ => RaisePropertyChanged("Count"));
            //_collectionChangesAndResetsPropertyChangeForwarder = CollectionChanges.ObserveOn(Scheduler).Subscribe(_ => RaisePropertyChanged("Item[]"));
        }

        #endregion

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
                    if (CountChangesSubject != null)
                    {
                        CountChangesSubject.Dispose();
                        CountChangesSubject = null;
                    }

                    if (DictionaryChangesSubject != null)
                    {
                        DictionaryChangesSubject.Dispose();
                        DictionaryChangesSubject = null;
                    }

                    if (ThrownExceptionsSubject != null)
                    {
                        ThrownExceptionsSubject.Dispose();
                        ThrownExceptionsSubject = null;
                    }
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

        #region Implementation of INotifyObservableExceptionsThrown

        /// <summary>
        /// Provides an observable sequence of exceptions thrown.
        /// </summary>
        /// <value>
        /// The thrown exceptions.
        /// </value>
        public virtual IObservable<Exception> ThrownExceptions
        {
            get
            {
                CheckForAndThrowIfDisposed();

                // not caring about IsDisposing / IsDisposed on purpose once subscribed, so corresponding Exceptions are forwarded 'til the "end" to already existing subscribers
                return ThrownExceptionsSubject;
            }
        }

        #endregion

        #region Implementation of INotifyObservableResets

        /// <summary>
        /// Gets a value indicating whether this instance is tracking and notifying about
        /// list / collection resets, typically for data binding.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>

        private readonly object _isTrackingResetsLocker = new object();
        private long _isTrackingResets = 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is tracking resets.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingResets
        {
            get { return Interlocked.Read(ref _isTrackingResets) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingResetsLocker)
                {
                    if (value == false && IsTrackingResets == false)
                        throw new InvalidOperationException("A Reset(s) Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingResets, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the reset notifications as an observable stream.  Whenever signaled,
        /// observers should reset any knowledge / state etc about the list.
        /// </summary>
        /// <value>
        /// The resets.
        /// </value>
        public virtual IObservable<Unit> Resets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return DictionaryChanges
                    .Where(change => change.ChangeType == ObservableDictionaryChangeType.Reset)
                    .SkipWhileContinuously(_ => IsTrackingResets == false)
                    .Select(_ => Unit.Default);
            }
        }

        /// <summary>
        /// (Temporarily) suppresses change notifications for resets until the <see cref="IDisposable" /> handed over to the caller
        /// has been Disposed and then a Reset will be signaled, if wanted and applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingResets = false;

            return Disposable.Create(() =>
            {
                IsTrackingResets = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        #endregion

        #region Implementation of INotifyObservableCountChanged

        private readonly object _isTrackingCountChangesLocker = new object();
        private long _isTrackingCountChanges = 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is tracking <see cref="IReadOnlyCollection{T}.Count" /> changes.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingCountChanges
        {
            get { return Interlocked.Read(ref _isTrackingCountChanges) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingCountChangesLocker)
                {
                    if (value == false && IsTrackingCountChanges == false)
                        throw new InvalidOperationException("A Count Change(s) Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingCountChanges, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The count changes.
        /// </value>
        public virtual IObservable<int> CountChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CountChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhileContinuously(_ => !IsTrackingCountChanges)
                    .DistinctUntilChanged();
            }
        }

        /// <summary>
        /// (Temporarily) suppresses item count change notification until the returned <see cref="IDisposable" />
        /// has been Disposed.
        /// </summary>
        /// <param name="signalCurrentCountWhenFinished">if set to <c>true</c> signals a the current count when disposed.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressCountChangedNotifications(bool signalCurrentCountWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingCountChanges = false;

            return Disposable.Create(() =>
            {
                IsTrackingCountChanges = true;

                if (signalCurrentCountWhenFinished)
                {
                    CountChangesSubject.OnNext(Count);
                }
            });
        }

        #endregion

        #region Implementation of INotifyObservableItemChanged

        /// <summary>
        /// (Temporarily) suppresses change notifications for <see cref="ObservableCollectionChangeType.ItemChanged"/> events until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingItemChanges = false;

            return Disposable.Create(() =>
            {
                IsTrackingItemChanges = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        private readonly object _isTrackingItemChangesLocker = new object();
        private long _isTrackingItemChanges = 0;

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        /// <see cref="INotifyPropertyChanged.PropertyChanged" /> events, if that interface is implemented, too.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">An Item Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.</exception>
        public bool IsTrackingItemChanges
        {
            get { return Interlocked.Read(ref _isTrackingItemChanges) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingItemChangesLocker)
                {
                    if (value == false && IsTrackingItemChanges == false)
                        throw new InvalidOperationException("An Item Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingItemChanges, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        private volatile int _thresholdAmountWhenItemChangesAreNotifiedAsReset;

        /// <summary>
        /// Gets the minimum amount of items that have been changed to be notified / considered a
        /// <see cref="ObservableCollectionChangeType.Reset" /> rather than individual <see cref="ObservableCollectionChangeType" /> notifications.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered reset.
        /// </value>
        public int ThresholdAmountWhenItemChangesAreNotifiedAsReset
        {
            get { return _thresholdAmountWhenItemChangesAreNotifiedAsReset; }
            set
            {
                CheckForAndThrowIfDisposed();

                _thresholdAmountWhenItemChangesAreNotifiedAsReset = value;

                RaisePropertyChanged();
            }
        }

        #endregion

        #region Implementation of INotifyObservableDictionaryChanged<TKey,TValue>

        /// <summary>
        /// Gets the dictionary changes as an observable stream.
        /// </summary>
        /// <value>
        /// The dictionary changes.
        /// </value>
        public virtual IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return DictionaryChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhileContinuously(change => !IsTrackingChanges)
                    .SkipWhileContinuously(change => change.ChangeType == ObservableDictionaryChangeType.ItemChanged && !IsTrackingItemChanges)
                    .SkipWhileContinuously(change => change.ChangeType == ObservableDictionaryChangeType.Reset && !IsTrackingResets);
            }
        }

        /// <summary>
        /// The actual event for <see cref="ObservableDictionaryChanged"/>.
        /// </summary>
        private EventHandler<ObservableDictionaryChangedEventArgs<TKey, TValue>> _observableDictionaryChanged;

        /// <summary>
        /// Occurs when the corresponding <see cref="IObservableCollection{T}" /> changed.
        /// </summary>
        public event EventHandler<ObservableDictionaryChangedEventArgs<TKey, TValue>> ObservableDictionaryChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _observableDictionaryChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _observableDictionaryChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:ObservableDictionaryChanged" /> event.
        /// </summary>
        /// <param name="observableDictionaryChangedEventArgs">
        ///     The <see cref="ObservableDictionaryChangedEventArgs{TKey,TValue}" /> instance
        ///     containing the event data.
        /// </param>
        protected virtual void RaiseObservableDictionaryChanged(ObservableDictionaryChangedEventArgs<TKey, TValue> observableDictionaryChangedEventArgs)
        {
            if (observableDictionaryChangedEventArgs == null) throw new ArgumentNullException(nameof(observableDictionaryChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            var eventHandler = _observableDictionaryChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, observableDictionaryChangedEventArgs));
            }
        }

        #endregion

        #region Implementation of INotifyObservableChanges

        /// <summary>
        /// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if wanted and applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressChangeNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingChanges = false;

            return Disposable.Create(() =>
            {
                IsTrackingChanges = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        private readonly object _isTrackingChangesLocker = new object();
        private long _isTrackingChanges = 0;

        /// <summary>
        /// Gets a value indicating whether this instance is currently suppressing observable collection changed notifications.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suppressing observable collection changed notifications; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">A Collection Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.</exception>
        public bool IsTrackingChanges
        {
            get { return Interlocked.Read(ref _isTrackingChanges) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingChangesLocker)
                {
                    if (value == false && IsTrackingChanges == false)
                        throw new InvalidOperationException("A Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingChanges, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out KeyValuePair<TKey,TValue>>

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <returns>
        /// The number of elements in the collection. 
        /// </returns>
        public virtual int Count
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerDictionary.Count;
            }
        }

        #endregion

        #region Implementation of IEnumerable<out KeyValuePair<TKey,TValue>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.GetEnumerator();
        }

        #endregion

        #region Implementation of INotifyCollectionChanged


        /// <summary>
        ///     The actual <see cref="CollectionChanged" /> event.
        /// </summary>
        private NotifyCollectionChangedEventHandler _collectionChanged;

        /// <summary>
        ///     Occurs when the collection changed.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _collectionChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _collectionChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="notifyCollectionChangedEventArgs">
        ///     The
        ///     <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null) throw new ArgumentNullException(nameof(notifyCollectionChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            var eventHandler = _collectionChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, notifyCollectionChangedEventArgs));
            }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyDictionary<TKey,TValue>

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <returns>
        /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public virtual bool ContainsKey(TKey key)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"/> interface contains an element that has the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the element that has the specified key in the read-only dictionary.
        /// </summary>
        /// <returns>
        /// The element that has the specified key in the read-only dictionary.
        /// </returns>
        /// <param name="key">The key to locate.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found. </exception>
        public virtual TValue this[TKey key]
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary[key];
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary. 
        /// </summary>
        /// <returns>
        /// An enumerable collection that contains the keys in the read-only dictionary.
        /// </returns>
        public virtual IEnumerable<TKey> Keys
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.Keys;
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <returns>
        /// An enumerable collection that contains the values in the read-only dictionary.
        /// </returns>
        public virtual IEnumerable<TValue> Values
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.Values;
            }
        }

        #endregion

        #region Implementation of INotifyObservableDictionaryItemChanged<out TKey,out TValue>

        /// <summary>
        /// Gets the observable streams of item changes, however these will only have their
        /// <see cref="IObservableDictionaryChange{TKey, TValue}.ChangeType" /> set to <see cref="ObservableDictionaryChangeType.ItemChanged" />.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        public virtual IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return DictionaryChanges
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhileContinuously(change => !IsTrackingChanges)
                    .Where(change => change.ChangeType == ObservableDictionaryChangeType.ItemChanged);
            }
        }

        #endregion

        #region Implementation of INotifyObservableItemChanged<out KeyValuePair<TKey,TValue>>

        /// <summary>
        /// Gets the observable streams of item changes, however these will only have their
        /// <see cref="IObservableCollectionChange{T}.ChangeType"/> set to <see cref="ObservableCollectionChangeType.ItemChanged"/>.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        public IObservable<IObservableCollectionChange<KeyValuePair<TKey, TValue>>> CollectionItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return DictionaryChanges
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhileContinuously(change => !IsTrackingChanges)
                    .SkipWhileContinuously(change => change.ChangeType == ObservableDictionaryChangeType.ItemChanged && !IsTrackingItemChanges)
                    .SkipWhileContinuously(change => change.ChangeType == ObservableDictionaryChangeType.Reset && !IsTrackingResets)
                    .Select(change => change.ToObservableCollectionChange());
            }
        }

        #endregion
    }
}