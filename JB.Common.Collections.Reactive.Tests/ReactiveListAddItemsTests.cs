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
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Tests
{
    public class ReactiveListAddItemsTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var testScheduler = new TestScheduler();

            var observableReportedCounts = new List<int>();
            var rangeToAdd = Enumerable.Range(lowerLimit, upperLimit - lowerLimit + 1).ToList();

            var reactiveList = new ReactiveList<int>(itemChangesToResetThreshold: 1D, scheduler: testScheduler);
            reactiveList.MinimumItemsChangedToBeConsideredReset = rangeToAdd.Count + 1;

            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCounts.Add(i);
            });

            // when
            reactiveList.AddRange(rangeToAdd);
            testScheduler.Start();

            // then
            observableReportedCounts.ShouldAllBeEquivalentTo(rangeToAdd);
        }

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
            var reactiveList = new ReactiveList<int>(itemChangesToResetThreshold: 1D, scheduler: testScheduler);
            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCount = i;
                countChangesCalled++;
            });

            // when
            for (int i = lowerLimit; i <= upperLimit; i++)
            {
                testScheduler.Start();
                reactiveList.Add(i);
                testScheduler.Stop();
            }

            // then check whether all items have been accounted for
            observableReportedCount.Should().Be((upperLimit == lowerLimit) ? upperLimit : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
            observableReportedCount.Should().Be(reactiveList.Count);
            countChangesCalled.Should().Be(reactiveList.Count);
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

            var reactiveList = new ReactiveList<int>(initialList, itemChangesToResetThreshold: 1D, scheduler: testScheduler);
            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCount = i;
                countChangesCalled++;
            });

            // when
            for (int i = lowerLimit; i <= upperLimit; i++)
            {
                testScheduler.Start();
                reactiveList.Add(i);
                testScheduler.Stop();
            }
            
            // then check whether all items have been accounted for
            var expectedCountChangesCalls = ((upperLimit == lowerLimit) ? upperLimit : (upperLimit - lowerLimit + 1));
            var expectedCount = expectedCountChangesCalls + initialList.Count;
            observableReportedCount.Should().Be(expectedCount); // +1 because the upper for loop goes up to & inclusive the upperLimit
            observableReportedCount.Should().Be(reactiveList.Count);
            countChangesCalled.Should().Be(expectedCountChangesCalls);
        }
    }
}