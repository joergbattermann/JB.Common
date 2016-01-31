using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using JB.Collections.Reactive.ExtensionMethods;
using JB.Reactive;
using JB.Reactive.Linq;

namespace JB.Collections.Reactive
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableCollection<T> : IObservableCollection<T>, ICollection, IDisposable
    {
        protected const string ItemIndexerName = "Item[]"; // taken from ObservableCollection.cs Line #421

        private IDisposable _innerListChangedRelevantCollectionChangedEventsForwader;
        private IDisposable _collectionChangesItemIndexerPropertyChangedForwarder;
        private IDisposable _countChangesCountPropertyChangedForwarder;

        private Subject<IObservableCollectionChange<T>> _collectionChangesSubject = new Subject<IObservableCollectionChange<T>>();
        private Subject<int> _countChangesSubject = new Subject<int>();
        private Subject<ObserverException> _observerExceptionsSubject = new Subject<ObserverException>();

        /// <summary>
        /// Gets the count changes observer.
        /// </summary>
        /// <value>
        /// The count changes observer.
        /// </value>
        protected IObserver<int> CountChangesObserver { get; private set; }

        /// <summary>
        /// Gets the thrown exceptions observer.
        /// </summary>
        /// <value>
        /// The thrown exceptions observer.
        /// </value>
        protected IObserver<ObserverException> ObserverExceptionsObserver { get; private set; }

        /// <summary>
        /// Gets the collection changes observer.
        /// </summary>
        /// <value>
        /// The collection changes observer.
        /// </value>
        protected IObserver<IObservableCollectionChange<T>> CollectionChangesObserver { get; private set; }

        /// <summary>
        ///     Gets the inner list.
        /// </summary>
        /// <value>
        ///     The inner list.
        /// </value>
        protected SynchronizedBindingList<T> InnerList { get; }

        /// <summary>
        ///     Gets the used scheduler.
        /// </summary>
        /// <value>
        ///     The scheduler.
        /// </value>
        protected IScheduler Scheduler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="list">The initial list, if any.</param>
        /// <param name="syncRoot">The object used to synchronize access to the thread-safe collection.</param>
        /// <param name="scheduler">The scheduler to to send out observer messages &amp; raise events on. If none is provided <see cref="System.Reactive.Concurrency.Scheduler.CurrentThread"/> will be used.</param>
        public ObservableCollection(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
        {
            // ToDo: check whether scheduler shall / should be used for internall used RX notifications / Subjects etc
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;

            InnerList = new SynchronizedBindingList<T>(list, syncRoot ?? new object());

            ThresholdAmountWhenChangesAreNotifiedAsReset = 100;

            IsTrackingChanges = true;
            IsTrackingItemChanges = true;
            IsTrackingCountChanges = true;
            IsTrackingResets = true;

            SetupObservablesAndObserversAndSubjects();
        }


        #region Implementation of INotifyObservableItemChanged<out T>

        /// <summary>
        /// Gets the observable streams of item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        public virtual IObservable<IObservableCollectionChange<T>> CollectionItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CollectionChanges
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(change => !IsTrackingChanges)
                    .Where(change => change.ChangeType == ObservableCollectionChangeType.ItemChanged);
            }
        }

        #endregion

        #region Implementation of INotifyObservableCollectionChanges<T>


        /// <summary>
        ///     (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressChangeNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed(false);

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
        ///     Gets or sets a value indicating whether this instance is tracking and notifying about all collection changes.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking and notifying about collection changes; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingChanges
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return Interlocked.Read(ref _isTrackingChanges) == 1;
            }
            protected set
            {
                CheckForAndThrowIfDisposed(false);

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
            get
            {
                CheckForAndThrowIfDisposed(false);
                return Interlocked.Read(ref _isTrackingResets) == 1;
            }
            protected set
            {
                CheckForAndThrowIfDisposed(false);

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
        ///     (Temporarily) suppresses change notifications for <see cref="ObservableCollectionChangeType.Reset" /> events until
        ///     the returned <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed(false);

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


        /// <summary>
        ///     Gets the collection change notifications as an observable stream.
        /// </summary>
        /// <value>
        ///     The collection changes.
        /// </value>
        public virtual IObservable<IObservableCollectionChange<T>> CollectionChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _collectionChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(change => !IsTrackingChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableCollectionChangeType.ItemChanged && !IsTrackingItemChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableCollectionChangeType.Reset && !IsTrackingResets);
            }
        }


        /// <summary>
        ///     Gets the reset notifications as an observable stream.  Whenever signaled,
        ///     observers should reset any knowledge / state etc about the list.
        /// </summary>
        /// <value>
        ///     The resets.
        /// </value>
        public virtual IObservable<Unit> Resets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CollectionChanges
                    .Where(change => change.ChangeType == ObservableCollectionChangeType.Reset)
                    .SkipContinuouslyWhile(_ => IsTrackingResets == false)
                    .Select(_ => Unit.Default);
            }
        }

        /// <summary>
        /// Provides an observable sequence of <see cref="ObserverException">exceptions</see> thrown by observers.
        /// An <see cref="ObserverException" /> provides a <see cref="ObserverException.Handled" /> property, if set to [true] by
        /// any of the observers of <see cref="ObserverExceptions" /> observable, it is assumed to be safe to continue
        /// without re-throwing the exception.
        /// </summary>
        /// <value>
        /// An observable stream of unhandled exceptions.
        /// </value>
        public virtual IObservable<ObserverException> ObserverExceptions
        {
            get
            {
                CheckForAndThrowIfDisposed();

                // not caring about IsDisposing / IsDisposed on purpose once subscribed, so corresponding Exceptions are forwarded 'til the "end" to already existing subscribers
                return _observerExceptionsSubject;
            }
        }
        
        #endregion
        
        #region Implementation of INotifyObservableCountChanges

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
            get
            {
                CheckForAndThrowIfDisposed(false);
                return Interlocked.Read(ref _isTrackingCountChanges) == 1;
            }
            protected set
            {
                CheckForAndThrowIfDisposed(false);

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
        ///     Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        ///     The count changes.
        /// </value>
        public virtual IObservable<int> CountChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _countChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(_ => !IsTrackingCountChanges)
                    .DistinctUntilChanged();
            }
        }


        /// <summary>
        ///     (Temporarily) suppresses item count change notification until the returned <see cref="IDisposable" />
        ///     has been Disposed.
        /// </summary>
        /// <param name="signalCurrentCountWhenFinished">
        ///     if set to <c>true</c> signals a the <see cref="IReadOnlyCollection{T}.Count" />
        ///     when finished.
        /// </param>
        /// <returns></returns>
        public virtual IDisposable SuppressCountChangeNotifications(bool signalCurrentCountWhenFinished = true)
        {
            CheckForAndThrowIfDisposed(false);

            IsTrackingCountChanges = false;

            return Disposable.Create(() =>
            {
                IsTrackingCountChanges = true;

                if (signalCurrentCountWhenFinished)
                {
                    CountChangesObserver.OnNext(Count);
                }
            });
        }

        #endregion
        
        #region Implementation of INotifyObservableItemChanged

        /// <summary>
        ///     (Temporarily) suppresses change notifications for (single) item changes until the returned
        ///     <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressItemChangeNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed(false);

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
        ///     Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        ///     <see cref="INotifyPropertyChanged.PropertyChanged" /> events, if the interface is implemented.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed(false);
                return Interlocked.Read(ref _isTrackingItemChanges) == 1;
            }
            protected set
            {
                CheckForAndThrowIfDisposed(false);

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
        public int ThresholdAmountWhenChangesAreNotifiedAsReset
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

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            CheckForAndThrowIfDisposed();

            return InnerList.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        public virtual event PropertyChangedEventHandler PropertyChanged
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

        #region Helpers

        /// <summary>
        /// Prepares and sets up the observables and subjects used, particularly
        /// <see cref="_collectionChangesSubject"/>, <see cref="_countChangesSubject"/> and <see cref="_observerExceptionsSubject"/>
        /// but also internally used RX subscriptions for <see cref="IBindingList.ListChanged"/> and somewhat hack-ish
        /// 'Count' and 'Items[]' <see cref="INotifyPropertyChanged"/> events on <see cref="CountChanges"/> and <see cref="CollectionChanges"/>
        /// occurrences (for WPF / Binding)
        /// </summary>
        private void SetupObservablesAndObserversAndSubjects()
        {
            ObserverExceptionsObserver = _observerExceptionsSubject.NotifyOn(Scheduler);
            CollectionChangesObserver = _collectionChangesSubject.NotifyOn(Scheduler);
            CountChangesObserver = _countChangesSubject.NotifyOn(Scheduler);
            
            // then connect to InnerList's ListChanged Event
            _innerListChangedRelevantCollectionChangedEventsForwader = System.Reactive.Linq.Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
                handler => InnerList.ListChanged += handler,
                handler => InnerList.ListChanged -= handler)
                .TakeWhile(_ => !IsDisposing && !IsDisposed)
                .SkipContinuouslyWhile(_ => !IsTrackingChanges)
                .Where(eventPattern => eventPattern?.EventArgs != null)
                .SelectMany(eventPattern => eventPattern.EventArgs.ToObservableCollectionChanges(InnerList))
                .ObserveOn(Scheduler)
                .Subscribe(
                    NotifyObserversAboutCollectionChanges,
                    exception =>
                    {
                        // ToDo: at this point this instance is practically doomed / no longer forwarding any events & therefore further usage of the instance itself should be prevented, or the observable stream should re-connect/signal-and-swallow exceptions. Either way.. not ideal.
                        var observerException = new ObserverException(
                            $"An error occured notifying observers of this {this.GetType().Name} - consistency and future notifications are no longer guaranteed.",
                            exception);
                        ObserverExceptionsObserver.OnNext(observerException);
                    });


            // 'Count' and 'Item[]' PropertyChanged events are used by WPF typically via / for ObservableCollections, see
            // http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs,421
            _countChangesCountPropertyChangedForwarder = CountChanges
                .ObserveOn(Scheduler)
                .Subscribe(_ => RaisePropertyChanged(nameof(Count)));

            _collectionChangesItemIndexerPropertyChangedForwarder = CollectionChanges
                .ObserveOn(Scheduler)
                .Subscribe(_ => RaisePropertyChanged(ItemIndexerName));
        }

        /// <summary>
        ///     Determines whether the amount of changed items is greater than the reset threshold and / or the minimum amount of
        ///     items to be considered as a reset.
        /// </summary>
        /// <param name="affectedItemsCount">The items changed / affected.</param>
        /// <param name="maximumAmountOfItemsChangedToBeConsideredResetThreshold">
        ///     The maximum amount of changed items count to
        ///     consider a change or a range of changes a reset.
        /// </param>
        /// <returns></returns>
        protected virtual bool IsItemsChangedAmountGreaterThanResetThreshold(int affectedItemsCount, int maximumAmountOfItemsChangedToBeConsideredResetThreshold)
        {
            if (affectedItemsCount <= 0) throw new ArgumentOutOfRangeException(nameof(affectedItemsCount));
            if (maximumAmountOfItemsChangedToBeConsideredResetThreshold < 0) throw new ArgumentOutOfRangeException(nameof(maximumAmountOfItemsChangedToBeConsideredResetThreshold));

            // check for '0' thresholds
            if (maximumAmountOfItemsChangedToBeConsideredResetThreshold == 0)
                return true;

            return affectedItemsCount >= maximumAmountOfItemsChangedToBeConsideredResetThreshold;
        }

        /// <summary>
        ///     Notifies all <see cref="CollectionChanges" /> and <see cref="Resets" /> subscribers and
        ///     raises the (observable)collection changed events.
        /// </summary>
        /// <param name="observableCollectionChange">The observable collection change.</param>
        protected virtual void NotifyObserversAboutCollectionChanges(IObservableCollectionChange<T> observableCollectionChange)
        {
            if (observableCollectionChange == null)
                throw new ArgumentNullException(nameof(observableCollectionChange));

            CheckForAndThrowIfDisposed();

            // go ahead and check whether a Reset or item add, -change, -move or -remove shall be signaled
            // .. based on the ThresholdAmountWhenChangesAreNotifiedAsReset value
            var actualObservableCollectionChange =
                (observableCollectionChange.ChangeType == ObservableCollectionChangeType.Reset
                 || IsItemsChangedAmountGreaterThanResetThreshold(1, ThresholdAmountWhenChangesAreNotifiedAsReset))
                    ? ObservableCollectionChange<T>.Reset
                    : observableCollectionChange;

            // raise events and notify about collection changes

            if (actualObservableCollectionChange.ChangeType == ObservableCollectionChangeType.ItemAdded
                || actualObservableCollectionChange.ChangeType == ObservableCollectionChangeType.ItemRemoved
                || actualObservableCollectionChange.ChangeType == ObservableCollectionChangeType.Reset)
            {
                try
                {
                    CountChangesObserver.OnNext(Count);
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException(
                        $"An error occured notifying {nameof(CountChanges)} Observers of this {this.GetType().Name}.",
                        exception);

                    ObserverExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                        throw;
                }
            }

            try
            {
                CollectionChangesObserver.OnNext(actualObservableCollectionChange);
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException(
                    $"An error occured notifying {nameof(CollectionChanges)} Observers of this {this.GetType().Name}.",
                    exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
            
            try
            {
                RaiseCollectionChanged(actualObservableCollectionChange.ToNotifyCollectionChangedEventArgs());
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException(
                    $"An error occured notifying CollectionChanged Subscribers of this {this.GetType().Name}.",
                    exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
        }
        
        #endregion Helpers

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
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;
            try
            {
                IsDisposing = true;

                Dispose(true);
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;
                
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                // and clear inner dictionary early on
                IDisposable notificationSuppression = !IsTrackingChanges
                    ? Disposable.Empty
                    : SuppressChangeNotifications(false);
                InnerList.Clear();
                notificationSuppression?.Dispose();

                if (_innerListChangedRelevantCollectionChangedEventsForwader != null)
                {
                    _innerListChangedRelevantCollectionChangedEventsForwader.Dispose();
                    _innerListChangedRelevantCollectionChangedEventsForwader = null;
                }

                if (_collectionChangesItemIndexerPropertyChangedForwarder != null)
                {
                    _collectionChangesItemIndexerPropertyChangedForwarder.Dispose();
                    _collectionChangesItemIndexerPropertyChangedForwarder = null;
                }

                if (_countChangesCountPropertyChangedForwarder != null)
                {
                    _countChangesCountPropertyChangedForwarder.Dispose();
                    _countChangesCountPropertyChangedForwarder = null;
                }

                var countChangesObserverAsDisposable = CountChangesObserver as IDisposable;
                countChangesObserverAsDisposable?.Dispose();
                CountChangesObserver = null;

                if (_countChangesSubject != null)
                {
                    _countChangesSubject.Dispose();
                    _countChangesSubject = null;
                }

                var collectionChangesObserverAsDisposable = CollectionChangesObserver as IDisposable;
                collectionChangesObserverAsDisposable?.Dispose();
                CollectionChangesObserver = null;

                if (_collectionChangesSubject != null)
                {
                    _collectionChangesSubject.Dispose();
                    _collectionChangesSubject = null;
                }

                var thrownExceptionsObserverAsDisposable = ObserverExceptionsObserver as IDisposable;
                thrownExceptionsObserverAsDisposable?.Dispose();
                ObserverExceptionsObserver = null;

                if (_observerExceptionsSubject != null)
                {
                    _observerExceptionsSubject.Dispose();
                    _observerExceptionsSubject = null;
                }
            }
        }

        /// <summary>
        /// Checks whether this instance has been disposed, optionally whether it is currently being disposed.
        /// </summary>
        /// <param name="checkIsDisposing">if set to <c>true</c> checks whether disposal is currently ongoing, indicated via <see cref="IsDisposing"/>.</param>
        protected virtual void CheckForAndThrowIfDisposed(bool checkIsDisposing = true)
        {
            if (checkIsDisposing && IsDisposing)
            {
                throw new ObjectDisposedException(GetType().Name, "This instance is currently being disposed.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        /// <summary>
        ///     The actual <see cref="INotifyCollectionChanged.CollectionChanged" /> event.
        /// </summary>
        private NotifyCollectionChangedEventHandler _collectionChanged;
        
        /// <summary>
        ///     Occurs when the collection changed.
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
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

            // only raise event if it's currently allowed
            if (!IsTrackingChanges
                || (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Replace && !IsTrackingItemChanges)
                || (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset && !IsTrackingResets))
            {
                return;
            }

            var eventHandler = _collectionChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, notifyCollectionChangedEventArgs));
            }
        }

        #endregion

        #region Implementation of ICollection
        
        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based
        ///     indexing.
        /// </param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="array" /> is multidimensional.-or- The number of elements
        ///     in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from
        ///     <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source
        ///     <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination
        ///     <paramref name="array" />.
        /// </exception>
        public virtual void CopyTo(Array array, int index)
        {
            CheckForAndThrowIfDisposed();

            ((IList)InnerList).CopyTo(array, index);
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        public virtual object SyncRoot
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return ((ICollection)InnerList).SyncRoot;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
        ///     (thread safe).
        /// </summary>
        /// <returns>
        ///     true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise,
        ///     false.
        /// </returns>
        public virtual bool IsSynchronized
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((ICollection)InnerList).IsSynchronized;
            }
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out T>

        /// <summary>
        ///     Gets the number of elements contained in this instance.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in this instance.
        /// </returns>
        public virtual int Count
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.Count;
            }
        }

        #endregion

        #region Implementation of ICollection<out T>

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public virtual void Add(T item)
        {
            CheckForAndThrowIfDisposed();

            InnerList.Add(item);
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public virtual void Clear()
        {
            CheckForAndThrowIfDisposed();

            InnerList.Clear();
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public virtual bool Contains(T item)
        {
            CheckForAndThrowIfDisposed();
            return InnerList.Contains(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The number of elements in the source
        ///     <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from
        ///     <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// </exception>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            CheckForAndThrowIfDisposed();

            InnerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
        ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public virtual bool Remove(T item)
        {
            CheckForAndThrowIfDisposed();

            return InnerList.Remove(item);
        }

        /// <summary>
        ///     Gets a value indicating whether the instance is read-only.
        /// </summary>
        /// <returns>
        ///     true if the instance is read-only; otherwise, false.
        /// </returns>
        public virtual bool IsReadOnly
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((ICollection<T>)InnerList).IsReadOnly;
            }
        }

        #endregion

        #region Implementation of IObservableCollection<T>

        /// <summary>
        ///     Adds a range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public virtual void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // ToDo: this needs (more) unit tests... something seems fishy in here.

            CheckForAndThrowIfDisposed();

            var itemsAsList = items.ToList();

            if (itemsAsList.Count == 0)
                return;

            // check whether change(s) shall be notified as individual changes OR as one final reset at the end
            var useResetInsteadOfIndividualChanges = IsItemsChangedAmountGreaterThanResetThreshold(itemsAsList.Count, ThresholdAmountWhenChangesAreNotifiedAsReset);
            var signalIndividualItemChanges = !useResetInsteadOfIndividualChanges;
            
            var originalRaiseListChangedEvents = InnerList.RaiseListChangedEvents;
            try
            {
                // signal or suppress list changed events.. depending on check above and whether the collection currently IS tracking changes, at all
                InnerList.RaiseListChangedEvents = signalIndividualItemChanges && IsTrackingChanges;

                // perform the actual range-add call but suppress raising of reset even explicitly
                InnerList.AddRange(itemsAsList, false);
            }
            finally
            {
                InnerList.RaiseListChangedEvents = originalRaiseListChangedEvents;

                // after resetting the InnerList.RaiseListChangedEvents property, raise Reset event, if 'still' desired
                if (useResetInsteadOfIndividualChanges && IsTrackingResets)
                {
                    NotifyObserversAboutCollectionChanges(ObservableCollectionChange<T>.Reset);
                }
            }
        }
        
        /// <summary>
        ///     Removes the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // ToDo: this needs (more) unit tests... something seems fishy in here.

            CheckForAndThrowIfDisposed();

            var itemsAsList = items.ToList();

            if (itemsAsList.Count == 0)
                return;

            // check whether change(s) shall be notified as individual changes OR as one final reset at the end
            var useResetInsteadOfIndividualChanges = IsItemsChangedAmountGreaterThanResetThreshold(itemsAsList.Count, ThresholdAmountWhenChangesAreNotifiedAsReset);
            var signalIndividualItemChanges = !useResetInsteadOfIndividualChanges;

            var originalRaiseListChangedEvents = InnerList.RaiseListChangedEvents;
            try
            {
                // signal or suppress list changed events.. depending on check above and whether the collection currently IS tracking changes, at all
                InnerList.RaiseListChangedEvents = signalIndividualItemChanges && IsTrackingChanges;

                // perform the actual range-remove call but suppress raising of reset even explicitly
                InnerList.RemoveRange(itemsAsList, false);
            }
            finally
            {
                InnerList.RaiseListChangedEvents = originalRaiseListChangedEvents;

                // after resetting the InnerList.RaiseListChangedEvents property, raise Reset event, if 'still' desired
                if (useResetInsteadOfIndividualChanges && IsTrackingResets)
                {
                    NotifyObserversAboutCollectionChanges(ObservableCollectionChange<T>.Reset);
                }
            }
        }

        /// <summary>
        /// Resets this instance and signals subscribers / binding consumers accordingly.
        /// </summary>
        public virtual void Reset()
        {
            CheckForAndThrowIfDisposed();

            InnerList.ResetBindings();
        }

        #endregion
    }
}