using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Reactive.Cache.Tests
{
    public class ObservableInMemoryCacheItemsTests
    {
        [Fact]
        public async Task ShouldAddNewItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // then
                cache.Count.Should().Be(2);
            }
        }

        [Fact]
        public async Task ShouldRemoveExistingItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // when
                await cache.Remove(2);

                // then
                cache.Count.Should().Be(1);
            }
        }

        [Fact]
        public async Task ShouldContainExistingItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                var containsAddedKey = await cache.Contains(1);
                var containsNonAddedKey = await cache.Contains(2);

                // then
                containsAddedKey.Should().BeTrue();
                containsNonAddedKey.Should().BeFalse();
            }
        }

        [Fact]
        public async Task ShouldGetExistingItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                var reRetrievedValue = await cache.Get(1);

                // then
                reRetrievedValue.Should().Be("One");
            }
        }
        
        [Fact]
        public async Task ContainsWhichShouldReturnCorrespondingly()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // when
                var resultForNonEmptyList = await cache.ContainsWhich(new List<int>() { 1, 2, 3, 4 }).ToList();
                var resultForEmptyList = await cache.ContainsWhich(new List<int>()).ToList();

                // then
                resultForNonEmptyList.Should().HaveCount(2);
                resultForNonEmptyList.Should().Contain(1);
                resultForNonEmptyList.Should().Contain(2);
                resultForEmptyList.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task ContainsAllShouldReturnCorrespondingly()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // when
                var containsAllAddedKey = await cache.ContainsAll(new [] {1,2});
                var doesNotContainOneOftheAddedKeysAndOneUnAddedOne = await cache.ContainsAll(new [] {2,3});
                var resultForEmptyContainsAllKeys = await cache.ContainsAll(new List<int>());

                // then
                containsAllAddedKey.Should().BeTrue();
                doesNotContainOneOftheAddedKeysAndOneUnAddedOne.Should().BeFalse();
                resultForEmptyContainsAllKeys.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldClearCache()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");
                await cache.Add(3, "Three");

                // when
                await cache.Clear();

                // then
                cache.Count.Should().Be(0);
            }
        }

        [Fact]
        public void ShouldExpireAndRemoveSingleElementForRemovalExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.Count.Should().Be(0);
            }
        }

        [Fact]
        public async Task ShouldExpireAndUpdateSingleElementWithSingleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<int, string> singleKeyUpdater = (i) => i.ToString();

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyUpdater: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });


                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValue = await cache.Get(1);

                cache.Count.Should().Be(1);
                updatedValue.Should().Be("1");
            }
        }

        [Fact]
        public async Task ShouldExpireAndUpdateMultipleElementsWithSingleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<int, string> singleKeyUpdater = (i) => i.ToString();

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyUpdater: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                        await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValueOne = await cache.Get(1);
                var updatedValueTwo = await cache.Get(2);

                cache.Count.Should().Be(2);
                updatedValueOne.Should().Be("1");
                updatedValueTwo.Should().Be("2");
            }
        }

        [Fact]
        public async Task ShouldExpireAndUpdateSingleElementWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysUpdater: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValue = await cache.Get(1);

                cache.Count.Should().Be(1);
                updatedValue.Should().Be("1");
            }
        }

        [Fact]
        public async Task ShouldExpireAndUpdateMultipleElementsWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysUpdater: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                        await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValueOne = await cache.Get(1);
                var updatedValueTwo = await cache.Get(2);

                cache.Count.Should().Be(2);
                updatedValueOne.Should().Be("1");
                updatedValueTwo.Should().Be("2");
            }
        }

        [Fact]
        public void ShouldNotExpireElementWithCorrespondingExpiryTime()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Remove);
                        await cache.Add(2, "Two", TimeSpan.FromDays(10), ObservableCacheExpirationType.Remove);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.Count.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldExpireAndRemoveMultipleElementsWithDifferentExpiryTimesForRemovalExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.Schedule(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove);
                        await cache.Add(2, "Two", TimeSpan.FromTicks(2), ObservableCacheExpirationType.Remove);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.Count.Should().Be(0);
            }
        }

        [Fact]
        public async Task ShouldThrowOnAddingOfItemWithExistingKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                Func<Task> action = async () =>
                {
                    await cache.Add(1, "One");
                };
                
                // then
                action.ShouldThrow<ArgumentException>().WithMessage("The key already existed in the dictionary.");
            }
        }

        [Fact]
        public async Task ShouldThrowOnRemovalOfNonExistingKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                Func<Task> action = async () =>
                {
                    await cache.Remove(2);
                };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }

        [Fact]
        public async Task ShouldThrowOnGetOfNonExistingKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                Func<Task> action = async () =>
                {
                    await cache.Get(2);
                };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }
    }
}