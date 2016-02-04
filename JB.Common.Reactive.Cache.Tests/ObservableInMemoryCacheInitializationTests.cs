// -----------------------------------------------------------------------
// <copyright file="ObservableInMemoryCacheInitializationTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using FluentAssertions;
using Xunit;

namespace JB.Reactive.Cache.Tests
{
    public class ObservableInMemoryCacheInitializationTests
    {
        [Fact]
        public void ShouldBeCorrectlyInitialized()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // then
                cache.CurrentCount.Should().Be(0);
                cache.ThresholdAmountWhenChangesAreNotifiedAsReset.Should().Be(Int32.MaxValue);
            }
        }
    }
}