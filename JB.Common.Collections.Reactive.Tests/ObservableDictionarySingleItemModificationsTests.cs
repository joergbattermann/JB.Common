using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionarySingleItemModificationsTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void SingleItemsAdditionForEmptyDictionaryIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            int observableReportedCount = -1;
            int countChangesCalled = 0;

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
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
                observableReportedCount.Should().Be((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableDictionary.Count);
                countChangesCalled.Should().Be(observableDictionary.Count);
            }
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(10, 100)]
        [InlineData(5, 1000)]
        public void SingleItemsAdditionForNonEmptyDictionaryIncreasesCountTest(int lowerLimit, int upperLimit)
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
        [InlineData(1, 1)]
        [InlineData(100, 10)]
        [InlineData(1, 0)]
        public void SingleItemsRemovalFromDictionaryDecreasesCountTest(int initialDictionarySize, int amountOfItemsToRemove)
        {
            if (amountOfItemsToRemove > initialDictionarySize)
                throw new ArgumentOutOfRangeException(nameof(amountOfItemsToRemove), $"Must be less than {nameof(initialDictionarySize)}");

            // given
            var initialList = Enumerable.Range(0, initialDictionarySize).ToDictionary(item => item, item => $"#{item}");

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