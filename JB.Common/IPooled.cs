using System;
using System.Threading;
using System.Threading.Tasks;

namespace JB
{
    public class Pooled<TValue> : IPooled<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pooled{TValue}"/> class.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="owningPool">The owning pool.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Pooled(TValue pooledValue, IPool<TValue> owningPool)
        {
            if (owningPool == null)
                throw new ArgumentNullException(nameof(owningPool));

            PooledValue = pooledValue;
            OwningPool = owningPool;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IPooled<TValue>

        /// <summary>
        /// Gets the pooled value.
        /// </summary>
        /// <value>
        /// The pooled value.
        /// </value>
        public TValue PooledValue { get; }

        /// <summary>
        /// Gets the owning pool.
        /// </summary>
        /// <value>
        /// The owning pool.
        /// </value>
        public IPool<TValue> OwningPool { get; }

        /// <summary>
        /// Detaches the <see cref="IPooled{TValue}.PooledValue"/> from the pool and hands it over to the caller.
        /// Once a value has been detached from its pool, this <see cref="IPooled{TValue}"/> allows
        /// no further calls to <see cref="IPooled{TValue}.ReleaseBackToPoolAsync"/> and <see cref="IPooled{TValue}.DetachFromPoolAsync"/>,
        /// indicating this via <see cref="IPooled{TValue}.HasBeenDetachedFromPool"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<TValue> DetachFromPoolAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases the <see cref="IPooled{TValue}.PooledValue"/> back to the <see cref="IPooled{TValue}.OwningPool"/> pool.
        /// Further calls to <see cref="IPooled{TValue}.PooledValue"/>, <see cref="IPooled{TValue}.ReleaseBackToPoolAsync"/> and
        /// <see cref="IPooled{TValue}.DetachFromPoolAsync"/> are prevented, but locally kept & copied direct
        /// references to the <see cref="IPooled{TValue}.PooledValue"/> should no longer be used, also.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task ReleaseBackToPoolAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPooled{TValue}.PooledValue"/> has been released back to pool
        /// and cannot / should no longer be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been released back to pool; otherwise, <c>false</c>.
        /// </value>
        public bool HasBeenReleasedBackToPool { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPooled{TValue}.PooledValue"/> has been detached from pool.
        /// And is no longer owned by the <see cref="IPooled{TValue}.OwningPool"/>, nor can it be
        /// <see cref="IPooled{TValue}.ReleaseBackToPoolAsync">released back</see>to it.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been detached from pool; otherwise, <c>false</c>.
        /// </value>
        public bool HasBeenDetachedFromPool { get; }

        #endregion
    }

    public interface IPooled<TValue> : IDisposable
    {
        /// <summary>
        /// Gets the pooled value.
        /// </summary>
        /// <value>
        /// The pooled value.
        /// </value>
        TValue PooledValue { get; }

        /// <summary>
        /// Gets the owning pool.
        /// </summary>
        /// <value>
        /// The owning pool.
        /// </value>
        IPool<TValue> OwningPool { get; }

        /// <summary>
        /// Detaches the <see cref="PooledValue"/> from the pool and hands it over to the caller.
        /// Once a value has been detached from its pool, this <see cref="IPooled{TValue}"/> allows
        /// no further calls to <see cref="ReleaseBackToPoolAsync"/> and <see cref="DetachFromPoolAsync"/>,
        /// indicating this via <see cref="HasBeenDetachedFromPool"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TValue> DetachFromPoolAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Releases the <see cref="PooledValue"/> back to the <see cref="OwningPool"/> pool.
        /// Further calls to <see cref="PooledValue"/>, <see cref="ReleaseBackToPoolAsync"/> and
        /// <see cref="DetachFromPoolAsync"/> are prevented, but locally kept & copied direct
        /// references to the <see cref="PooledValue"/> should no longer be used, also.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ReleaseBackToPoolAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a value indicating whether the <see cref="PooledValue"/> has been released back to pool
        /// and cannot / should no longer be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been released back to pool; otherwise, <c>false</c>.
        /// </value>
        bool HasBeenReleasedBackToPool { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="PooledValue"/> has been detached from pool.
        /// And is no longer owned by the <see cref="OwningPool"/>, nor can it be
        /// <see cref="ReleaseBackToPoolAsync">released back</see>to it.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been detached from pool; otherwise, <c>false</c>.
        /// </value>
        bool HasBeenDetachedFromPool { get; }
    }
}