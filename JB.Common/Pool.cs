﻿// -----------------------------------------------------------------------
// <copyright file="Pool.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JB
{
    /// <summary>
    /// A managed pool for shared, re-usable <typeparam name="TValue">instances</typeparam>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Pool<TValue> : IDisposable
    {
        private long _isDisposed = 0;
        private long _isDisposing = 0;

        /// <summary>
        /// Gets the instance builder.
        /// </summary>
        /// <value>
        /// The instance builder.
        /// </value>
        private Func<TValue> InstanceBuilder { get; set; }

        /// <summary>
        /// Gets the pooled instances.
        /// </summary>
        /// <value>
        /// The pooled instances.
        /// </value>
        private ConcurrentQueue<TValue> PooledInstances { get; set; }

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
        /// Gets the count of available, ready-to-be-acquired instances in the pool.
        /// </summary>
        /// <value>
        /// The available instances count.
        /// </value>
        public int AvailableInstancesCount
        {
            get
            {
                if (IsDisposed || IsDisposing)
                    throw new ObjectDisposedException(nameof(Pool<TValue>));

                return PooledInstances?.Count ?? 0;
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
        /// Initializes a new instance of the <see cref="Pool{TValue}" /> class.
        /// </summary>
        /// <param name="instanceBuilder">The instance builder.</param>
        public Pool(Func<TValue> instanceBuilder)
                    : this(instanceBuilder, 0)
        {
            if (instanceBuilder == null)
                throw new ArgumentNullException(nameof(instanceBuilder));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{TValue}" /> class.
        /// </summary>
        /// <param name="instanceBuilder">The instance builder.</param>
        /// <param name="initialInstances">The initial instances.</param>
        public Pool(Func<TValue> instanceBuilder, params TValue[] initialInstances)
                            : this(instanceBuilder, initialInstances ?? Enumerable.Empty<TValue>())
        {
            if (instanceBuilder == null)
                throw new ArgumentNullException(nameof(instanceBuilder));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{TValue}"/> class.
        /// </summary>
        /// <param name="instanceBuilder">The instance builder.</param>
        /// <param name="initialPoolSize">Initial size of the pool.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public Pool(Func<TValue> instanceBuilder, int initialPoolSize)
        {
            if (instanceBuilder == null)
                throw new ArgumentNullException(nameof(instanceBuilder));

            if (initialPoolSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialPoolSize));

            InstanceBuilder = instanceBuilder;
            PooledInstances = new ConcurrentQueue<TValue>(Enumerable.Range(0, initialPoolSize).Select(_ => instanceBuilder.Invoke()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{TValue}"/> class.
        /// </summary>
        /// <param name="instanceBuilder">The instance builder.</param>
        /// <param name="initialInstances">The initial instances.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Pool(Func<TValue> instanceBuilder, IEnumerable<TValue> initialInstances)
        {
            if (instanceBuilder == null)
                throw new ArgumentNullException(nameof(instanceBuilder));

            InstanceBuilder = instanceBuilder;
            PooledInstances = initialInstances != null
                ? new ConcurrentQueue<TValue>(initialInstances)
                : new ConcurrentQueue<TValue>();
        }

        #region Implementation of IPool<TValue>

        /// <summary>
        /// Increases the total size of the pool by the amount specified.
        /// </summary>
        /// <param name="increaseBy">The amount of values to increase the pool by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task IncreasePoolSizeAsync(int increaseBy = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(nameof(Pool<TValue>));

            if (increaseBy < 0)
                throw new ArgumentOutOfRangeException(nameof(increaseBy));

            for (int i = 0; i < increaseBy; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var instance = InstanceBuilder.Invoke();
                await Task.Run(() =>
                {
                    PooledInstances.Enqueue(instance);
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Decreases the size of the pool by the amount of instances specified.
        /// This only has an effect if there are any instances currently available to be
        /// acquired, this has no effect on already and currently acquired instances.
        /// </summary>
        /// <param name="decreaseBy">The amount of values to decrease the pool by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Cannot decrease the amount of (available) pooled items by less than 0
        /// or
        /// Cannot decrease the amount of (available) pooled items by more than what's available.
        /// </exception>
        public async Task DecreaseAvailablePoolSizeAsync(int decreaseBy = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(nameof(Pool<TValue>));

            if (decreaseBy < 0)
                throw new ArgumentOutOfRangeException(nameof(decreaseBy), "Cannot decrease the amount of (available) pooled items by less than 0");

            if (decreaseBy > PooledInstances.Count)
                throw new ArgumentOutOfRangeException(nameof(decreaseBy), "Cannot decrease the amount of (available) pooled items by more than what's available.");

            for (int i = 0; i < decreaseBy; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (PooledInstances.IsEmpty)
                    return; // nothing more to do - the queue is empty

                TValue dequeuedValue;
                while (PooledInstances.IsEmpty == false && PooledInstances.TryDequeue(out dequeuedValue) == false)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Acquires the next available pooled value.
        /// </summary>
        /// <param name="pooledValueAcquisitionMode">The pooled value acquisition mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Pooled<TValue>> AcquirePooledValueAsync(
            PooledValueAcquisitionMode pooledValueAcquisitionMode = PooledValueAcquisitionMode.AvailableInstanceOrDefaultValue,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(nameof(Pool<TValue>));

            TValue value = default(TValue);

            // check whether we actually currently have any instances left
            if (PooledInstances.IsEmpty)
            {
                if (pooledValueAcquisitionMode == PooledValueAcquisitionMode.AvailableInstanceOrDefaultValue)
                {
                    return default(Pooled<TValue>);
                }
                else if (pooledValueAcquisitionMode == PooledValueAcquisitionMode.AvailableInstanceOrCreateNewOne)
                {
                    value = InstanceBuilder.Invoke();
                }
                else
                {
                    while (PooledInstances.IsEmpty || PooledInstances.TryDequeue(out value) == false)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await Task.Yield();
                    }
                }
            }
            else
            {
                // try to retrieve the next available / queued value
                if (PooledInstances.TryDequeue(out value) == false)
                {
                    if (pooledValueAcquisitionMode == PooledValueAcquisitionMode.AvailableInstanceOrDefaultValue)
                    {
                        return default(Pooled<TValue>);
                    }
                    else if (pooledValueAcquisitionMode == PooledValueAcquisitionMode.AvailableInstanceOrCreateNewOne) // build a new one
                    {
                        value = InstanceBuilder.Invoke();
                    }
                    else
                    {
                        // keep trying
                        while (PooledInstances.TryDequeue(out value) == false)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await Task.Yield();
                        }
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new Pooled<TValue>(value, this);
        }

        /// <summary>
        /// Releases a <paramref name="pooledValue"/> back into the pool.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public void ReleasePooledValue(Pooled<TValue> pooledValue, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(nameof(Pool<TValue>));

            if (pooledValue == null)
                throw new ArgumentNullException(nameof(pooledValue));

            if (pooledValue.OwningPool != this)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Only pooled values managed by this pool can be released back into the pool.");

            if (pooledValue.HasBeenReleasedBackToPool)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Pooled values that have already been released back and returned to the pool cannot be released a second time.");

            if (pooledValue.HasBeenDetachedFromPool)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Detached pooled values can no longer be released and returned back into the pool.");

            // else
            cancellationToken.ThrowIfCancellationRequested();

            PooledInstances.Enqueue(pooledValue.Value);
            pooledValue.HasBeenReleasedBackToPool = true;
        }

        /// <summary>
        /// Detaches a <paramref name="pooledValue"/> from the pool.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public TValue DetachPooledValue(Pooled<TValue> pooledValue, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsDisposed || IsDisposing)
                throw new ObjectDisposedException(nameof(Pool<TValue>));

            if (pooledValue == null)
                throw new ArgumentNullException(nameof(pooledValue));

            if (pooledValue.OwningPool != this)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Only pooled values managed by this pool can be detached from it.");

            if (pooledValue.HasBeenReleasedBackToPool)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Pooled values that have already been released back and returned to the pool can no longer be detached from the pool.");

            if (pooledValue.HasBeenDetachedFromPool)
                throw new ArgumentOutOfRangeException(nameof(pooledValue), "Detached pooled values cannot be detached a second time from the pool.");

            // else
            cancellationToken.ThrowIfCancellationRequested();

            var result = pooledValue.Value;
            pooledValue.HasBeenDetachedFromPool = true;

            return result;
        }

        #endregion

        #region Implementation of IDisposable

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
                if (PooledInstances.IsEmpty)
                    return;

                while (PooledInstances.IsEmpty == false)
                {
                    TValue value;
                    while (PooledInstances.TryDequeue(out value) == false)
                    {
                        if (PooledInstances.IsEmpty)
                            break;
                    }

                    var disposableValue = value as IDisposable;
                    disposableValue?.Dispose();
                }
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;

                PooledInstances = null;
                InstanceBuilder = null;

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}