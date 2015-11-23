// -----------------------------------------------------------------------
// <copyright file="ReactiveListAddItemsTests.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableListAddSingleItemsTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void AddingSingleItemsForEmptyListIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var testScheduler = new TestScheduler();
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
                    testScheduler.Start();
                    observableList.Add(i);
                    testScheduler.Stop();
                }

                // then check whether all items have been accounted for
                observableReportedCount.Should().Be((upperLimit == lowerLimit) ? upperLimit : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
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
            var testScheduler = new TestScheduler();
            var initialList = new List<int>() {1,2,3};
            int observableReportedCount = initialList.Count;
            int countChangesCalled = 0;

            using (var observableList = new ObservableList<int>(initialList, scheduler: testScheduler))
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
                    testScheduler.Start();
                    observableList.Add(i);
                    testScheduler.Stop();
                }

                // then check whether all items have been accounted for
                var expectedCountChangesCalls = ((upperLimit == lowerLimit) ? upperLimit : (upperLimit - lowerLimit + 1));
                var expectedCount = expectedCountChangesCalls + initialList.Count;
                observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
                observableReportedCount.Should().Be(observableList.Count);
                countChangesCalled.Should().Be(expectedCountChangesCalls);
            }
        }
    }
}