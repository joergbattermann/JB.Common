// -----------------------------------------------------------------------
// <copyright file="ReactiveListTests.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Tests
{
    public class ReactiveListTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(99, 999)]
        [InlineData(42, 42)]
        public void AddRangeCountChangesTest(int lowerLimit, int upperLimit)
        {
            var testScheduler = new TestScheduler();

            var observableReportedCounts = new List<int>();
            var reactiveList = new ReactiveList<int>(scheduler: testScheduler);
            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCounts.Add(i);
            });

            var rangeAdded = Enumerable.Range(lowerLimit, upperLimit).ToList();

            reactiveList.AddRange(rangeAdded);
            testScheduler.AdvanceBy(rangeAdded.Count);

            rangeAdded.ShouldAllBeEquivalentTo(observableReportedCounts);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void AddingIndividualItemsForEmptyListIncreasesCountTest(int lowerLimit, int upperLimit)
        {
            // given
            var testScheduler = new TestScheduler();
            int observableReportedCount = -1;
            var reactiveList = new ReactiveList<int>(scheduler: testScheduler);
            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCount = i;
            });


            // when
            for (int i = lowerLimit; i <= upperLimit; i++)
            {
                reactiveList.Add(i);
            }

            testScheduler.Start();

            // then check whether all items have been accounted for
            observableReportedCount.Should().Be((upperLimit == lowerLimit) ? upperLimit : (upperLimit - lowerLimit + 1)); // +1 because the upper for loop goes up to & inclusive the upperLimit
            reactiveList.Count.Should().Be(observableReportedCount);
        }
    }
}