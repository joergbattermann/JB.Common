using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionarySingleItemModificationsTests
    {
        [Theory]
        [InlineData(10, 10)]
        [InlineData(10, 100)]
        [InlineData(5, 1000)]
        public void AddIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var initialList = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "Some Value"),
                new KeyValuePair<int, string>(2, "Some Other Value"),
                new KeyValuePair<int, string>(3, "Some Totally Different Value"),
            };

            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableDictionary = new ObservableDictionary<int, string>(initialList))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;
                observableDictionary.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = lowerLimit; i <= upperLimit; i++)
                {
                    observableDictionary.Add(i, $"#{i}");
                }

                // then check whether all items have been accounted for
                var expectedCountChangesCalls = ((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1));
                var expectedCount = expectedCountChangesCalls + initialList.Count;

                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableDictionary.Count);

                countChangesCalled.Should().Be(expectedCountChangesCalls);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void AddNotifiesAdditionTest(int amountOfItemsToAdd)
        {
            // given
            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<IObservableDictionaryChange<int, string>>();

            using (var observableDictionary = new ObservableDictionary<int, string>(scheduler: scheduler))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;

                using (observableDictionary.DictionaryChanges.Subscribe(observer))
                {
                    var addedKeyValuePairs = new List<KeyValuePair<int, string>>();
                    for (int i = 0; i < amountOfItemsToAdd; i++)
                    {
                        var keyValuePair = new KeyValuePair<int, string>(i, $"#{i}");

                        observableDictionary.Add(keyValuePair.Key, keyValuePair.Value);
                        addedKeyValuePairs.Add(keyValuePair);

                        scheduler.AdvanceBy(1);
                    }

                    // then
                    observableDictionary.Count.Should().Be(amountOfItemsToAdd);
                    observer.Messages.Count.Should().Be(amountOfItemsToAdd);
                    
                    if (amountOfItemsToAdd > 0)
                    {
                        observer.Messages.Select(message => message.Value.Value.ChangeType).Should().OnlyContain(changeType => changeType == ObservableDictionaryChangeType.ItemAdded);

                        observer.Messages.Select(message => message.Value.Value.Key).Should().Contain(addedKeyValuePairs.Select(kvp => kvp.Key));
                        observer.Messages.Select(message => message.Value.Value.Value).Should().Contain(addedKeyValuePairs.Select(kvp => kvp.Value));
                    }
                }
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 5)]
        [InlineData(100, 99)]
        public void RemoveNotifiesRemovalTest(int initialDictionarySize, int amountOfItemsToRemove)
        {
            if (amountOfItemsToRemove > initialDictionarySize)
                throw new ArgumentOutOfRangeException(nameof(amountOfItemsToRemove), $"Must be less than {nameof(initialDictionarySize)}");

            // given
            var scheduler = new TestScheduler();

            var initialValues = Enumerable.Range(0, initialDictionarySize).ToDictionary(item => item, item => $"#{item}");
            var observer = scheduler.CreateObserver<IObservableDictionaryChange<int, string>>();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialValues, scheduler: scheduler))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;

                using (observableDictionary.DictionaryChanges.Subscribe(observer))
                {
                    var removedKeyValuePairs = new List<KeyValuePair<int, string>>();

                    for (int i = 0; i < amountOfItemsToRemove; i++)
                    {
                        var lastEntry = observableDictionary.Last();
                        observableDictionary.Remove(lastEntry.Key);

                        removedKeyValuePairs.Add(lastEntry);

                        scheduler.AdvanceBy(1);
                    }

                    // then
                    observableDictionary.Count.Should().Be(initialDictionarySize - amountOfItemsToRemove);
                    observer.Messages.Count.Should().Be(amountOfItemsToRemove);

                    if (initialDictionarySize > 0)
                    {
                        observer.Messages.Select(message => message.Value.Value.ChangeType).Should().OnlyContain(changeType => changeType == ObservableDictionaryChangeType.ItemRemoved);

                        observer.Messages.Select(message => message.Value.Value.Key).Should().Contain(removedKeyValuePairs.Select(kvp => kvp.Key));
                        observer.Messages.Select(message => message.Value.Value.Value).Should().Contain(removedKeyValuePairs.Select(kvp => kvp.Value));
                    }
                }
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(100, 10)]
        [InlineData(1, 0)]
        public void RemoveDecreasesCountTest(int initialDictionarySize, int amountOfItemsToRemove)
        {
            if (amountOfItemsToRemove > initialDictionarySize)
                throw new ArgumentOutOfRangeException(nameof(amountOfItemsToRemove), $"Must be less than {nameof(initialDictionarySize)}");

            // given
            var initialValues = Enumerable.Range(0, initialDictionarySize).ToDictionary(item => item, item => $"#{item}");

            int observableReportedCount = initialValues.Count;
            int countChangesCalled = 0;

            using (var observableDictionary = new ObservableDictionary<int, string>(initialValues))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;
                observableDictionary.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = 0; i < amountOfItemsToRemove; i++)
                {
                    observableDictionary.Remove(observableDictionary.Last().Key);
                }

                // then check whether all items have been accounted for
                var expectedCount = initialDictionarySize - amountOfItemsToRemove;

                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableDictionary.Count);

                countChangesCalled.Should().Be(amountOfItemsToRemove);
            }
        }
    }
}