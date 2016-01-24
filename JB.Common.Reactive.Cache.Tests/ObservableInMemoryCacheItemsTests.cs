using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using System.Reactive.Linq;
using JB.Collections;
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
        public async Task ShouldUpdateValueForExistingItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                await cache.Update(1, "ONE");
                var updatedValue = await cache.Get(1);

                // then
                cache.Count.Should().Be(1);
                updatedValue.Should().Be("ONE");
            }
        }

        [Fact]
        public async Task ShouldRemoveExistingItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One", TimeSpan.MaxValue);
                await cache.Add(2, "Two", TimeSpan.MaxValue);

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
                await cache.Add(1, "One", TimeSpan.MaxValue);

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
        public async Task ShouldUpdateExpirationCorrectlyForExistingItem()
        {
            // given
            var testScheduler = new TestScheduler();
            var expiresAtTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing);
                    });

                // when
                long tickAtTimeOfUpdate = -1; // this 'remembers' the virtual time the expiration update took place
                testScheduler.ScheduleAsync(
                    TimeSpan.FromTicks(1),
                    async (scheduler, token) =>
                    {
                        tickAtTimeOfUpdate = scheduler.Now.Ticks;
                        await cache.UpdateExpiration(1, TimeSpan.FromTicks(3 * expiresAtTicks));
                    });

                // when
                testScheduler.AdvanceBy(expiresAtTicks);
                var updatedExpiration = await cache.ExpiresIn(1);

                // then
                updatedExpiration.ShouldBeEquivalentTo(TimeSpan.FromTicks((2 * expiresAtTicks) + tickAtTimeOfUpdate));
            }
        }
        
        [Fact]
        public async Task ShouldExpireInAndAtProvideAccurateFutureNowAndPastExpirationInformation()
        {
            // given
            var testScheduler = new TestScheduler();
            var expiresAtTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler))
            {
                await cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing);

                // when
                var expiresIn = await cache.ExpiresIn(1);
                var expiresAt = await cache.ExpiresAt(1);

                // then
                expiresIn.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks));
                expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime.AddTicks(expiresAtTicks));

                // and when
                testScheduler.AdvanceBy(expiresAtTicks);
                expiresIn = await cache.ExpiresIn(1);
                expiresAt = await cache.ExpiresAt(1);

                // then
                expiresIn.ShouldBeEquivalentTo(TimeSpan.Zero);
                expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime);

                // and finally when
                testScheduler.AdvanceBy(expiresAtTicks);
                expiresIn = await cache.ExpiresIn(1);
                expiresAt = await cache.ExpiresAt(1);

                // then
                expiresIn.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks * -1));
                expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime.Subtract(TimeSpan.FromTicks(expiresAtTicks)));
            }
        }

        [Fact]
        public void ShouldExpireAndKeepSingleElementForDoNothingExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);

                // then
                cache.Count.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldThrowKeyHasExpiredExceptionOnGetAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var initialTestSchedulerDateTime = testScheduler.Now.DateTime;
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
                Func<Task> action = async () =>
                {
                    await cache.Get(1);
                };

                // then
                action
                    .ShouldThrow<KeyHasExpiredException<int>>()
                    .WithMessage($"The key has expired on {initialTestSchedulerDateTime}.");
            }
        }

        [Fact]
        public void ShouldThrowKeyHasExpiredExceptionOnUpdateAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var initialTestSchedulerDateTime = testScheduler.Now.DateTime;
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
                Func<Task> action = async () =>
                {
                    await cache.Update(1, "ONE");
                };

                // then
                action
                    .ShouldThrow<KeyHasExpiredException<int>>()
                    .WithMessage($"The key has expired on {initialTestSchedulerDateTime}.");
            }
        }

        [Fact]
        public void ShouldNotThrowKeyHasExpiredExceptionOnGetIfWantedAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
                Func<Task> action = async () =>
                {
                    await cache.Get(1, false);
                };

                // then
                action
                    .ShouldNotThrow<KeyHasExpiredException<int>>();
            }
        }

        [Fact]
        public void ShouldExpireAndRemoveSingleElementForRemovalExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                testScheduler.ScheduleAsync(
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

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
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

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
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

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
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

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
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
        public async Task ShouldThrowAggregateExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnUpdatedValuesForMultipleKeysForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;
            var exceptionsObserver = testScheduler.CreateObserver<ObserverException>();

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i * 10, i => (i * 20).ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler, throwOnExpirationHandlingExceptions: false))
            {
                cache.ObserverExceptions.Subscribe(exceptionsObserver);

                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                        await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                exceptionsObserver.Messages.Count.Should().Be(1);
                exceptionsObserver.Messages.Last().Should().NotBeNull();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.Should().NotBeNull().And.BeOfType<AggregateException>();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Count.Should().Be(2);
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.First().Should().BeOfType<KeyNotFoundException<int>>();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Last().Should().BeOfType<KeyNotFoundException<int>>();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.First().As<KeyNotFoundException<int>>().Key.Should().Be(1);
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Last().As<KeyNotFoundException<int>>().Key.Should().Be(2);
            }
        }

        [Fact]
        public async Task ShouldThrowKeyNotFoundExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnOneUpdatedValueForMultipleKeysForUpdateExpiryType()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;
            var exceptionsObserver = testScheduler.CreateObserver<ObserverException>();

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.Skip(1).ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler, throwOnExpirationHandlingExceptions: false))
            {
                cache.ObserverExceptions.Subscribe(exceptionsObserver);

                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                        await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                exceptionsObserver.Messages.Count.Should().Be(1);
                exceptionsObserver.Messages.Last().Should().NotBeNull();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.Should().NotBeNull().And.BeOfType<KeyNotFoundException<int>>();
                exceptionsObserver.Messages.Last().Value.Value.InnerException.As<KeyNotFoundException<int>>().Key.Should().Be(1);
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
                testScheduler.ScheduleAsync(
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
        public void ShouldExpireElementWithCorrespondingExpiryTimeWhenDue()
        {
            // given
            var testScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
            {
                testScheduler.ScheduleAsync(
                    TimeSpan.Zero,
                    async (scheduler, token) =>
                    {
                        await cache.Add(1, "One", TimeSpan.FromTicks(expirationTimeoutInTicks), ObservableCacheExpirationType.Remove);
                    });

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.Count.Should().Be(1);

                // but when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.Count.Should().Be(0);
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
                testScheduler.ScheduleAsync(
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
                await cache.Add(1, "One", TimeSpan.MaxValue);

                // when
                Func<Task> action = async () =>
                {
                    await cache.Add(1, "One", TimeSpan.MaxValue);
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
                await cache.Add(1, "One", TimeSpan.MaxValue);

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
                await cache.Add(1, "One", TimeSpan.MaxValue);

                // when
                Func<Task> action = async () =>
                {
                    await cache.Get(2);
                };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }

        [Fact]
        public async Task ShouldThrowOnExpiresInOfNonExistingKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                Func<Task> action = async () =>
                {
                    await cache.ExpiresIn(2);
                };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }

        [Fact]
        public async Task ShouldThrowOnExpiresAtOfNonExistingKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                Func<Task> action = async () =>
                {
                    await cache.ExpiresAt(2);
                };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }
    }
}