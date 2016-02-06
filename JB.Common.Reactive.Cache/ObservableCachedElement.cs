// -----------------------------------------------------------------------
// <copyright file="ObservableCachedElement.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Threading;

namespace JB.Reactive.Cache
{
    public class ObservableCachedElement<TKey, TValue> : IDisposable
    {
        private IScheduler _expirationScheduler;
        private ObservableCacheExpirationType _expirationType;
        private DateTime _expiryDateTime;
        private TimeSpan _originalExpiry;

        private IDisposable _expirySchedulerCancellationDisposable;

        private long _expirationChangesCount = 0;
        private long _hasExpired = 0;
        private readonly object _expiryModificationLocker = new object();

        /// <summary>
        /// Gets or sets the expiration changes count.
        /// </summary>
        /// <value>
        /// The expiration changes count.
        /// </value>
        protected long ExpirationChangesCount => Interlocked.Read(ref _expirationChangesCount);

        /// <summary>
        /// Gets or sets a value indicating whether this instance has expired.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has expired; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasExpired
        {
            get
            {
                return Interlocked.Read(ref _hasExpired) == 1;
            }
            protected set
            {
                if (Equals(value, HasExpired))
                    return;
               
                Interlocked.Exchange(ref _hasExpired, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has expiry been updated at least once..
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has expiry been updated; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasExpiryBeenUpdated => ExpirationChangesCount > 0;

        /// <summary>
        ///     The actual <see cref="KeyPropertyChanged" /> event.
        /// </summary>
        private EventHandler<ForwardedEventArgs<PropertyChangedEventArgs>> _keyPropertyChanged;

        /// <summary>
        /// Occurs when this instance's <see cref="Key"/> has raised an <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>

        public virtual event EventHandler<ForwardedEventArgs<PropertyChangedEventArgs>> KeyPropertyChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();

                _keyPropertyChanged += value;
            }
            remove
            {
                _keyPropertyChanged -= value;
            }
        }

        /// <summary>
        ///     The actual <see cref="ValuePropertyChanged" /> event.
        /// </summary>
        private EventHandler<ForwardedEventArgs<PropertyChangedEventArgs>> _valuePropertyChanged;

        /// <summary>
        /// Occurs when this instance's <see cref="Value"/> has raised an <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>

        public virtual event EventHandler<ForwardedEventArgs<PropertyChangedEventArgs>> ValuePropertyChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();

                _valuePropertyChanged += value;
            }
            remove
            {
                _valuePropertyChanged -= value;
            }
        }

