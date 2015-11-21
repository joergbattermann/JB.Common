// -----------------------------------------------------------------------
// <copyright file="IPool.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace JB
{
    public interface IPool<TValue>
    {
        /// <summary>
        /// Increases the total size of the pool by the amount specified.
        /// </summary>
        /// <param name="increaseBy">The amount of values to increase the pool by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task IncreasePoolSizeAsync(int increaseBy = 1, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Decreases the size of the pool by the amount of instances specified.
        /// This only has an effect if there are any instances currently available to be
        /// acquired, this has no effect on already and currently acquired instances.
        /// </summary>
        /// <param name="decreaseBy">The amount of values to decrease the pool by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task DecreaseAvailablePoolSizeAsync(int decreaseBy = 1, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Acquires the next available pooled value.
        /// </summary>
        /// <param name="pooledValueAcquisitionMode">The pooled value acquisition mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IPooled<TValue>> AcquirePooledValueAsync(PooledValueAcquisitionMode pooledValueAcquisitionMode = PooledValueAcquisitionMode.WaitForNextAvailableInstance,
            CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Gets the count of available, ready-to-be-acquired instances in the pool.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<int> GetAvailableCountAsync(CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Releases a <paramref name="pooledValue"/> back into the pool.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ReleasePooledValueAsync(IPooled<TValue> pooledValue, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Detaches a <paramref name="pooledValue"/> from the pool.
        /// </summary>
        /// <param name="pooledValue">The pooled value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TValue> DetachPooledValueAsync(IPooled<TValue> pooledValue, CancellationToken cancellationToken = default(CancellationToken));
    }
}