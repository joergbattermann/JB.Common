// -----------------------------------------------------------------------
// <copyright file="ObservableInMemoryCache.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using JB.Collections.Reactive;
using JB.Reactive.Cache.ExtensionMethods;
using JB.Reactive.Linq;

namespace JB.Reactive.Cache
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableInMemoryCache<TKey, TValue> : IObservableCache<TKey, TValue>, IDisposable
    {
        private Subject<IObservableCacheChange<TKey, TValue>> _cacheChangesSubject = new Subject<IObservableCacheChange<TKey, TValue>>();
        private Subject<ObserverException> _unhandledObserverExceptionsSubject = new Subject<ObserverException>();
        
        /// <summary>
        /// Gets the cache changes observer.
        /// </summary>
        /// <value>
        /// The cache changes observer.
        /// </value>
        protected IObserver<IObservableCacheChange<TKey, TValue>> CacheChangesObserver { get; private set; }

        /// <summary>
        /// Gets the thrown exceptions observer.
        /// </summary>
        /// <value>
        /// The thrown exceptions observer.
        /// </value>
        protected IObserver<ObserverException> UnhandledObserverExceptionsObserver { get; private set; }

        /// <summary>
        /// Gets or sets the inner dictionary.
        /// </summary>
        /// <value>
        /// The inner dictionary.
        /// </value>
        protected ObservableDictionary<TKey, ObservableCachedElement<TKey, TValue>> InnerDictionary { get; private set; }

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        protected IScheduler Scheduler { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TKey"/> comparer.
        /// </value>
        protected IEqualityComparer<TKey> KeyComparer { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> implementation to use when comparing values.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TValue"/> comparer.
        /// </value>
        protected IEqualityComparer<TValue> ValueComparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ObservableInMemoryCache" />.
        /// </summary>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing values.</param>
        /// <param name="scheduler">The scheduler to to send out observer messages & raise events on. If none is provided <see cref="System.Reactive.Concurrency.Scheduler.CurrentThread"/> will be used.</param>
        public ObservableInMemoryCache(
            IEqualityComparer<TKey> keyComparer = null,
            IEqualityComparer<TValue> valueComparer = null,
            IScheduler scheduler = null)
        {
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.CurrentThread;

            KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            InnerDictionary = new ObservableDictionary<TKey, ObservableCachedElement<TKey, TValue>>(
                keyComparer: KeyComparer,
                valueComparer: new ObservableCachedElementValueEqualityComparer<TKey, TValue>(ValueComparer),
                scheduler: scheduler);

            ThresholdAmountWhenChangesAreNotifiedAsReset = Int32.MaxValue;

            SetupObservablesAndObserversAndSubjects();
        }

        #region Helper Methods

        /// <summary>
        /// Prepares and sets up the observables and subjects used, particularly
        /// <see cref="_cacheChangesSubject"/> and <see cref="_unhandledObserverExceptionsSubject"/>.
        /// </summary>
        private void SetupObservablesAndObserversAndSubjects()
        {
            // prepare subjects for RX
            UnhandledObserverExceptionsObserver = _unhandledObserverExceptionsSubject.NotifyOn(Scheduler);
            CacheChangesObserver = _cacheChangesSubject.NotifyOn(Scheduler);
        }

        /// <summary>
        /// Adds <see cref="OnCachedElementValuePropertyChanged"/> as event handler for <paramref name="cachedElement"/>'s <see cref="ObservableCachedElement{TKey,TValue}.ValuePropertyChanged"/> event.
        /// </summary>
        /// <param name="cachedElement">The value.</param>
        private void AddToForwardedPropertyChangedHandling(ObservableCachedElement<TKey, TValue> cachedElement)
        {
            CheckForAndThrowIfDisposed();

            if (cachedElement != null)
            {
                cachedElement.ValuePropertyChanged += OnCachedElementValuePropertyChanged;
            }
        }

        /// <summary>
        /// Called when a cached element's value property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="forwardedEventArgs">The <see cref="ForwardedEventArgs{PropertyChangedEventArgs}"/> instance containing the event data.</param>
        private void OnCachedElementValuePropertyChanged(object sender, ForwardedEventArgs<PropertyChangedEventArgs> forwardedEventArgs)
        {
            if (forwardedEventArgs == null)
                throw new ArgumentNullException(nameof(forwardedEventArgs));

            var senderAsObservableCachedElement = sender as ObservableCachedElement<TKey, TValue>;
            if (senderAsObservableCachedElement == null)
                throw new InvalidOperationException($"This should not happen - {nameof(sender)} must be {typeof(ObservableCachedElement<TKey, TValue>).Name} instances");

            try
            {
                CacheChangesObserver.OnNext(ObservableCacheChange<TKey, TValue>.ItemChanged(
                    senderAsObservableCachedElement.Key,
                    senderAsObservableCachedElement.Value,
                    forwardedEventArgs.OriginalEventArgs.PropertyName,
                    senderAsObservableCachedElement.ExpiresAt(),
                    senderAsObservableCachedElement.ExpirationType));
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException(
                    $"An error occured notifying {nameof(Changes)} Observers of this {this.GetType().Name}.",
                    exception);

                UnhandledObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
        }

        /// <summary>
        /// Removes <see cref="OnCachedElementValuePropertyChanged"/> as event handler for <paramref name="cachedElement"/>'s <see cref="ObservableCachedElement{TKey,TValue}.ValuePropertyChanged"/> event.
        /// </summary>
        /// <param name="cachedElement">The value.</param>
        private void RemoveFromForwardedPropertyChangedHandling(ObservableCachedElement<TKey, TValue> cachedElement)
        {
            CheckForAndThrowIfDisposed();

            if (cachedElement != null)
            {
                cachedElement.ValuePropertyChanged -= OnCachedElementValuePropertyChanged;
            }
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
                    var cacheChangesObserverAsDisposable = CacheChangesObserver as IDisposable;
                    cacheChangesObserverAsDisposable?.Dispose();
                    CacheChangesObserver = null;

                    if (_cacheChangesSubject != null)
                    {
                        _cacheChangesSubject.Dispose();
                        _cacheChangesSubject = null;
                    }

                    var thrownExceptionsObserverAsDisposable = UnhandledObserverExceptionsObserver as IDisposable;
                    thrownExceptionsObserverAsDisposable?.Dispose();
                    UnhandledObserverExceptionsObserver = null;

                    if (_unhandledObserverExceptionsSubject != null)
                    {
                        _unhandledObserverExceptionsSubject.Dispose();
                        _unhandledObserverExceptionsSubject = null;
                    }

                    if (InnerDictionary != null)
                    {
                        var innerDictionaryAsDisposable = InnerDictionary as IDisposable;
                        innerDictionaryAsDisposable?.Dispose();
                        InnerDictionary = null;
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

        #region Implementation of INotifyUnhandledObserverExceptions

        /// <summary>
        /// Provides an observable sequence of unhandled <see cref="ObserverException">exceptions</see> thrown by observers.
        /// An <see cref="ObserverException" /> provides a <see cref="ObserverException.Handled" /> property, if set to [true] by
        /// any of the observers of <see cref="UnhandledObserverExceptions" /> observable, it is assumed to be safe to continue
        /// without re-throwing the exception.
        /// </summary>
        /// <value>
        /// An observable stream of unhandled exceptions.
        /// </value>
        public virtual IObservable<ObserverException> UnhandledObserverExceptions
        {
            get
            {
                CheckForAndThrowIfDisposed();

                // not caring about IsDisposing / IsDisposed on purpose once subscribed, so corresponding Exceptions are forwarded 'til the "end" to already existing subscribers
                return _unhandledObserverExceptionsSubject.Merge(InnerDictionary.UnhandledObserverExceptions);
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

            return InnerDictionary.SuppressChangeNotifications(signalResetWhenFinished);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently suppressing observable change notifications of any kind.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suppressing observable change notifications; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">A Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.</exception>
        public virtual bool IsTrackingChanges
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.IsTrackingChanges;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of the minimum amount of changes to switch individual notifications to a reset one.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered as a reset.
        /// </value>
        public int ThresholdAmountWhenChangesAreNotifiedAsReset
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset;
            }
            set
            {
                CheckForAndThrowIfDisposed();

                InnerDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset = value;
            }
        }

        #endregion

        #region Implementation of INotifyObservableCacheChanges<out TKey,out TValue>

        /// <summary>
        ///     Gets an observable stream of changes to the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <value>
        ///     The changes.
        /// </value>
        public virtual IObservable<IObservableCacheChange<TKey, TValue>> Changes
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.DictionaryChanges
                        .SkipContinuouslyWhile(change => change.ChangeType != ObservableDictionaryChangeType.ItemChanged) // this must be handled here separately from the underlying dictionary
                        .Select(dictionaryChange => dictionaryChange.ToObservableCacheChange())
                    .Merge(_cacheChangesSubject)
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(change => !IsTrackingChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.ItemChanged && !IsTrackingItemChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.ItemReplaced && !IsTrackingItemChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.Reset && !IsTrackingResets);
            }
        }

        #endregion

        #region Implementation of IObservableCache<TKey,TValue>

        /// <summary>
        /// Gets the count of keys in this instance.
        /// </summary>
        /// <value>
        /// The count of keys in this instance.
        /// </value>
        public virtual int Count
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.Count;
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
        public virtual bool IsTrackingResets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.IsTrackingResets;
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

                return InnerDictionary
                    .Resets
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(_ => !IsTrackingResets);
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

            return InnerDictionary.SuppressResetNotifications(signalResetWhenFinished);
        }

        #endregion

        #region Implementation of INotifyObservableCacheItemChanges<out TKey,out TValue>

        /// <summary>
        /// Gets the observable streams of collection item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        public virtual IObservable<IObservableCacheChange<TKey, TValue>> ItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();
                
                return Changes
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .Where(change => change.ChangeType == ObservableCacheChangeType.ItemChanged || change.ChangeType == ObservableCacheChangeType.ItemReplaced)
                    .SkipContinuouslyWhile(change => !IsTrackingItemChanges);
            }
        }

        #endregion

        #region Implementation of INotifyObservableItemChanges

        /// <summary>
        /// (Temporarily) suppresses item change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.SuppressItemChangedNotifications(signalResetWhenFinished);
        }

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if that interface is implemented, too.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsTrackingItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.IsTrackingItemChanges;
            }
        }

        #endregion
    }
}