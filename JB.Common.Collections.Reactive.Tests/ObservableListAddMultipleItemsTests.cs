// -----------------------------------------------------------------------
// <copyright file="ObservableListAddMultipleItemsTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableListAddMultipleItemsTests
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

            using (var observableList = new ObservableList<int>())
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = rangeToAdd.Count + 1;
                observableList.CountChanges.Subscribe(testObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { observableList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testObserver.Messages.Count.Should().Be(rangeToAdd.Count);
                observableList.Count.Should().Be(rangeToAdd.Count);
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
            var testCollectionChangesObserver = testScheduler.CreateObserver<IObservableCollectionChange<int>>();
            var testListChangesObserver = testScheduler.CreateObserver<IObservableListChange<int>>();

            using (var observableList = new ObservableList<int>())
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = rangeToAdd.Count + 1;
                observableList.CollectionChanges.Subscribe(testCollectionChangesObserver);
                observableList.ListChanges.Subscribe(testListChangesObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { observableList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testCollectionChangesObserver.Messages.Count.Should().Be(rangeToAdd.Count);
                testCollectionChangesObserver.Messages.Select(message => message.Value.Value.Item).ShouldAllBeEquivalentTo(rangeToAdd);

                testListChangesObserver.Messages.Count.Should().Be(rangeToAdd.Count);
                testListChangesObserver.Messages.Select(message => message.Value.Value.Item).ShouldAllBeEquivalentTo(rangeToAdd);

                observableList.Count.Should().Be(rangeToAdd.Count);
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
            var testObserverCollectionChanges = testScheduler.CreateObserver<IObservableCollectionChange<int>>();
            var testObserverResets = testScheduler.CreateObserver<Unit>();

            using (var observableList = new ObservableList<int>())
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = 0;

                observableList.CollectionChanges.Subscribe(testObserverCollectionChanges);
                observableList.Resets.Subscribe(testObserverResets);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { observableList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                var shouldBeReset = rangeToAdd.Count >= observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset;
                testObserverCollectionChanges.Messages.Count.Should().Be(shouldBeReset ? 1 : rangeToAdd.Count);
                testObserverCollectionChanges.Messages.Should()
                    .Match(recordedMessages =>
                        recordedMessages.All(message => message.Value.Value.ChangeType == (shouldBeReset ? ObservableCollectionChangeType.Reset : ObservableCollectionChangeType.ItemAdded)));

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

            using (var observableList = new ObservableList<int>())
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = 0;
                observableList.CountChanges.Subscribe(testObserver);

                testScheduler.Schedule(TimeSpan.FromTicks(100), () => { observableList.AddRange(rangeToAdd); });
                testScheduler.Start();

                // then
                testObserver.Messages.Count.Should().Be(1);
                testObserver.Messages.Last().Should().NotBeNull();
                testObserver.Messages.Last().Value.Value.Should().Be(observableList.Count);
            }
        }
    }
}