        /// <summary>
        ///     Forwards the property changed event for the <paramref name="sender"/>.
        /// </summary>
        protected virtual void RaiseForwardedPropertyChanged(EventHandler<ForwardedEventArgs<PropertyChangedEventArgs>> eventToRaise, object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs == null) throw new ArgumentNullException(nameof(propertyChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            eventToRaise?.Invoke(this, new ForwardedEventArgs<PropertyChangedEventArgs>(sender, propertyChangedEventArgs));
        }

        /// <summary>
        /// Stops an ongoing expiration timer (if any) and uses the <paramref name="expirationScheduler" /> to schedule a new expiration
        /// notification on the given <paramref name="expirationObserver" /> after the given <paramref name="expiry" />.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expirationObserver">The expiration observer to notify upon this instance's expiration.</param>
        /// <param name="observerExceptionsObserver">The <see cref="ObserverException"/> observer to notify, well.. observer exceptions on.</param>
        /// <param name="expirationScheduler">The expiration scheduler to schedule the expiration notification on.</param>
        /// <param name="isUpdate">if set to <c>true</c> indicats that this is an update, rather than an initial call (via .ctor).</param>
        private void CreateOrUpdateExpiration(
            TimeSpan expiry,
            IObserver<ObservableCachedElement<TKey, TValue>> expirationObserver,
            IObserver<ObserverException> observerExceptionsObserver,
            IScheduler expirationScheduler,
            bool isUpdate)
        {
            if (expirationObserver == null) throw new ArgumentNullException(nameof(expirationObserver));
            if (observerExceptionsObserver == null) throw new ArgumentNullException(nameof(observerExceptionsObserver));
            if (expirationScheduler == null) throw new ArgumentNullException(nameof(expirationScheduler));

            if (expiry < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be negative");
            
            CheckForAndThrowIfDisposed();

            lock (_expiryModificationLocker)
            {
                // cancel an existing, scheduled expiration
                try
                {
                    _expirySchedulerCancellationDisposable?.Dispose();
                    _expirySchedulerCancellationDisposable = null;
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException($"An error occured cancelling a scheduled expiration of a {this.GetType().Name} instance.", exception);

                    observerExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                        throw;
                }

                try
                {
                    ExpirationScheduler = expirationScheduler;

                    // set 'new' expiry datetime
                    ExpiryDateTime = CalculateExpiryDateTime(expiry);
                    OriginalExpiry = expiry;

                    if (isUpdate)
                    {
                        // and...
                        if (HasExpired)
                        {
                            // reset hasexpired flag
                            HasExpired = false;

                            // and re-add to value / property changed handling and forwarding
                            AddKeyAndValueToPropertyChangedHandling();
                        }
                    }
                    else
                    {
                        // otherwise, if this is basically the first call to this method & thereby initial start
                        // the value needs to be added to value / property changed handling and forwarding, too
                        AddKeyAndValueToPropertyChangedHandling();
                    }

                    // and finally schedule expiration on scheduler for given time
                    // IF TimeSpan.MaxValue hasn't been specified
                    if (expiry < TimeSpan.MaxValue)
                    {
                        _expirySchedulerCancellationDisposable = ExpirationScheduler.Schedule(expiry,
                            () =>
                            {
                                try
                                {
                                    lock (_expiryModificationLocker)
                                    {
                                        RemoveKeyAndValueFromPropertyChangedHandling();
                                        HasExpired = true;
                                    }
                                    
                                    expirationObserver.OnNext(this);
                                }
                                catch (Exception exception)
                                {
                                    var observerException = new ObserverException($"An error occured notifying about the expiration of a {this.GetType().Name} instance.", exception);

                                    observerExceptionsObserver.OnNext(observerException);

                                    if (observerException.Handled == false)
                                        throw;
                                }
                            });
                    }
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException($"An error occured updating expiration data of a {this.GetType().Name} instance.", exception);

                    observerExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                        throw;
                }
                finally
                {
                    Interlocked.Increment(ref _expirationChangesCount);
                }
            }
        }
        
        /// <summary>
        /// Calculates the expiration timespan based on the <paramref name="expirationDateTime"/>.
        /// </summary>
        /// <param name="expirationDateTime">The expiration date time.</param>
        /// <returns></returns>
        private TimeSpan CalculateExpirationTimespan(DateTime expirationDateTime)
        {
            return expirationDateTime.ToUniversalTime() - ExpirationScheduler.Now.UtcDateTime;
        }
        
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TValue Value { get; }

        /// <summary>
        /// Gets the type of the cache expiration.
        /// </summary>
        /// <value>
        /// The type of the cache expiration.
        /// </value>
        public ObservableCacheExpirationType ExpirationType
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _expirationType;
            }
            private set
            {
                CheckForAndThrowIfDisposed();

                if (HasExpired)
                    throw new InvalidOperationException($"Once {nameof(HasExpired)} has been set, expiration behavior cannot be changed anymore.");

                _expirationType = value;
            }
        }

