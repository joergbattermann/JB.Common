using System;
using System.Threading;

namespace JB
{
    /// <summary>
    /// A Pooled <typeparamref name="TValue">instance</typeparamref> managed by a <see cref="Pool{TValue}"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Pooled<TValue> : IDisposable
    {
        private long _isDisposed = 0;
        private long _isDisposing = 0;

        private long _hasBeenReleasedBackToPool = 0;
        private long _hasBeenDetachedFromPool = 0;

        private TValue _value;
        private Pool<TValue> _owningPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pooled{TValue}"/> class.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="owningPool">The owning pool.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Pooled(TValue pooledValue, Pool<TValue> owningPool)
        {
            if (owningPool == null)
                throw new ArgumentNullException(nameof(owningPool));

            Value = pooledValue;
            OwningPool = owningPool;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get
            {
                return Interlocked.Read(ref _isDisposed) == 1;
            }
            private set
            {
                Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is being disposing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        private bool IsDisposing
        {
            get
            {
                return Interlocked.Read(ref _isDisposing) == 1;
            }
            set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                if (HasBeenReleasedBackToPool == false && HasBeenDetachedFromPool == false)
                {
                    if (_owningPool == null || _owningPool.IsDisposed)
                    {
                        var valueAsIDisposable = Value as IDisposable;
                        valueAsIDisposable?.Dispose();
                    }
                    else
                    {
                        ReleaseBackToPool();
                    }
                }
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;

                Value = default(TValue);
                OwningPool = null;

                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Implementation of IPooled<TValue>

        /// <summary>
        /// Gets the pooled value.
        /// </summary>
        /// <value>
        /// The pooled value.
        /// </value>
        public TValue Value
        {
            get
            {
                if (IsDisposed) // only check for Disposed as the value IS being returned to pool during disposal, if applicable
                    throw new ObjectDisposedException(this.GetType().Name);

                if (HasBeenReleasedBackToPool)
                    throw new InvalidOperationException("Value has already been released back into the owning pool. This instance can no longer be used.");

                if (HasBeenDetachedFromPool)
                    throw new InvalidOperationException("Value has already been detached from the owning pool and handed over to the caller at that point. This instance can no longer be used.");

                return _value;
            }
            private set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets the owning pool.
        /// </summary>
        /// <value>
        /// The owning pool.
        /// </value>
        public Pool<TValue> OwningPool
        {
            get
            {
                if (IsDisposed) // only check for Disposed as the value IS being returned to pool during disposal, if applicable
                    throw new ObjectDisposedException(this.GetType().Name);

                if(_owningPool != null && _owningPool.IsDisposed)
                    throw new ObjectDisposedException(nameof(OwningPool));

                return _owningPool;
            }
            private set
            {
                _owningPool = value;
            }
        }

        /// <summary>
        /// Detaches the <see cref="Value"/> from the pool and hands it over to the caller.
        /// Once a value has been detached from its pool, this <see cref="Pooled{TValue}"/> allows
        /// no further calls to <see cref="ReleaseBackToPool"/> and <see cref="HasBeenDetachedFromPool"/>,
        /// indicating this via <see cref="HasBeenDetachedFromPool"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public TValue DetachFromPool(CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(this.GetType().Name);

            if (HasBeenReleasedBackToPool)
                throw new InvalidOperationException("Pooled values that have already been released back and returned to the pool can no longer be detached from the pool.");

            if (HasBeenDetachedFromPool)
                throw new InvalidOperationException("Detached pooled values cannot be detached a second time from the pool.");

            return OwningPool.DetachPooledValue(this, cancellationToken);
        }

        /// <summary>
        /// Releases the <see cref="Value"/> back to the <see cref="Pooled{TValue}.OwningPool"/> pool.
        /// Further calls to <see cref="Value"/>, <see cref="ReleaseBackToPool"/> and
        /// <see cref="DetachFromPool"/> are prevented, but locally kept and copied direct
        /// references to the <see cref="Value"/> should no longer be used, also.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public void ReleaseBackToPool(CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsDisposed) // only check for Disposed as the value IS being returned to pool during disposal, if applicable
                throw new ObjectDisposedException(this.GetType().Name);

            if (HasBeenReleasedBackToPool)
                throw new InvalidOperationException("Pooled values that have already been released back and returned to the pool cannot be released a second time.");

            if (HasBeenDetachedFromPool)
                throw new InvalidOperationException("Detached pooled values can no longer be released and returned back into the pool.");

            if (OwningPool.IsDisposed)
            {
                throw new InvalidOperationException("Pooled value cannot be returned back into the pool if the latter has already been disposed.");
            }

            OwningPool.ReleasePooledValue(this, cancellationToken);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> has been released back to pool
        /// and cannot / should no longer be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been released back to pool; otherwise, <c>false</c>.
        /// </value>
        public bool HasBeenReleasedBackToPool
        {
            get
            {
                if (IsDisposed) // only check for Disposed as the value IS being returned to pool during disposal, if applicable
                    throw new ObjectDisposedException(this.GetType().Name);

                return Interlocked.Read(ref _hasBeenReleasedBackToPool) == 1;
            }
            internal set
            {
                Interlocked.Exchange(ref _hasBeenReleasedBackToPool, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> has been detached from pool.
        /// And is no longer owned by the <see cref="OwningPool"/>, nor can it be
        /// <see cref="ReleaseBackToPool">released back</see>to it.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been detached from pool; otherwise, <c>false</c>.
        /// </value>
        public bool HasBeenDetachedFromPool
        {
            get
            {
                if (IsDisposed) // only check for Disposed as the value IS being returned to pool during disposal, if applicable
                    throw new ObjectDisposedException(this.GetType().Name);

                return Interlocked.Read(ref _hasBeenDetachedFromPool) == 1;
            }
            internal set
            {
                Interlocked.Exchange(ref _hasBeenDetachedFromPool, value ? 1 : 0);
            }
        }

        #endregion
    }
}