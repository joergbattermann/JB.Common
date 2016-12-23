// -----------------------------------------------------------------------
// <copyright file="PoolTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace JB.Tests
{
    /// <summary>
    /// Tests for <see cref="Pool{TValue}"/> instances.
    /// </summary>
    public class PoolTests
    {
        [Fact]
        public void PoolInitializationWithoutAnyInitialInstancesTest()
        {
            // given

            // when
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString());

            // then
            pool.AvailableInstancesCount.Should().Be(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(1000)]
        public void PoolInitializationWithInitialInstanceCountTest(int initialCount)
        {
            // given

            // when
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), initialCount);

            // then
            pool.AvailableInstancesCount.Should().Be(initialCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(1000)]
        public void PoolInitializationWithInitialInstancesTest(int initialCount)
        {
            // given
            var initialInstances = new List<string>();
            for (int i = 0; i < initialCount; i++)
            {
                initialInstances.Add(Guid.NewGuid().ToString());
            }

            // when
            var pool = new Pool<string>((token) => DateTime.UtcNow.Ticks.ToString(), initialInstances);

            // then
            pool.AvailableInstancesCount.Should().Be(initialCount);
        }

        [Fact]
        public void PoolInitializationWithInitialInstanceViaParamsTest()
        {
            // given

            // when
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            // then
            pool.AvailableInstancesCount.Should().Be(3);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 10)]
        [InlineData(10, 100)]
        public async Task PoolCanIncreaseAvailableInstancesTest(int initialInstanceCount, int instancesToIncreaseBy)
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), initialInstanceCount);
            pool.TotalInstancesCount.Should().Be(initialInstanceCount);

            // when
            await pool.IncreasePoolSizeAsync(instancesToIncreaseBy);
            pool.TotalInstancesCount.Should().Be(initialInstanceCount + instancesToIncreaseBy);

            // then
            pool.AvailableInstancesCount.Should().Be(initialInstanceCount + instancesToIncreaseBy);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 0)]
        [InlineData(10, 10)]
        [InlineData(100, 10)]
        public async Task PoolCanDecreaseAvailableInstancesTest(int initialInstanceCount, int instancesToDecreaseBy)
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), initialInstanceCount);
            pool.TotalInstancesCount.Should().Be(initialInstanceCount);

            // when
            await pool.DecreaseAvailablePoolSizeAsync(instancesToDecreaseBy);
            pool.TotalInstancesCount.Should().Be(initialInstanceCount - instancesToDecreaseBy);

            // then
            pool.AvailableInstancesCount.Should().Be(initialInstanceCount - instancesToDecreaseBy);
        }

        [Fact]
        public void PoolCannotDecreaseAvailableInstancesWithNegativeSizeTest()
        {
            // given
            var emptyPool = new Pool<string>((token) => Guid.NewGuid().ToString());

            // when
            Func<Task> invalidDecreaseOnEmptyPool = async () => await emptyPool.DecreaseAvailablePoolSizeAsync(-1);

            // then
            invalidDecreaseOnEmptyPool.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("decreaseBy");
        }

        [Fact]
        public void PoolCannotDecreaseAvailableInstancesBelow0Test()
        {
            // given
            var emptyPool = new Pool<string>((token) => Guid.NewGuid().ToString());
            var nonEmptyPool = new Pool<string>((token) => Guid.NewGuid().ToString(), 10);

            // when
            Func<Task> invalidDecreaseOnEmptyPool = async () => await emptyPool.DecreaseAvailablePoolSizeAsync(1);
            Func<Task> invalidDecreaseOnNonEmptyPool = async () => await nonEmptyPool.DecreaseAvailablePoolSizeAsync(11);

            // then
            invalidDecreaseOnEmptyPool.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("decreaseBy");
            invalidDecreaseOnNonEmptyPool.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("decreaseBy");
        }

        [Fact]
        public async Task EmptyPoolReturnsDefaultValueOnAcquisitionWhenRequestedTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString());

            // when
            var acquiredPooledItem = await pool.AcquirePooledValueAsync(PooledValueAcquisitionMode.AvailableInstanceOrDefaultValue);

            // then
            acquiredPooledItem.Should().Be(default(Pooled<string>));
            pool.AvailableInstancesCount.Should().Be(0);
        }

        [Fact]
        public async Task EmptyPoolPerformsInstanceCreationOnAcquisitionWhenRequestedTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString());

            // when
            var acquiredPooledItem = await pool.AcquirePooledValueAsync(PooledValueAcquisitionMode.AvailableInstanceOrCreateNewOne);

            // then
            acquiredPooledItem.Should().NotBeNull();
            acquiredPooledItem.Value.Should().NotBeNull();
            pool.AvailableInstancesCount.Should().Be(0);
        }

        [Fact]
        public async Task AcquiredInstancesCannotBeDetachedMultipleTimesTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action detachFromPoolAction = () => pool.DetachPooledValue(acquiredPooledItem);

            // then
            // prior to returning poolsize should be down by one
            detachFromPoolAction.ShouldNotThrow();

            // however a second (or more) time(s) should not be allowed
            detachFromPoolAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task AcquiredInstancesCannotBeReturnedMultipleTimesTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action returningToPoolAction = () => pool.ReleasePooledValue(acquiredPooledItem);

            // then
            // prior to returning poolsize should be down by one
            returningToPoolAction.ShouldNotThrow();
            
            // however a second (or more) time(s) should not be allowed
            returningToPoolAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task AcquiredInstancesCanBeDetachedFromOwningPoolTest(int poolSize)
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), poolSize);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();
            var acquiredPooledItemValue = acquiredPooledItem.Value;

            // when
            var detachedValue = pool.DetachPooledValue(acquiredPooledItem);
            
            // then
            pool.TotalInstancesCount.Should().Be(poolSize - 1);
            pool.AvailableInstancesCount.Should().Be(poolSize - 1);
            detachedValue.Should().Be(acquiredPooledItemValue);

            acquiredPooledItem.HasBeenReleasedBackToPool.Should().Be(false);
            acquiredPooledItem.HasBeenDetachedFromPool.Should().Be(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task AcquiredInstancesCanBeReturnedToOwningPoolTest(int poolSize)
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), poolSize);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action returningToPoolAction = () => pool.ReleasePooledValue(acquiredPooledItem);
            Action accessingPoolValueAfterReturningToPoolAction = () => { var pooledValue = acquiredPooledItem.Value; };

            // then
            // prior to returning poolsize should be down by one
            pool.AvailableInstancesCount.Should().Be(poolSize -1);

            returningToPoolAction.ShouldNotThrow();

            // after returning poolsize should be back to expected size
            pool.AvailableInstancesCount.Should().Be(poolSize);
            acquiredPooledItem.HasBeenReleasedBackToPool.Should().Be(true);
            acquiredPooledItem.HasBeenDetachedFromPool.Should().Be(false);

            accessingPoolValueAfterReturningToPoolAction.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public async Task AcquiredInstancesCannotBeDetachedFromDifferentPoolTest()
        {
            // given
            var owningPool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var secondPool = new Pool<string>((token) => Guid.NewGuid().ToString());

            var acquiredPooledItem = await owningPool.AcquirePooledValueAsync();

            // when
            Action detachFromPoolAction = () => secondPool.DetachPooledValue(acquiredPooledItem);

            // then
            detachFromPoolAction.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("pooledValue");

            owningPool.AvailableInstancesCount.Should().Be(0);
            acquiredPooledItem.HasBeenReleasedBackToPool.Should().Be(false);
            acquiredPooledItem.HasBeenDetachedFromPool.Should().Be(false);
        }

        [Fact]
        public async Task AcquiredInstancesCannotBeReturnedToDifferentPoolTest()
        {
            // given
            var owningPool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var secondPool = new Pool<string>((token) => Guid.NewGuid().ToString());

            var acquiredPooledItem = await owningPool.AcquirePooledValueAsync();

            // when
            Action returningToPoolAction = () => secondPool.ReleasePooledValue(acquiredPooledItem);
            Action accessingPoolValueAfterReturningToDifferentPoolAction = () => { var pooledValue = acquiredPooledItem.Value; };

            // then
            returningToPoolAction.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("pooledValue");

            owningPool.AvailableInstancesCount.Should().Be(0);
            acquiredPooledItem.HasBeenReleasedBackToPool.Should().Be(false);
            acquiredPooledItem.HasBeenDetachedFromPool.Should().Be(false);

            accessingPoolValueAfterReturningToDifferentPoolAction.ShouldNotThrow<InvalidOperationException>();
        }

        [Fact]
        public void PoolCurrentlyEmptyWaitsForNextAvailableReturnedValueOnAcquisitionWhenRequestedTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);

            // when
            Pooled<string> waitingConsumerAcquiredPooledValue = default(Pooled<string>);
            Func<Task> taskArrangement = () =>
            {
                var initialConsumerHasAcquiredValue = false;
                var secondActuallyWaitingForAcquisitionConsumerIsWaiting = false;
                var releaseValueFlagForInitialConsumer = false;

                Func<Task> initialAcquisitionTask = async () =>
                {
                    var pooledValue = await pool.AcquirePooledValueAsync();
                    initialConsumerHasAcquiredValue = true;

                    while (releaseValueFlagForInitialConsumer == false)
                    {
                        await Task.Yield();
                    }

                    pooledValue.ReleaseBackToPool();
                };

                Func<Task> secondAndSupposedlyWaitingAcquisitionTask = async () =>
                {
                    while (initialConsumerHasAcquiredValue == false)
                    {
                        await Task.Yield();
                    }

                    secondActuallyWaitingForAcquisitionConsumerIsWaiting = true;
                    waitingConsumerAcquiredPooledValue = await pool.AcquirePooledValueAsync(PooledValueAcquisitionMode.AvailableInstanceOrWaitForNextOne);
                };

                Func<Task> acquistionAndReleaseSignalWatcherTask = async () =>
                {
                    while (initialConsumerHasAcquiredValue == false || secondActuallyWaitingForAcquisitionConsumerIsWaiting == false)
                    {
                        await Task.Yield();
                    }

                    releaseValueFlagForInitialConsumer = true;
                };

                return Task.WhenAll(Task.Run(initialAcquisitionTask), Task.Run(secondAndSupposedlyWaitingAcquisitionTask), Task.Run(acquistionAndReleaseSignalWatcherTask));
            };

            // then
            taskArrangement.ShouldNotThrow();
            waitingConsumerAcquiredPooledValue.Should().NotBe(default(Pooled<string>));
            waitingConsumerAcquiredPooledValue.Value.Should().NotBe(default(string));
            pool.AvailableInstancesCount.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1000)]
        public async Task NonEmptyPoolAllowsInstanceAcquisitionTest(int initialInstanceCount)
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), initialInstanceCount);

            // when
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // then
            acquiredPooledItem.Should().NotBeNull();
            pool.AvailableInstancesCount.Should().Be(initialInstanceCount - 1);
        }
    }
}