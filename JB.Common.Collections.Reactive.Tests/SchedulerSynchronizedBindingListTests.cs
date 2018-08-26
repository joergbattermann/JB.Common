// -----------------------------------------------------------------------
// <copyright file="SchedulerSynchronizedBindingListTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class SchedulerSynchronizedBindingListTests
    {
        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var initialList = new List<int>() { 1, 2, 3 };

            // when
            var schedulerSynchronizedBindingList = new SchedulerSynchronizedBindingList<int>(initialList);

            // then
            schedulerSynchronizedBindingList.Should().HaveCount(initialList.Count);
            schedulerSynchronizedBindingList.Should().BeEquivalentTo(initialList);
        }
    }
}