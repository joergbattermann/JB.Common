// -----------------------------------------------------------------------
// <copyright file="ReactiveListAddItemsTests.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableListSingleItemModificationsTests
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

            using (var observableList = new ObservableList<int>())
            {
                // when
                observableList.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = lowerLimit; i <= upperLimit; i++)
                {
                    observableList.Add(i);
                }

                // then check whether all items have been accounted for
                observableReportedCount.Should().Be((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableList.Count);
                countChangesCalled.Should().Be(observableList.Count);
            }
        }
        
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void AddingSingleItemsForNonEmptyListIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var initialList = new List<int>() {1,2,3};
            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableList = new ObservableList<int>(initialList))
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;
                observableList.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });
                
                for (int i = lowerLimit; i <= upperLimit; i++)
                {
                    observableList.Add(i);
                }
                
                // then check whether all items have been accounted for
                var expectedCountChangesCalls = ((upperLimit == lowerLimit) ? 1 : (upperLimit - lowerLimit + 1));
                var expectedCount = expectedCountChangesCalls + initialList.Count;

                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableList.Count);

                countChangesCalled.Should().Be(expectedCountChangesCalls);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(100, 10)]
        [InlineData(1, 0)]
        public void RemoveSingleItemsFromListDecreasesCountTest(int initialListSize, int amountOfItemsToRemove)
        {
            if(amountOfItemsToRemove > initialListSize)
                throw new ArgumentOutOfRangeException(nameof(amountOfItemsToRemove), $"Must be less than {nameof(initialListSize)}");

            // given
            var initialList = Enumerable.Range(0, initialListSize).ToList();

            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableList = new ObservableList<int>(initialList))
            {
                // when
                observableList.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;
                observableList.CountChanges.Subscribe(i =>
                {
                    observableReportedCount = i;
                    countChangesCalled++;
                });

                for (int i = 0; i < amountOfItemsToRemove; i++)
                {
                    observableList.RemoveAt(0);
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