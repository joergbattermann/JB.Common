// -----------------------------------------------------------------------
// <copyright file="ObservableListSingleItemModificationsTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableListSingleItemModificationsTests
    {
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