        /// <summary>
        /// Gets or sets the original expiry - this is either the one specified on creation of this instance or,
        /// if updated, the one specified for the latest update.
        /// </summary>
        /// <value>
        /// The original expiry.
        /// </value>
        public TimeSpan OriginalExpiry
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _originalExpiry;
            }
            protected set
            {
                CheckForAndThrowIfDisposed();

                _originalExpiry = value;
            }
        }

        /// <summary>
        /// Gets the expiry <see cref="DateTime"/>.
        /// </summary>
        /// <value>
        /// The expiry <see cref="DateTime"/>.
        /// </value>
        protected DateTime ExpiryDateTime
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _expiryDateTime;
            }
            private set
            {
                CheckForAndThrowIfDisposed();
                
                _expiryDateTime = value;
            }
        }

        /// <summary>
        /// Gets the expiration <see cref="IScheduler"/> to schedule the expiration notification on.
        /// </summary>
        /// <value>
        /// The expiration <see cref="IScheduler"/>.
        /// </value>
        protected IScheduler ExpirationScheduler
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _expirationScheduler;
            }
            private set
            {
                CheckForAndThrowIfDisposed(false);
                
                _expirationScheduler = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCachedElement{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The cached value.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        public ObservableCachedElement(TKey key, TValue value, TimeSpan expiry, ObservableCacheExpirationType expirationType)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (expiry < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be negative");

            Key = key;
            Value = value;

            // ToDo: usage of OriginalExpiry / expiry is a dirty hack - this needs to be done more properly sometimes down the road
            OriginalExpiry = expiry;
            ExpirationType = expirationType;
        }

        /// <summary>
        /// Calculates the <see cref="TimeSpan"/> this instance will expire in or already has expired.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> this instance has or will expire in.</returns>
        public virtual TimeSpan ExpiresIn()
        {
            CheckForAndThrowIfDisposed();

            return CalculateExpirationTimespan(ExpiryDateTime);
        }

        /// <summary>
        /// Calculates the <see cref="DateTime"/> this instance will expire in or already has expired.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> this instance has or will expire in.</returns>
        public virtual DateTime ExpiresAt()
        {
            CheckForAndThrowIfDisposed();

            return ExpirationScheduler.Now.UtcDateTime.Add(ExpiresIn());
        }

        /// <summary>
        /// Adds <see cref="OnPropertyChanged"/> as event handler for <see cref="Key"/> and <see cref="Value"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        protected virtual void AddKeyAndValueToPropertyChangedHandling()
        {
            CheckForAndThrowIfDisposed();

            var keyAsINotifyPropertyChanged = (Key as INotifyPropertyChanged);
            if (keyAsINotifyPropertyChanged != null)
                keyAsINotifyPropertyChanged.PropertyChanged += OnPropertyChanged;

            var valueAsINotifyPropertyChanged = (Value as INotifyPropertyChanged);
            if (valueAsINotifyPropertyChanged != null)
                valueAsINotifyPropertyChanged.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Removes <see cref="OnPropertyChanged"/> as event handler for <see cref="Key"/> and <see cref="Value"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        protected virtual void RemoveKeyAndValueFromPropertyChangedHandling()
        {
            var keyAsINotifyPropertyChanged = (Key as INotifyPropertyChanged);
            if (keyAsINotifyPropertyChanged != null)
                keyAsINotifyPropertyChanged.PropertyChanged -= OnPropertyChanged;

            var valueAsINotifyPropertyChanged = (Value as INotifyPropertyChanged);
            if (valueAsINotifyPropertyChanged != null)
                valueAsINotifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Handles <see cref="INotifyPropertyChanged.PropertyChanged"/> events for <typeparamref name="TValue"/> instances
        /// - if that type implements <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckForAndThrowIfDisposed();
            
            if (sender is TKey)
                RaiseForwardedPropertyChanged(_keyPropertyChanged, sender, e);
            else if (sender is TValue)
                RaiseForwardedPropertyChanged(_valuePropertyChanged, sender, e);
        }

        /// <summary>
        /// Stops an ongoing expiration timer and uses the <paramref name="expirationScheduler" /> to schedule a new expiration
        /// notification on the given <paramref name="expirationObserver" /> after the given <paramref name="expiry" />.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expirationObserver">The expiration observer to notify upon this instance's expiration.</param>
        /// <param name="observerExceptionsObserver">The <see cref="ObserverException"/> observer to notify, well.. observer exceptions on.</param>
        /// <param name="expirationScheduler">The expiration scheduler to schedule the expiration notification on.</param>
        public virtual void StartOrUpdateExpiration(
            TimeSpan expiry,
            IObserver<ObservableCachedElement<TKey, TValue>> expirationObserver,
            IObserver<ObserverException> observerExceptionsObserver,
            IScheduler expirationScheduler)
        {
            CreateOrUpdateExpiration(expiry, expirationObserver, observerExceptionsObserver, expirationScheduler, ExpirationChangesCount > 0);
        }

        /// <summary>
        /// Stops the expiration notification.
        /// </summary>
        public virtual void StopExpiration()
        {
            CheckForAndThrowIfDisposed();

            if (HasExpired)
                return;

            _expirySchedulerCancellationDisposable?.Dispose();
            _expirySchedulerCancellationDisposable = null;
        }

        /// <summary>
        /// Calculates the (valid) expiry date time.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        protected DateTime CalculateExpiryDateTime(TimeSpan expiry)
        {
            if (expiry < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be negative");

            if (expiry == TimeSpan.MaxValue)
                return DateTime.MaxValue;

            var now = ExpirationScheduler.Now.UtcDateTime;
            var maxExpiry = DateTime.MaxValue - now;

            if (expiry >= maxExpiry)
                return DateTime.MaxValue;

            return now.Add(expiry);
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
                    _expirySchedulerCancellationDisposable?.Dispose();
                    _expirySchedulerCancellationDisposable = null;

                    if (!HasExpired)
                    {
                        RemoveKeyAndValueFromPropertyChangedHandling();
                    }

                    _expirationScheduler = null;
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

        /// <summary>
        ///     Performs an implicit conversion from <see><cref>JB.Reactive.Cache.ObservableCachedElement{TKey, TValue}</cref></see>
        ///     to <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}" />.
        /// </summary>
        /// <param name="observableCachedElement">The observable cached element.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator KeyValuePair<TKey, TValue>(ObservableCachedElement<TKey, TValue> observableCachedElement)
        {
            return new KeyValuePair<TKey, TValue>(observableCachedElement.Key, observableCachedElement.Value);
        }
    }
}