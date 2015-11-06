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
        [Fact]
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

        [Fact]
        public void AddCountChangesForEmptyListTest()
        {
            var testScheduler = new TestScheduler();

            int observableReportedCount = -1;
            var reactiveList = new ReactiveList<int>(scheduler: testScheduler);
            reactiveList.CountChanges.Subscribe(i =>
            {
                observableReportedCount = i;
            });

            reactiveList.Add(42);
            testScheduler.AdvanceBy(1);

            observableReportedCount.Should().Be(1);
        }
    }
}