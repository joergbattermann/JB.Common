using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Tests
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
            
            var reactiveList = new ReactiveList<int>(itemChangesToResetThreshold: 1D);
            reactiveList.MinimumItemsChangedToBeConsideredReset = rangeToAdd.Count + 1;
            reactiveList.CountChanges.Subscribe(testObserver);

            // when
            testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
            testScheduler.Start();

            // then
            testObserver.Messages.Count.Should().Be(rangeToAdd.Count);
            reactiveList.Count.Should().Be(rangeToAdd.Count);
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

            var reactiveList = new ReactiveList<int>(itemChangesToResetThreshold: 1D);
            reactiveList.MinimumItemsChangedToBeConsideredReset = rangeToAdd.Count + 1;
            reactiveList.CollectionChanges.Subscribe(testObserver);

            // when
            testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
            testScheduler.Start();

            // then
            testObserver.Messages.Count.Should().Be(rangeToAdd.Count);
            testObserver.Messages.Select(message => message.Value.Value.Item).ToList().ShouldAllBeEquivalentTo(rangeToAdd);
            reactiveList.Count.Should().Be(rangeToAdd.Count);
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
            var testObserver = testScheduler.CreateObserver<IReactiveCollectionChange<int>>();

            var reactiveList = new ReactiveList<int>(itemChangesToResetThreshold: 1D);
            reactiveList.CollectionChanges.Subscribe(testObserver);

            // when
            testScheduler.Schedule(TimeSpan.FromTicks(100), () => { reactiveList.AddRange(rangeToAdd); });
            testScheduler.Start();

            // then
            var shouldBeReset = rangeToAdd.Count >= reactiveList.MinimumItemsChangedToBeConsideredReset;
            testObserver.Messages.Count.Should().Be(shouldBeReset ? 1 : rangeToAdd.Count);
            testObserver.Messages.Should()
                .Match(recordedMessages => 
                    recordedMessages.All(message => message.Value.Value.ChangeType == (shouldBeReset ? ReactiveCollectionChangeType.Reset : ReactiveCollectionChangeType.ItemAdded)));
        }
    }
}