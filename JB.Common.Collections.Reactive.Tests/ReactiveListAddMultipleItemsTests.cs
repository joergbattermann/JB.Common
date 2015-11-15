﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ReactiveListAddMultipleItemsTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeIncreasesCountOneByOneTest(int lowerLimit, int upperLimit)
        {
            // given
            var rangeToAdd = Enumerable.Range(lowerLimit, upperLimit - lowerLimit + 1).ToList();
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<int>();

            using (var reactiveList = new ReactiveList<int>())
            {
                // when
                reactiveList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = rangeToAdd.Count + 1;
                reactiveList.CountChanges.Subscribe(testObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testObserver.Messages.Count.Should().Be(rangeToAdd.Count);
                reactiveList.Count.Should().Be(rangeToAdd.Count);
            }
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeNotifiesAboutItemsInOrderTest(int lowerLimit, int upperLimit)
        {
            // given
            var rangeToAdd = Enumerable.Range(lowerLimit, upperLimit - lowerLimit + 1).ToList();
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<IReactiveCollectionChange<int>>();

            using (var reactiveList = new ReactiveList<int>())
            {
                // when
                reactiveList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = rangeToAdd.Count + 1;
                reactiveList.CollectionChanges.Subscribe(testObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testObserver.Messages.Count.Should().Be(rangeToAdd.Count);
                testObserver.Messages.Select(message => message.Value.Value.Item).ToList().ShouldAllBeEquivalentTo(rangeToAdd);
                reactiveList.Count.Should().Be(rangeToAdd.Count);
            }
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeNotifiesAsResetInsteadOfIndividualItemsWhenItemCountAboveThresholdTest(int lowerLimit, int upperLimit)
        {
            // given
            var rangeToAdd = Enumerable.Range(lowerLimit, upperLimit - lowerLimit + 1).ToList();
            var testScheduler = new TestScheduler();
            var testObserverCollectionChanges = testScheduler.CreateObserver<IReactiveCollectionChange<int>>();
            var testObserverResets = testScheduler.CreateObserver<Unit>();

            using (var reactiveList = new ReactiveList<int>())
            {
                // when
                reactiveList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = 0;

                reactiveList.CollectionChanges.Subscribe(testObserverCollectionChanges);
                reactiveList.Resets.Subscribe(testObserverResets);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                var shouldBeReset = rangeToAdd.Count >= reactiveList.ThresholdAmountWhenItemChangesAreNotifiedAsReset;
                testObserverCollectionChanges.Messages.Count.Should().Be(shouldBeReset ? 1 : rangeToAdd.Count);
                testObserverCollectionChanges.Messages.Should()
                    .Match(recordedMessages =>
                        recordedMessages.All(message => message.Value.Value.ChangeType == (shouldBeReset ? ReactiveCollectionChangeType.Reset : ReactiveCollectionChangeType.ItemAdded)));

                testObserverResets.Messages.Count.Should().Be(shouldBeReset ? 1 : 0);
            } 
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeNotifiesCountAfterResetWhenItemCountAboveThresholdTest(int lowerLimit, int upperLimit)
        {
            // given
            var rangeToAdd = Enumerable.Range(lowerLimit, upperLimit - lowerLimit + 1).ToList();
            var testScheduler = new TestScheduler();
            var testObserver = testScheduler.CreateObserver<int>();

            using (var reactiveList = new ReactiveList<int>())
            {
                // when
                reactiveList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = 0;
                reactiveList.CountChanges.Subscribe(testObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testObserver.Messages.Count.Should().Be(1);
                testObserver.Messages.Last().Should().NotBeNull();
                testObserver.Messages.Last().Value.Value.Should().Be(reactiveList.Count);
            }
        }
    }
}