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
                var containsAllAddedKey = await cache.ContainsAll(new[] { 1, 2 });
                var doesNotContainOneOftheAddedKeysAndOneUnAddedOne = await cache.ContainsAll(new[] { 2, 3 });
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
        public async Task ShouldContainAllCurrentKeys()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");
                await cache.Add(3, "Three");

                // when
                var keys = cache.CurrentKeys;

                // then
                keys.Count.Should().Be(3);

                keys.Should().Contain(1);
                keys.Should().Contain(2);
                keys.Should().Contain(3);
            }
        }

        [Fact]
        public async Task ShouldProvideExistingAndAddedKeys()
        {
            // given
            var testScheduler = new TestScheduler();
            var expiresAtTicks = 10;
            var keysObserver = testScheduler.CreateObserver<int>();

            using (var cache = new ObservableInMemoryCache<int, string>(notificationScheduler: testScheduler))
            {
                cache.Add(1, "One", testScheduler).Subscribe();
                cache.Add(2, "Two", testScheduler).Subscribe();
                cache.Add(3, "Three", testScheduler).Subscribe();
                testScheduler.AdvanceBy(3);

                // when
                using (cache.Keys.Subscribe(keysObserver))
                {
                    cache.Add(4, "Four").Subscribe();
                    cache.Add(5, "Five").Subscribe();

                    testScheduler.AdvanceBy(2);
                }

                // then
                keysObserver.Messages.Count.Should().Be(5);
                keysObserver.Messages.Last().Should().NotBeNull();
            }
        }

        //    [Fact]
        //    public async Task ShouldContainAllCurrentValues()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            await cache.Add(1, "One");
        //            await cache.Add(2, "Two");
        //            await cache.Add(3, "Three");

        //            // when
        //            var values = cache.CurrentValues;

        //            // then
        //            values.Count.Should().Be(3);

        //            values.Should().Contain("One");
        //            values.Should().Contain("Two");
        //            values.Should().Contain("Three");
        //        }
        //    }

        //    [Fact]
        //    public void ShouldUpdateExpirationCorrectlyForExistingItem()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expiresAtTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing);
        //                });

        //            // when
        //            long tickAtTimeOfUpdate = -1; // this 'remembers' the virtual time the expiration update took place
        //            TimeSpan updatedExpiration = default(TimeSpan);
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.FromTicks(1),
        //                async (scheduler, token) =>
        //                {
        //                    tickAtTimeOfUpdate = scheduler.Now.Ticks;
        //                    await cache.UpdateExpiration(1, TimeSpan.FromTicks(3 * expiresAtTicks));
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expiresAtTicks);

        //            long tickAtTimeOfExpirationCheck = -1; // this 'remembers' the virtual time the expiration check took place
        //            testScheduler.ScheduleAsync(
        //                async (scheduler, token) =>
        //                {
        //                    tickAtTimeOfExpirationCheck = scheduler.Now.Ticks;
        //                    updatedExpiration = await cache.ExpiresIn(1);
        //                });
        //            testScheduler.AdvanceBy(1);

        //            // then
        //            updatedExpiration.Should().Be(TimeSpan.FromTicks((3 * expiresAtTicks) + tickAtTimeOfUpdate - tickAtTimeOfExpirationCheck));
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldExpireInAndAtProvideAccurateFutureNowAndPastExpirationInformation()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expiresAtTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler))
        //        {
        //            await cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing);

        //            // when
        //            var expiresIn = await cache.ExpiresIn(1);
        //            var expiresAt = await cache.ExpiresAt(1);

        //            // then
        //            expiresIn.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks));
        //            expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime.AddTicks(expiresAtTicks));

        //            // and when
        //            testScheduler.AdvanceBy(expiresAtTicks);
        //            expiresIn = await cache.ExpiresIn(1);
        //            expiresAt = await cache.ExpiresAt(1);

        //            // then
        //            expiresIn.ShouldBeEquivalentTo(TimeSpan.Zero);
        //            expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime);

        //            // and finally when
        //            testScheduler.AdvanceBy(expiresAtTicks);
        //            expiresIn = await cache.ExpiresIn(1);
        //            expiresAt = await cache.ExpiresAt(1);

        //            // then
        //            expiresIn.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks * -1));
        //            expiresAt.ShouldBeEquivalentTo(testScheduler.Now.UtcDateTime.Subtract(TimeSpan.FromTicks(expiresAtTicks)));
        //        }
        //    }

        //    [Fact]
        //    public void ShouldExpireAndKeepSingleElementForDoNothingExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);

        //            // then
        //            cache.Count.Should().Be(1);
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowKeyHasExpiredExceptionOnGetAfterKeyHasExpiredWithDoNothingExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var initialTestSchedulerDateTime = testScheduler.Now.DateTime;
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Get(1);
        //            };

        //            // then
        //            action
        //                .ShouldThrow<KeyHasExpiredException<int>>()
        //                .WithMessage($"The key has expired on {initialTestSchedulerDateTime}.");
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowKeyHasExpiredExceptionOnUpdateAfterKeyHasExpiredWithDoNothingExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var initialTestSchedulerDateTime = testScheduler.Now.DateTime;
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Update(1, "ONE");
        //            };

        //            // then
        //            action
        //                .ShouldThrow<KeyHasExpiredException<int>>()
        //                .WithMessage($"The key has expired on {initialTestSchedulerDateTime}.");
        //        }
        //    }

        //    [Fact]
        //    public void ShouldNotThrowKeyHasExpiredExceptionOnGetIfWantedAfterKeyHasExpiredWithDoNothingExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks * 10);
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Get(1, false);
        //            };

        //            // then
        //            action
        //                .ShouldNotThrow<KeyHasExpiredException<int>>();
        //        }
        //    }

        //    [Fact]
        //    public void ShouldExpireAndRemoveSingleElementForRemovalExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: testScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            cache.Count.Should().Be(0);
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldExpireAndUpdateSingleElementWithSingleKeyUpdaterFuncForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        Func<int, string> singleKeyUpdater = (i) => i.ToString();

        //        using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });


        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            var updatedValue = await cache.Get(1);

        //            cache.Count.Should().Be(1);
        //            updatedValue.Should().Be("1");
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldExpireAndUpdateMultipleElementsWithSingleKeyUpdaterFuncForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        Func<int, string> singleKeyUpdater = (i) => i.ToString();

        //        using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                    await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            var updatedValueOne = await cache.Get(1);
        //            var updatedValueTwo = await cache.Get(2);

        //            cache.Count.Should().Be(2);
        //            updatedValueOne.Should().Be("1");
        //            updatedValueTwo.Should().Be("2");
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldExpireAndUpdateSingleElementWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

        //        using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            var updatedValue = await cache.Get(1);

        //            cache.Count.Should().Be(1);
        //            updatedValue.Should().Be("1");
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldExpireAndUpdateMultipleElementsWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

        //        using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                    await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            var updatedValueOne = await cache.Get(1);
        //            var updatedValueTwo = await cache.Get(2);

        //            cache.Count.Should().Be(2);
        //            updatedValueOne.Should().Be("1");
        //            updatedValueTwo.Should().Be("2");
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowAggregateExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnUpdatedValuesForMultipleKeysForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;
        //        var exceptionsObserver = testScheduler.CreateObserver<ObserverException>();

        //        Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i * 10, i => (i * 20).ToString()); };

        //        using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler, throwOnExpirationHandlingExceptions: false))
        //        {
        //            cache.ObserverExceptions.Subscribe(exceptionsObserver);

        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                    await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            exceptionsObserver.Messages.Count.Should().Be(1);
        //            exceptionsObserver.Messages.Last().Should().NotBeNull();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.Should().NotBeNull().And.BeOfType<AggregateException>();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Count.Should().Be(2);
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.First().Should().BeOfType<KeyNotFoundException<int>>();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Last().Should().BeOfType<KeyNotFoundException<int>>();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.First().As<KeyNotFoundException<int>>().Key.Should().Be(1);
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<AggregateException>().InnerExceptions.Last().As<KeyNotFoundException<int>>().Key.Should().Be(2);
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowKeyNotFoundExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnOneUpdatedValueForMultipleKeysForUpdateExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;
        //        var exceptionsObserver = testScheduler.CreateObserver<ObserverException>();

        //        Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.Skip(1).ToDictionary(i => i, i => i.ToString()); };

        //        using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler, throwOnExpirationHandlingExceptions: false))
        //        {
        //            cache.ObserverExceptions.Subscribe(exceptionsObserver);

        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                    await cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            exceptionsObserver.Messages.Count.Should().Be(1);
        //            exceptionsObserver.Messages.Last().Should().NotBeNull();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.Should().NotBeNull().And.BeOfType<KeyNotFoundException<int>>();
        //            exceptionsObserver.Messages.Last().Value.Value.InnerException.As<KeyNotFoundException<int>>().Key.Should().Be(1);
        //        }
        //    }

        //    [Fact]
        //    public void ShouldNotExpireElementWithCorrespondingExpiryTime()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Remove);
        //                    await cache.Add(2, "Two", TimeSpan.FromDays(10), ObservableCacheExpirationType.Remove);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            cache.Count.Should().Be(1);
        //        }
        //    }

        //    [Fact]
        //    public void ShouldExpireElementWithCorrespondingExpiryTimeWhenDue()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.FromTicks(expirationTimeoutInTicks), ObservableCacheExpirationType.Remove);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            cache.Count.Should().Be(1);

        //            // but when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            cache.Count.Should().Be(0);
        //        }
        //    }

        //    [Fact]
        //    public void ShouldExpireAndRemoveMultipleElementsWithDifferentExpiryTimesForRemovalExpiryType()
        //    {
        //        // given
        //        var testScheduler = new TestScheduler();
        //        var expirationTimeoutInTicks = 10;

        //        using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: testScheduler))
        //        {
        //            testScheduler.ScheduleAsync(
        //                TimeSpan.Zero,
        //                async (scheduler, token) =>
        //                {
        //                    await cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove);
        //                    await cache.Add(2, "Two", TimeSpan.FromTicks(2), ObservableCacheExpirationType.Remove);
        //                });

        //            // when
        //            testScheduler.AdvanceBy(expirationTimeoutInTicks);

        //            // then
        //            cache.Count.Should().Be(0);
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldThrowOnAddingOfItemWithExistingKey()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            await cache.Add(1, "One", TimeSpan.MaxValue);

        //            // when
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Add(1, "One", TimeSpan.MaxValue);
        //            };

        //            // then
        //            action.ShouldThrow<ArgumentException>().WithMessage("The key already existed in the dictionary.");
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldThrowOnRemovalOfNonExistingKey()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            await cache.Add(1, "One", TimeSpan.MaxValue);

        //            // when
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Remove(2);
        //            };

        //            // then
        //            action.ShouldThrow<KeyNotFoundException>();
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldThrowOnGetOfNonExistingKey()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            await cache.Add(1, "One", TimeSpan.MaxValue);

        //            // when
        //            Func<Task> action = async () =>
        //            {
        //                await cache.Get(2);
        //            };

        //            // then
        //            action.ShouldThrow<KeyNotFoundException>();
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowOnExpiresInOfNonExistingKey()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            // when
        //            Func<Task> action = async () =>
        //            {
        //                await cache.ExpiresIn(2);
        //            };

        //            // then
        //            action.ShouldThrow<KeyNotFoundException>();
        //        }
        //    }

        //    [Fact]
        //    public void ShouldThrowOnExpiresAtOfNonExistingKey()
        //    {
        //        // given
        //        using (var cache = new ObservableInMemoryCache<int, string>())
        //        {
        //            // when
        //            Func<Task> action = async () =>
        //            {
        //                await cache.ExpiresAt(2);
        //            };

        //            // then
        //            action.ShouldThrow<KeyNotFoundException>();
        //        }
        //    }

        //    [Fact]
        //    public async Task ShouldNotifySubscribersAboutValueChangesWhileItemsAreInCache()
        //    {
        //        // given
        //        var notificationScheduler = new TestScheduler();

        //        int key = 1;
        //        var testInpcImplementationInstance = new MyNotifyPropertyChanged<int, string>(key);

        //        var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();
        //        var valueChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();

        //        using (var cache = new ObservableInMemoryCache<int, MyNotifyPropertyChanged<int, string>>(notificationScheduler: notificationScheduler))
        //        {
        //            cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

        //            IDisposable cacheChangesSubscription = null;
        //            IDisposable valueChangesSubscription = null;

        //            try
        //            {
        //                cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
        //                valueChangesSubscription = cache.ValueChanges.Subscribe(valueChangesObserver);

        //                // when
        //                await cache.Add(key, testInpcImplementationInstance);
        //                notificationScheduler.AdvanceBy(2);

        //                // then
        //                changesObserver.Messages.Count.Should().Be(1);
        //                changesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemAdded);
        //                changesObserver.Messages.First().Value.Value.Key.Should().Be(key);
        //                changesObserver.Messages.First().Value.Value.Value.Should().Be(testInpcImplementationInstance);

        //                valueChangesObserver.Messages.Count.Should().Be(0);

        //                // and when
        //                testInpcImplementationInstance.FirstProperty = Guid.NewGuid().ToString();

        //                notificationScheduler.AdvanceBy(2);

        //                // then
        //                changesObserver.Messages.Count.Should().Be(2);
        //                valueChangesObserver.Messages.Count.Should().Be(1);

        //                changesObserver.Messages.Last().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemValueChanged);
        //                changesObserver.Messages.Last().Value.Value.Key.Should().Be(1);
        //                changesObserver.Messages.Last().Value.Value.Value.Should().Be(testInpcImplementationInstance);
        //                changesObserver.Messages.Last().Value.Value.OldValue.Should().BeNull();
        //                changesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));

        //                valueChangesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemValueChanged);
        //                valueChangesObserver.Messages.First().Value.Value.Key.Should().Be(1);
        //                valueChangesObserver.Messages.First().Value.Value.Value.Should().Be(testInpcImplementationInstance);
        //                valueChangesObserver.Messages.First().Value.Value.OldValue.Should().BeNull();
        //                valueChangesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));
        //            }
        //            finally
        //            {
        //                cacheChangesSubscription?.Dispose();
        //                valueChangesSubscription?.Dispose();
        //            }
        //        }

        //    }

        //    [Fact]
        //    public async Task ShouldNotifySubscribersAboutKeyChangesWhileItemsAreInCache()
        //    {
        //        // given
        //        var notificationScheduler = new TestScheduler();

        //        int value = 1;
        //        var key = new MyNotifyPropertyChanged<int, string>(value);

        //        var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();
        //        var keyChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();

        //        using (var cache = new ObservableInMemoryCache<MyNotifyPropertyChanged<int, string>, int>(notificationScheduler: notificationScheduler))
        //        {
        //            cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

        //            IDisposable cacheChangesSubscription = null;
        //            IDisposable keyChangesSubscription = null;

        //            try
        //            {
        //                cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
        //                keyChangesSubscription = cache.KeyChanges.Subscribe(keyChangesObserver);

        //                // when
        //                await cache.Add(key, value);
        //                notificationScheduler.AdvanceBy(2);

        //                // then
        //                changesObserver.Messages.Count.Should().Be(1);
        //                changesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemAdded);
        //                changesObserver.Messages.First().Value.Value.Key.Should().Be(key);
        //                changesObserver.Messages.First().Value.Value.Value.Should().Be(value);

        //                keyChangesObserver.Messages.Count.Should().Be(0);

        //                // and when
        //                key.FirstProperty = Guid.NewGuid().ToString();

        //                notificationScheduler.AdvanceBy(2);

        //                // then
        //                changesObserver.Messages.Count.Should().Be(2);
        //                keyChangesObserver.Messages.Count.Should().Be(1);

        //                changesObserver.Messages.Last().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemKeyChanged);
        //                changesObserver.Messages.Last().Value.Value.Key.Should().Be(key);
        //                changesObserver.Messages.Last().Value.Value.Value.Should().Be(1);
        //                changesObserver.Messages.Last().Value.Value.OldValue.Should().Be(default(int));
        //                changesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));

        //                keyChangesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemKeyChanged);
        //                keyChangesObserver.Messages.First().Value.Value.Key.Should().Be(key);
        //                keyChangesObserver.Messages.First().Value.Value.Value.Should().Be(1);
        //                keyChangesObserver.Messages.First().Value.Value.OldValue.Should().Be(default(int));
        //                keyChangesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));
        //            }
        //            finally
        //            {
        //                cacheChangesSubscription?.Dispose();
        //                keyChangesSubscription?.Dispose();
        //            }
        //        }
        //    }
    }
}