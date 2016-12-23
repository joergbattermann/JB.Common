// -----------------------------------------------------------------------
// <copyright file="PooledTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace JB.Tests
{
    /// <summary>
    /// Tests for <see cref="Pooled{TValue}"/> instances.
    /// </summary>
    public class PooledTests
    {
        [Fact]
        public async Task CannotBeReturnedToPoolMultipleTimesTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action returningToPoolAction = () => acquiredPooledItem.ReleaseBackToPool();

            // then
            // prior to returning poolsize should be down by one
            returningToPoolAction.ShouldNotThrow();

            // however a second (or more) time(s) should not be allowed
            returningToPoolAction.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public async Task CannotBeDetachedFromPoolOnceReturnedTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action returningToPoolAction = () => acquiredPooledItem.ReleaseBackToPool();
            Action detachingFromPoolAction = () => acquiredPooledItem.DetachFromPool();

            // then
            returningToPoolAction.ShouldNotThrow();
            detachingFromPoolAction.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public async Task CannotBeReturnedToPoolOnceDetachedTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action detachingFromPoolAction = () => acquiredPooledItem.DetachFromPool();
            Action returningToPoolAction = () => acquiredPooledItem.ReleaseBackToPool();

            // then
            detachingFromPoolAction.ShouldNotThrow();
            returningToPoolAction.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public async Task WillNotBeReturnedToPoolAfterPoolDisposalTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            pool.Dispose();

            // then
            acquiredPooledItem.IsDisposed.Should().Be(false);

            Action valueDispose = () => acquiredPooledItem.Dispose();

            valueDispose.ShouldNotThrow<ObjectDisposedException>();
        }

        [Fact]
        public async Task WillBeReturnedToPoolOnDisposalTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // then

            pool.AvailableInstancesCount.Should().Be(0);
            acquiredPooledItem.IsDisposed.Should().Be(false);

            acquiredPooledItem.Dispose();

            pool.AvailableInstancesCount.Should().Be(1);
            acquiredPooledItem.IsDisposed.Should().Be(true);
        }

        [Fact]
        public async Task WillPreventFurtherDirectUsageAfterDisposalTest()
        {
            // given
            var pool = new Pool<string>((token) => Guid.NewGuid().ToString(), 1);
            var acquiredPooledItem = await pool.AcquirePooledValueAsync();

            // when
            Action accessingValueAction = () => { var value = acquiredPooledItem.Value; };

            Action accessingHasBeenDetachedFromPoolAction = () => { var value = acquiredPooledItem.HasBeenDetachedFromPool; };
            Action accessingHasBeenReleasedBackToPoolAction = () => { var value = acquiredPooledItem.HasBeenReleasedBackToPool; };

            acquiredPooledItem.Dispose();

            // then
            accessingValueAction.ShouldThrow<ObjectDisposedException>().And.ObjectName.Should().Be(acquiredPooledItem.GetType().Name);
            accessingHasBeenDetachedFromPoolAction.ShouldThrow<ObjectDisposedException>().And.ObjectName.Should().Be(acquiredPooledItem.GetType().Name);
            accessingHasBeenReleasedBackToPoolAction.ShouldThrow<ObjectDisposedException>().And.ObjectName.Should().Be(acquiredPooledItem.GetType().Name);
        }
    }
}