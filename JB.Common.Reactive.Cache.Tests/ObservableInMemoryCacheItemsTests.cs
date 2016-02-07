using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using System.Reactive.Linq;
using JB.Collections;
using JB.Reactive.Cache.ExtensionMethods;
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
                cache.CurrentCount.Should().Be(2);
            }
        }

        [Fact]
        public async Task ShouldAddRangeOfNewItems()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var keyValuePairs = new Dictionary<int, string>()
                {
                    {1, "One"},
                    {2, "Two"}
                };
                await cache.AddRange(keyValuePairs);

                // then
                cache.CurrentCount.Should().Be(2);
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
                cache.CurrentCount.Should().Be(1);
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
                cache.CurrentCount.Should().Be(1);
            }
        }

        [Fact]
        public async Task ShouldRemoveRangeOfExistingItem()
        {
            // given
            var testScheduler = new TestScheduler();
            var removalObserver = testScheduler.CreateObserver<bool>();

            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One", TimeSpan.MaxValue);
                await cache.Add(2, "Two", TimeSpan.MaxValue);
                await cache.Add(3, "Three", TimeSpan.MaxValue);

                // when
                cache.RemoveRange(new List<int>() { 1, 2 }, testScheduler).Subscribe(removalObserver);
                testScheduler.AdvanceBy(3);

                // then
                cache.CurrentCount.Should().Be(1);
                removalObserver.Messages.Count.Should().Be(3);
                removalObserver.Messages[0].Value.Value.Should().Be(true);
                removalObserver.Messages[1].Value.Value.Should().Be(true);
                removalObserver.Messages[2].Value.Kind.Should().Be(NotificationKind.OnCompleted);
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
                cache.CurrentCount.Should().Be(0);
            }
        }

        [Fact]
        public async Task ShouldClearCacheOnEveryClearRequest()
        {
            // given
            var workerScheduler = new TestScheduler();
            var clearObserver = workerScheduler.CreateObserver<Unit>();

            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");
                await cache.Add(3, "Three");

                // when
                cache.Clear(Observable.Range(0, 2).Select(_ => Unit.Default), workerScheduler).Subscribe(clearObserver);
                workerScheduler.AdvanceBy(3);

                // then
                cache.CurrentCount.Should().Be(0);

                clearObserver.Messages.Count.Should().Be(3);
                clearObserver.Messages[0].Value.Kind.Should().Be(NotificationKind.OnNext);
                clearObserver.Messages[1].Value.Kind.Should().Be(NotificationKind.OnNext);
                clearObserver.Messages[2].Value.Kind.Should().Be(NotificationKind.OnCompleted);
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
        public void KeysShouldProvideExistingAndAddedKeys()
        {
            // given
            var workerScheduler = new TestScheduler();

            var keysObserver = workerScheduler.CreateObserver<int>();

            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                cache.Add(1, "One", workerScheduler).Subscribe();
                cache.Add(2, "Two", workerScheduler).Subscribe();
                cache.Add(3, "Three", workerScheduler).Subscribe();

                workerScheduler.AdvanceBy(3);

                // when
                using (cache.Keys.Subscribe(keysObserver))
                {
                    cache.Add(4, "Four", workerScheduler).Subscribe();
                    cache.Add(5, "Five", workerScheduler).Subscribe();

                    workerScheduler.AdvanceBy(4);
                }

                // then
                keysObserver.Messages.Count.Should().Be(5);
                keysObserver.Messages.Last().Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ShouldContainAllCurrentValues()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");
                await cache.Add(3, "Three");

                // when
                var values = cache.CurrentValues;

                // then
                values.Count.Should().Be(3);

                values.Should().Contain("One");
                values.Should().Contain("Two");
                values.Should().Contain("Three");
            }
        }

        [Fact]
        public void ShouldUpdateExpirationCorrectlyForExistingItem()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var expiresAtTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing).Subscribe();

                // when
                expirationScheduler.Schedule(
                    TimeSpan.FromTicks(5),
                    _ =>
                    { 
                        cache.UpdateExpiry(1, TimeSpan.FromTicks(2 * expiresAtTicks)).Subscribe();
                    });

                // when
                expirationScheduler.AdvanceBy(5);

                var expirationUpdateObserver = expirationScheduler.CreateObserver<TimeSpan>();
                expirationScheduler.Schedule(
                    TimeSpan.FromTicks(5),
                    _ =>
                    {
                        cache.ExpiresIn(1).Subscribe(expirationUpdateObserver);
                    });

                expirationScheduler.AdvanceBy(5);

                // then
                expirationUpdateObserver.Messages.Count.Should().Be(2);
                expirationUpdateObserver.Messages.First().Value.Value.Ticks.Should().Be(15);
            }
        }

        [Fact]
        public void ShouldExpireInAndAtProvideAccurateFutureNowAndPastExpirationInformation()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            var expiresInObserver = workerScheduler.CreateObserver<TimeSpan>();
            var expiresAtObserver = workerScheduler.CreateObserver<DateTime>();

            var expiresAtTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.FromTicks(expiresAtTicks), ObservableCacheExpirationType.DoNothing, workerScheduler).Subscribe();
                expirationScheduler.AdvanceBy(2);
                workerScheduler.AdvanceBy(1);
                
                // when
                cache.ExpiresIn(1, workerScheduler).Subscribe(expiresInObserver);
                workerScheduler.AdvanceBy(3);

                cache.ExpiresAt(1, workerScheduler).Subscribe(expiresAtObserver);
                workerScheduler.AdvanceBy(3);

                // then
                expiresInObserver.Messages.Count.Should().Be(2);
                expiresAtObserver.Messages.Count.Should().Be(2);

                expiresInObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks));
                expiresAtObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(expirationScheduler.Now.UtcDateTime.AddTicks(expiresAtTicks));

                // and when
                expiresInObserver.Messages.Clear();
                expiresAtObserver.Messages.Clear();
                expirationScheduler.AdvanceBy(expiresAtTicks);

                cache.ExpiresIn(1, workerScheduler).Subscribe(expiresInObserver);
                workerScheduler.AdvanceBy(2);

                cache.ExpiresAt(1, workerScheduler).Subscribe(expiresAtObserver);
                workerScheduler.AdvanceBy(2);

                // then
                expiresInObserver.Messages.Count.Should().Be(2);
                expiresAtObserver.Messages.Count.Should().Be(2);

                expiresInObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(TimeSpan.Zero);
                expiresAtObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(expirationScheduler.Now.UtcDateTime);


                // and finally when
                expiresInObserver.Messages.Clear();
                expiresAtObserver.Messages.Clear();
                expirationScheduler.AdvanceBy(expiresAtTicks);

                cache.ExpiresIn(1, workerScheduler).Subscribe(expiresInObserver);
                workerScheduler.AdvanceBy(2);

                cache.ExpiresAt(1, workerScheduler).Subscribe(expiresAtObserver);
                workerScheduler.AdvanceBy(2);

                // then
                expiresInObserver.Messages.Count.Should().Be(2);
                expiresAtObserver.Messages.Count.Should().Be(2);

                expiresInObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(TimeSpan.FromTicks(expiresAtTicks * -1));
                expiresAtObserver.Messages.First().Value.Value.ShouldBeEquivalentTo(expirationScheduler.Now.UtcDateTime.Subtract(TimeSpan.FromTicks(expiresAtTicks)));
            }
        }

        [Fact]
        public void ShouldExpireAndKeepSingleElementForDoNothingExpiryType()
        {
            // given
            var scheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing, scheduler).Subscribe();
                scheduler.AdvanceBy(2);

                // when
                scheduler.AdvanceBy(expirationTimeoutInTicks * 10);

                // then
                cache.CurrentCount.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldThrowKeyHasExpiredExceptionOnGetAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            var initialTestSchedulerDateTime = expirationScheduler.Now.DateTime;
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks * 10);

                // and
                var valueObserver = workerScheduler.CreateObserver<string>();
                cache.Get(1, true, workerScheduler).Subscribe(valueObserver);
                workerScheduler.AdvanceBy(4);

                // then
                valueObserver.Messages.Count.Should().Be(1);
                valueObserver.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                valueObserver.Messages.First().Value.Exception.Should().BeOfType<KeyHasExpiredException<int>>();
                valueObserver.Messages.First().Value.Exception.Message.Should().Be($"The key has expired on {initialTestSchedulerDateTime}.");
            }
        }

        [Fact]
        public void ShouldThrowKeyHasExpiredExceptionOnUpdateAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            var initialTestSchedulerDateTime = expirationScheduler.Now.DateTime;
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks * 10);

                // and
                var observer = workerScheduler.CreateObserver<KeyValuePair<int, string>>();
                cache.Update(1, "ONE", true, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(3);

                // then
                observer.Messages.Count.Should().Be(1);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<KeyHasExpiredException<int>>();
                observer.Messages.First().Value.Exception.Message.Should().Be($"The key has expired on {initialTestSchedulerDateTime}.");
            }
        }

        [Fact]
        public void ShouldNotThrowKeyHasExpiredExceptionOnGetIfWantedAfterKeyHasExpiredWithDoNothingExpiryType()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();
            
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks)))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.DoNothing, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks * 10);

                // and
                var valueObserver = workerScheduler.CreateObserver<string>();
                cache.Get(1, false, workerScheduler).Subscribe(valueObserver);
                workerScheduler.AdvanceBy(4);

                // then
                valueObserver.Messages.Count.Should().Be(2);
                valueObserver.Messages.First().Value.Kind.Should().Be(NotificationKind.OnNext);
                valueObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);

                valueObserver.Messages.First().Value.Value.Should().Be("One");
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
                cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove).Subscribe();

                // when
                testScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.CurrentCount.Should().Be(0);
            }
        }

        [Fact]
        public void ShouldExpireAndUpdateSingleElementWithSingleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<int, string> singleKeyUpdater = (i) => i.ToString();

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update).Subscribe();

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValueGetObserver = workerScheduler.CreateObserver<string>();
                cache.Get(1, true, workerScheduler).Subscribe(updatedValueGetObserver);
                workerScheduler.AdvanceBy(4);

                cache.CurrentCount.Should().Be(1);

                updatedValueGetObserver.Messages.Count.Should().Be(2);
                updatedValueGetObserver.Messages.First().Value.Value.Should().Be("1");
            }
        }

        [Fact]
        public void ShouldExpireAndUpdateMultipleElementsWithSingleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<int, string> singleKeyUpdater = (i) => i.ToString();

            using (var cache = new ObservableInMemoryCache<int, string>(singleKeyRetrievalFunction: singleKeyUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var updatedValueGetObserver = workerScheduler.CreateObserver<string>();
                cache.Get(1, true, workerScheduler).Subscribe(updatedValueGetObserver);
                cache.Get(2, true, workerScheduler).Subscribe(updatedValueGetObserver);
                workerScheduler.AdvanceBy(4);

                cache.CurrentCount.Should().Be(2);

                updatedValueGetObserver.Messages.Count.Should().Be(4);
                updatedValueGetObserver.Messages[0].Value.Value.Should().Be("1");
                updatedValueGetObserver.Messages[2].Value.Value.Should().Be("2");
            }
        }

        [Fact]
        public void ShouldExpireAndUpdateSingleElementWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var getObserver = workerScheduler.CreateObserver<string>();
                cache.Get(1, true, workerScheduler).Subscribe(getObserver);
                workerScheduler.AdvanceBy(4);

                cache.CurrentCount.Should().Be(1);
                getObserver.Messages.Count.Should().Be(2);
                getObserver.Messages.First().Value.Value.Should().Be("1");
            }
        }

        [Fact]
        public void ShouldExpireAndUpdateMultipleElementsWithMultipleKeyUpdaterFuncForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                var getObserver = workerScheduler.CreateObserver<string>();

                cache.Get(1, true, workerScheduler).Subscribe(getObserver);
                cache.Get(2, true, workerScheduler).Subscribe(getObserver);
                workerScheduler.AdvanceBy(4);

                cache.CurrentCount.Should().Be(2);
                getObserver.Messages.Count.Should().Be(4);
                getObserver.Messages[0].Value.Value.Should().Be("1");
                getObserver.Messages[2].Value.Value.Should().Be("2");
            }
        }

        [Fact]
        public void ShouldThrowAggregateExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnUpdatedValuesForMultipleKeysForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            var exceptionsObserver = expirationScheduler.CreateObserver<ObserverException>();

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.ToDictionary(i => i * 10, i => (i * 20).ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler, throwOnExpirationHandlingExceptions: false))
            {
                cache.ObserverExceptions.Subscribe(exceptionsObserver);

                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

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
        public void ShouldThrowKeyNotFoundExceptionWhenMultipleKeyUpdaterFuncDoesNotReturnOneUpdatedValueForMultipleKeysForUpdateExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            var exceptionsObserver = expirationScheduler.CreateObserver<ObserverException>();

            Func<IEnumerable<int>, IEnumerable<KeyValuePair<int, string>>> multipleKeysUpdater = (ints) => { return ints.Skip(1).ToDictionary(i => i, i => i.ToString()); };

            using (var cache = new ObservableInMemoryCache<int, string>(multipleKeysRetrievalFunction: multipleKeysUpdater, expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler, throwOnExpirationHandlingExceptions: false))
            {
                cache.ObserverExceptions.Subscribe(exceptionsObserver);

                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.Zero, ObservableCacheExpirationType.Update, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

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
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.Zero, ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.FromDays(10), ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);
                
                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.CurrentCount.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldExpireElementWithCorrespondingExpiryTimeWhenDue()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.FromTicks(expirationTimeoutInTicks), ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);
                
                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.CurrentCount.Should().Be(1);

                // but when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.CurrentCount.Should().Be(0);
            }
        }

        [Fact]
        public void ShouldExpireAndRemoveMultipleElementsWithDifferentExpiryTimesForRemovalExpiryType()
        {
            // given
            var workerScheduler = new TestScheduler();
            var expirationScheduler = new TestScheduler();
            var expirationTimeoutInTicks = 10;

            using (var cache = new ObservableInMemoryCache<int, string>(expiredElementsHandlingChillPeriod: TimeSpan.FromTicks(expirationTimeoutInTicks), expirationScheduler: expirationScheduler))
            {
                cache.Add(1, "One", TimeSpan.FromTicks(1), ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.FromTicks(2), ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                expirationScheduler.AdvanceBy(expirationTimeoutInTicks);

                // then
                cache.CurrentCount.Should().Be(0);
            }
        }

        [Fact]
        public void ShouldThrowOnAddingOfItemWithExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                cache.Add(1, "One", TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                var observer = workerScheduler.CreateObserver<KeyValuePair<int, string>>();
                cache.Add(1, "ReallyOne", TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(2);

                // then
                observer.Messages.Count.Should().Be(1);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<KeyAlreadyExistsException<int>>();
                observer.Messages.First().Value.Exception.As<KeyAlreadyExistsException<int>>().Key.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldThrowOnAddingOfRangeOfItemsWithExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                cache.Add(1, "One", TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(2);

                // when
                var observer = workerScheduler.CreateObserver<KeyValuePair<int, string>>();

                var keyValuePairs = new Dictionary<int, string>()
                {
                    {1, "One"},
                    {2, "Two"}
                };
                cache.AddRange(keyValuePairs, TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(4);

                // then
                observer.Messages.Count.Should().Be(2);

                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnNext);
                observer.Messages.First().Value.Value.Key.Should().Be(2);
                observer.Messages.First().Value.Value.Value.Should().Be("Two");

                observer.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.Last().Value.Exception.Should().BeOfType<KeyAlreadyExistsException<int>>();
                observer.Messages.Last().Value.Exception.Message.Should().Be("An element with the same key already exists.");
                observer.Messages.Last().Value.Exception.As<KeyAlreadyExistsException<int>>().Key.Should().Be(1);
            }
        }

        [Fact]
        public void ShouldThrowOnAddingOfRangeOfItemsWithExistingKeys()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                cache.Add(1, "One", TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe();
                cache.Add(2, "Two", TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe();
                workerScheduler.AdvanceBy(4);

                // when
                var observer = workerScheduler.CreateObserver<KeyValuePair<int, string>>();

                var keyValuePairs = new Dictionary<int, string>()
                {
                    {1, "One"},
                    {2, "Two"}
                };
                cache.AddRange(keyValuePairs, TimeSpan.MaxValue, scheduler: workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(4);

                // then
                observer.Messages.Count.Should().Be(1);

                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<AggregateException>();
                observer.Messages.First().Value.Exception.As<AggregateException>().InnerExceptions.Count.Should().Be(2);
                observer.Messages.First().Value.Exception.As<AggregateException>().InnerExceptions.First().Should().BeOfType<KeyAlreadyExistsException<int>>();
                observer.Messages.First().Value.Exception.As<AggregateException>().InnerExceptions.First().As<KeyAlreadyExistsException<int>>().Key.Should().Be(1);
                observer.Messages.First().Value.Exception.As<AggregateException>().InnerExceptions.Last().As<KeyAlreadyExistsException<int>>().Key.Should().Be(2);
            }
        }

        [Fact]
        public void ShouldNotifyNonRemovalOfNonExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var observer = workerScheduler.CreateObserver<bool>();
                cache.Remove(1, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(3);

                // then
                observer.Messages.Count.Should().Be(2);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnNext);
                observer.Messages.First().Value.Value.Should().Be(false);

                observer.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }

        [Fact]
        public void ShouldNotifyNonRemovalOfRangeOfNonExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var observer = workerScheduler.CreateObserver<bool>();
                cache.RemoveRange(new List<int>() { 1 }, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(3);

                // then
                observer.Messages.Count.Should().Be(2);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnNext);
                observer.Messages.First().Value.Value.Should().Be(false);

                observer.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }

        [Fact]
        public async Task ShouldNotifyNonRemovalOfRangeOfNonExistingKeys()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                var observer = workerScheduler.CreateObserver<bool>();
                
                cache.RemoveRange(new List<int>() { 1, 2 }, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(3);

                // then
                observer.Messages.Count.Should().Be(3);
                observer.Messages[0].Value.Value.Should().Be(true);
                observer.Messages[1].Value.Value.Should().Be(false);

                observer.Messages[2].Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }

        [Fact]
        public void ShouldThrowOnGetOfNonExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var observer = workerScheduler.CreateObserver<string>();
                cache.Get(1, true, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(4);

                // then
                observer.Messages.Count.Should().Be(1);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<KeyNotFoundException>();
            }
        }

        [Fact]
        public void ShouldThrowOnExpiresInOfNonExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var observer = workerScheduler.CreateObserver<TimeSpan>();
                cache.ExpiresIn(1, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(2);

                // then
                observer.Messages.Count.Should().Be(1);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<KeyNotFoundException>();
            }
        }

        [Fact]
        public void ShouldNotExpireCachedElementsOfDisposedCache()
        {
            // given
            var expirationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            var cache = new ObservableInMemoryCache<int, string>(expirationScheduler: expirationScheduler);
            cache.Add(1, "One", TimeSpan.FromTicks(5), ObservableCacheExpirationType.Remove, workerScheduler).Subscribe();
            workerScheduler.AdvanceBy(2);
            
            // when
            cache.Dispose();
            cache = null;

            Action action = () => { expirationScheduler.AdvanceBy(10); };

            // then
            action.ShouldNotThrow<ObjectDisposedException>();
            

            //using ()
            //{
            //    // when
            //    var observer = expirationScheduler.CreateObserver<DateTime>();
            //    cache.ExpiresAt(1, expirationScheduler).Subscribe(observer);
            //    expirationScheduler.AdvanceBy(2);

            //    // then
            //    observer.Messages.CurrentCount.Should().Be(1);
            //    observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
            //    observer.Messages.First().Value.Exception.Should().BeOfType<KeyNotFoundException>();
            //}
        }

        [Fact]
        public void ShouldThrowOnExpiresAtOfNonExistingKey()
        {
            // given
            var workerScheduler = new TestScheduler();
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                var observer = workerScheduler.CreateObserver<DateTime>();
                cache.ExpiresAt(1, workerScheduler).Subscribe(observer);
                workerScheduler.AdvanceBy(2);

                // then
                observer.Messages.Count.Should().Be(1);
                observer.Messages.First().Value.Kind.Should().Be(NotificationKind.OnError);
                observer.Messages.First().Value.Exception.Should().BeOfType<KeyNotFoundException>();
            }
        }

        [Fact]
        public void ShouldNotifySubscribersAboutValueChangesWhileItemsAreInCache()
        {
            // given
            var notificationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            int key = 1;
            var testInpcImplementationInstance = new MyNotifyPropertyChanged<int, string>(key);

            var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();
            var valueChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();

            using (var cache = new ObservableInMemoryCache<int, MyNotifyPropertyChanged<int, string>>(notificationScheduler: notificationScheduler))
            {
                cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable cacheChangesSubscription = null;
                IDisposable valueChangesSubscription = null;

                try
                {
                    cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
                    valueChangesSubscription = cache.ValueChanges.Subscribe(valueChangesObserver);

                    // when
                    cache.Add(key, testInpcImplementationInstance, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    // then
                    changesObserver.Messages.Count.Should().Be(1);
                    changesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemAdded);
                    changesObserver.Messages.First().Value.Value.Key.Should().Be(key);
                    changesObserver.Messages.First().Value.Value.Value.Should().Be(testInpcImplementationInstance);

                    valueChangesObserver.Messages.Count.Should().Be(0);

                    // and when
                    testInpcImplementationInstance.FirstProperty = Guid.NewGuid().ToString();

                    notificationScheduler.AdvanceBy(1);

                    // then
                    changesObserver.Messages.Count.Should().Be(2);
                    valueChangesObserver.Messages.Count.Should().Be(1);

                    changesObserver.Messages.Last().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemValueChanged);
                    changesObserver.Messages.Last().Value.Value.Key.Should().Be(1);
                    changesObserver.Messages.Last().Value.Value.Value.Should().Be(testInpcImplementationInstance);
                    changesObserver.Messages.Last().Value.Value.OldValue.Should().BeNull();
                    changesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));

                    valueChangesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemValueChanged);
                    valueChangesObserver.Messages.First().Value.Value.Key.Should().Be(1);
                    valueChangesObserver.Messages.First().Value.Value.Value.Should().Be(testInpcImplementationInstance);
                    valueChangesObserver.Messages.First().Value.Value.OldValue.Should().BeNull();
                    valueChangesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));
                }
                finally
                {
                    cacheChangesSubscription?.Dispose();
                    valueChangesSubscription?.Dispose();
                }
            }
        }

        [Fact]
        public void ShouldNotNotifySubscribersAboutValueChangesAfterItemsAreRemovedFromCache()
        {
            // given
            var notificationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            int key = 1;
            var testInpcImplementationInstance = new MyNotifyPropertyChanged<int, string>(key);

            var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();
            var valueChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<int, MyNotifyPropertyChanged<int, string>>>();

            using (var cache = new ObservableInMemoryCache<int, MyNotifyPropertyChanged<int, string>>(notificationScheduler: notificationScheduler))
            {
                cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable cacheChangesSubscription = null;
                IDisposable valueChangesSubscription = null;

                try
                {
                    cache.Add(key, testInpcImplementationInstance, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    // when
                    cache.Remove(key, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
                    valueChangesSubscription = cache.ValueChanges.Subscribe(valueChangesObserver);

                    // .. and
                    testInpcImplementationInstance.FirstProperty = Guid.NewGuid().ToString();
                    notificationScheduler.AdvanceBy(1);

                    // then
                    changesObserver.Messages.Count.Should().Be(0);
                    valueChangesObserver.Messages.Count.Should().Be(0);
                }
                finally
                {
                    cacheChangesSubscription?.Dispose();
                    valueChangesSubscription?.Dispose();
                }
            }
        }

        [Fact]
        public async Task ShouldNotifySubscribersAboutKeyChangesWhileItemsAreInCache()
        {
            // given
            var notificationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            int value = 1;
            var key = new MyNotifyPropertyChanged<int, string>(value);

            var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();
            var keyChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();

            using (var cache = new ObservableInMemoryCache<MyNotifyPropertyChanged<int, string>, int>(notificationScheduler: notificationScheduler))
            {
                cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable cacheChangesSubscription = null;
                IDisposable keyChangesSubscription = null;

                try
                {
                    cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
                    keyChangesSubscription = cache.KeyChanges.Subscribe(keyChangesObserver);

                    // when
                    cache.Add(key, value, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    // then
                    changesObserver.Messages.Count.Should().Be(1);
                    changesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemAdded);
                    changesObserver.Messages.First().Value.Value.Key.Should().Be(key);
                    changesObserver.Messages.First().Value.Value.Value.Should().Be(value);

                    keyChangesObserver.Messages.Count.Should().Be(0);

                    // and when
                    key.FirstProperty = Guid.NewGuid().ToString();

                    notificationScheduler.AdvanceBy(1);

                    // then
                    changesObserver.Messages.Count.Should().Be(2);
                    keyChangesObserver.Messages.Count.Should().Be(1);

                    changesObserver.Messages.Last().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemKeyChanged);
                    changesObserver.Messages.Last().Value.Value.Key.Should().Be(key);
                    changesObserver.Messages.Last().Value.Value.Value.Should().Be(1);
                    changesObserver.Messages.Last().Value.Value.OldValue.Should().Be(default(int));
                    changesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));

                    keyChangesObserver.Messages.First().Value.Value.ChangeType.Should().Be(ObservableCacheChangeType.ItemKeyChanged);
                    keyChangesObserver.Messages.First().Value.Value.Key.Should().Be(key);
                    keyChangesObserver.Messages.First().Value.Value.Value.Should().Be(1);
                    keyChangesObserver.Messages.First().Value.Value.OldValue.Should().Be(default(int));
                    keyChangesObserver.Messages.Last().Value.Value.ChangedPropertyName.Should().Be(nameof(MyNotifyPropertyChanged<int, string>.FirstProperty));
                }
                finally
                {
                    cacheChangesSubscription?.Dispose();
                    keyChangesSubscription?.Dispose();
                }
            }
        }

        [Fact]
        public async Task ShouldNotNotifySubscribersAboutKeyChangesAfterItemsAreRemovedFromCache()
        {
            // given
            var notificationScheduler = new TestScheduler();
            var workerScheduler = new TestScheduler();

            int value = 1;
            var key = new MyNotifyPropertyChanged<int, string>(value);

            var changesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();
            var keyChangesObserver = notificationScheduler.CreateObserver<IObservableCacheChange<MyNotifyPropertyChanged<int, string>, int>>();

            using (var cache = new ObservableInMemoryCache<MyNotifyPropertyChanged<int, string>, int>(notificationScheduler: notificationScheduler))
            {
                cache.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable cacheChangesSubscription = null;
                IDisposable keyChangesSubscription = null;

                try
                {
                    cache.Add(key, value, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    // when
                    cache.Remove(key, workerScheduler).Subscribe();
                    workerScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(2);
                    notificationScheduler.AdvanceBy(5);

                    cacheChangesSubscription = cache.Changes.Subscribe(changesObserver);
                    keyChangesSubscription = cache.KeyChanges.Subscribe(keyChangesObserver);

                    // .. and
                    key.FirstProperty = Guid.NewGuid().ToString();
                    notificationScheduler.AdvanceBy(1);

                    // then
                    changesObserver.Messages.Count.Should().Be(0);
                    keyChangesObserver.Messages.Count.Should().Be(0);
                }
                finally
                {
                    cacheChangesSubscription?.Dispose();
                    keyChangesSubscription?.Dispose();
                }
            }
        }
    }
}