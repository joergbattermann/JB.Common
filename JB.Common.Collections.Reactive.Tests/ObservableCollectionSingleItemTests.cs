// -----------------------------------------------------------------------
// <copyright file="ObservableCollectionSingleItemTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableCollectionSingleItemTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void AddingSingleItemsForEmptyListIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            int observableReportedCount = -1;
            int countChangesCalled = 0;

            using (var observableCollection = new ObservableCollection<int>())
            {
                // when
                observableCollection.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = lowerLimit; i <= upperLimit; i++)
                {
                    observableCollection.Add(i);
                }

                // then check whether all items have been accounted for
                observableReportedCount.Should().Be((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableCollection.Count);
                countChangesCalled.Should().Be(observableCollection.Count);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void AddingSingleItemsForNonEmptyListIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var initialList = new List<int>() {1, 2, 3};
            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableCollection = new ObservableCollection<int>(initialList))
            {
                // when
                observableCollection.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;
                observableCollection.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = lowerLimit; i <= upperLimit; i++)
                {
                    observableCollection.Add(i);
                }

                // then check whether all items have been accounted for
                var expectedCountChangesCalls = ((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1));
                var expectedCount = expectedCountChangesCalls + initialList.Count;

                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableCollection.Count);

                countChangesCalled.Should().Be(expectedCountChangesCalls);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(100, 10)]
        [InlineData(1, 0)]
        public void RemoveSingleItemsFromListDecreasesCountTest(int initialListSize, int amountOfItemsToRemove)
        {
            if (amountOfItemsToRemove > initialListSize)
                throw new ArgumentOutOfRangeException(nameof(amountOfItemsToRemove), $"Must be less than {nameof(initialListSize)}");

            // given
            var initialList = Enumerable.Range(0, initialListSize).ToList();

            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableList = new ObservableCollection<int>(initialList))
            {
                // when
                observableList.ThresholdAmountWhenChangesAreNotifiedAsReset = int.MaxValue;
                observableList.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = 0; i < amountOfItemsToRemove; i++)
                {
                    observableList.Remove(observableList.Last());
                }

                // then check whether all items have been accounted for
                var expectedCount = initialListSize - amountOfItemsToRemove;

                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableList.Count);

                countChangesCalled.Should().Be(amountOfItemsToRemove);
            }
        }
    }
}