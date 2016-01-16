// -----------------------------------------------------------------------
// <copyright file="ObservableCachedElement.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;

namespace JB.Reactive.Cache
{
    public class ObservableCachedElement<TKey, TValue> : INotifyPropertyChanged, IDisposable
    {
        private ObservableCacheExpirationType _expirationType;
        private DateTime _expiryDateTime;

        private long _hasExpired = 0;
        private readonly object _expiryModificationLocker = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance has expired.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has expired; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasExpired
        {
            get { return Interlocked.Read(ref _hasExpired) == 1; }
            protected set
            {
                if (value == false && HasExpired)
                    throw new InvalidOperationException($"Once {nameof(HasExpired)} has been set, it cannot be reset back to false.");
                
                Interlocked.Exchange(ref _hasExpired, value ? 1 : 0);
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
        ///     Raises the <see cref="ValuePropertyChanged"/> event.
        /// </summary>
        protected virtual void RaiseValuePropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs == null) throw new ArgumentNullException(nameof(propertyChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            _valuePropertyChanged?.Invoke(this, new ForwardedEventArgs<PropertyChangedEventArgs>(Value, propertyChangedEventArgs));
        }

        /// <summary>
        ///     The actual <see cref="Expired" /> event.
        /// </summary>
        private EventHandler _expired;

        /// <summary>
        /// Occurs when this instance has expired.
        /// </summary>

        public virtual event EventHandler Expired
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _expired += value;
            }
            remove
            {
                _expired -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="Expired"/> event.
        /// </summary>
        protected virtual void RaiseExpired()
        {
            if (IsDisposed || IsDisposing)
                return;

            _expired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the timer used internally for expiration handling.
        /// </summary>
        /// <value>
        /// The timer.
        /// </value>
        protected System.Timers.Timer Timer { get; private set; }

        /// <summary>
        /// Prepares and starts the expiration <see cref="Timer" />.
        /// </summary>
        /// <param name="expirationDateTime">The expiration date time.</param>
        protected void SetupAndStartExpirationTimer(DateTime expirationDateTime)
        {
            if(expirationDateTime < DateTime.Now)
                throw new ArgumentOutOfRangeException(nameof(expirationDateTime), "Must be now or in the future");

            CheckForAndThrowIfDisposed();
            
            lock (_expiryModificationLocker)
            {
                ExpiryDateTime = expirationDateTime;
                Timer = new System.Timers.Timer
                {
                    AutoReset = false,
                    Interval = GetTimerIntervalMillisecondsForExpiry(CalculateExpirationTimespan(expirationDateTime))
                };

                Timer.Elapsed += OnExpirationTimerElapsed;
                Timer.Start();
            }
        }

        /// <summary>
        /// Gets the timer interval in milliseconds for the provided <paramref name="expiry"/> <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        private double GetTimerIntervalMillisecondsForExpiry(TimeSpan expiry)
        {
            var interval = expiry > TimeSpan.Zero
                ? (expiry.TotalMilliseconds > Int32.MaxValue ? Int32.MaxValue : expiry.TotalMilliseconds) // Int32.MaxValue is the max value System.Timers.Timer supports..
                : TimeSpan.FromTicks(0).TotalMilliseconds;

            return interval;
        }

        /// <summary>
        /// Calculates the expiration timespan based on the <paramref name="expirationDateTime"/>.
        /// </summary>
        /// <param name="expirationDateTime">The expiration date time.</param>
        /// <returns></returns>
        private TimeSpan CalculateExpirationTimespan(DateTime expirationDateTime)
        {
            var now = DateTime.UtcNow;
            var normalizedExpirationDateTime = expirationDateTime.ToUniversalTime();

            if (now == normalizedExpirationDateTime)
                return TimeSpan.Zero;

            if (now < normalizedExpirationDateTime)
                return expirationDateTime - now;

            // else
            return now - expirationDateTime;
        }

        /// <summary>
        /// Called when the <see cref="System.Timers.Timer.Elapsed"/> event has occured.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="elapsedEventArgs">The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event data.</param>
        private void OnExpirationTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (IsDisposing || IsDisposed)
                return;

            HasExpired = true;

            RemoveValueFromPropertyChangedHandling(Value);

            RaiseExpired();
        }

        /// <summary>
        /// Updates and then restarts the expiration <see cref="Timer" />.
        /// </summary>
        /// <param name="expirationDateTime">The expiration date time.</param>
        protected void UpdateAndRestartExpirationTimer(DateTime expirationDateTime)
        {
            CheckForAndThrowIfDisposed();

            if(HasExpired)
                throw new InvalidOperationException("This instance has already expired.");

            lock (_expiryModificationLocker)
            {
                if (Timer.Enabled)
                {
                    Timer.Stop();
                }

                ExpiryDateTime = expirationDateTime;
                
                Timer.Interval = GetTimerIntervalMillisecondsForExpiry(CalculateExpirationTimespan(expirationDateTime));
                Timer.Start();
            }
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

                if (Equals(_expirationType, value))
                    return;

                _expirationType = value;

                RaisePropertyChanged();
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

                if (HasExpired)
                    throw new InvalidOperationException($"Once {nameof(HasExpired)} has been set, expiration behavior cannot be changed anymore.");

                if (Equals(_expiryDateTime, value))
                    return;

                _expiryDateTime = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCachedElement{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        public ObservableCachedElement(TKey key, TValue value, TimeSpan expiry, ObservableCacheExpirationType expirationType)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (expiry < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be negative");
            if (expiry > TimeSpan.FromMilliseconds(Int32.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be greater than {typeof(Int32).Name}.{nameof(Int32.MaxValue)}");

            Key = key;
            Value = value;

            AddValueToPropertyChangedHandling(Value);

            ExpirationType = expirationType;
            SetupAndStartExpirationTimer(CalculateExpiryDateTime(expiry));
        }

        /// <summary>
        /// Calculates the <see cref="TimeSpan"/> this instance will expire in or already has expired.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> this instance has or will expire in.</returns>
        public virtual TimeSpan ExpiresIn()
        {
            CheckForAndThrowIfDisposed();

            return CalculateExpirationTimespan(DateTime.Now);
        }

        /// <summary>
        /// Calculates the <see cref="DateTime"/> this instance will expire in or already has expired.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> this instance has or will expire in.</returns>
        public virtual DateTime ExpiresWhen()
        {
            CheckForAndThrowIfDisposed();

            return DateTime.Now.Add(ExpiresIn());
        }

        /// <summary>
        /// Adds <see cref="OnValuePropertyChanged"/> as event handler for <paramref name="value"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="value">The value.</param>
        private void AddValueToPropertyChangedHandling(TValue value)
        {
            CheckForAndThrowIfDisposed();

            var valueAsINotifyPropertyChanged = (value as INotifyPropertyChanged);

            if (valueAsINotifyPropertyChanged != null)
            {
                valueAsINotifyPropertyChanged.PropertyChanged += OnValuePropertyChanged;
            }
        }

        /// <summary>
        /// Removes <see cref="OnValuePropertyChanged"/> as event handler for <paramref name="value"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="value">The value.</param>
        private void RemoveValueFromPropertyChangedHandling(TValue value)
        {
            CheckForAndThrowIfDisposed();

            var valueAsINotifyPropertyChanged = (value as INotifyPropertyChanged);

            if (valueAsINotifyPropertyChanged != null)
            {
                valueAsINotifyPropertyChanged.PropertyChanged -= OnValuePropertyChanged;
            }
        }

        /// <summary>
        /// Handles <see cref="INotifyPropertyChanged.PropertyChanged"/> events for <typeparamref name="TValue"/> instances
        /// - if that type implements <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckForAndThrowIfDisposed();

            RaisePropertyChanged(nameof(Value));
            RaiseValuePropertyChanged(e);
        }

        /// <summary>
        /// Updates the expiration <see cref="TimeSpan"/> and <see cref="ObservableCacheExpirationType"/> of this instance.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expirationType">Type of the expiration.</param>
        public virtual void UpdateExpiration(TimeSpan expiry, ObservableCacheExpirationType expirationType)
        {
            if (expiry < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be negative");
            if (expiry > TimeSpan.FromMilliseconds(Int32.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be greater than {typeof(Int32).Name}.{nameof(Int32.MaxValue)}");

            CheckForAndThrowIfDisposed();

            if (HasExpired)
                throw new InvalidOperationException("This instance has already expired.");


            UpdateAndRestartExpirationTimer(CalculateExpiryDateTime(expiry));
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
            if (expiry > TimeSpan.FromMilliseconds(Int32.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(expiry), $"{nameof(expiry)} cannot be greater than {typeof(Int32).Name}.{nameof(Int32.MaxValue)}");

            var now = DateTime.Now;
            var maxExpiry = DateTime.MaxValue - now;

            if (expiry >= maxExpiry)
                return DateTime.MaxValue;

            return now.Add(expiry);
        }

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
                _propertyChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (IsDisposed || IsDisposing)
                return;

            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    if (Timer != null)
                    {
                        if(Timer.Enabled)
                            Timer.Stop();

                        Timer.Dispose();
                        Timer = null;
                    }

                    if (HasExpired == false)
                    {
                        RemoveValueFromPropertyChangedHandling(Value);
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
    }
}