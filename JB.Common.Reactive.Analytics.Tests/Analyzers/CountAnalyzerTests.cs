// -----------------------------------------------------------------------
// <copyright file="CountAnalyzerTests.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.ExtensionMethods;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Common.Reactive.Analytics.Tests.Analyzers
{
    public class CountAnalyzerTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 90)]
        [InlineData(0, 1000)]
        public void BufferWhileShouldReleaseBufferOnCompleted(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();

            var sourceSequenceObserver = testScheduler.CreateObserver<int>();
            var analysisResultsObserver = testScheduler.CreateObserver<ICountBasedAnalysisResult>();

            var comparisonList = Observable.Range(start, count).ToEnumerable().ToList();
            var observable = Observable.Range(start, count).AnalyzeCount(analysisResultsObserver, scheduler: testScheduler);

            using (observable.Subscribe(sourceSequenceObserver))
            {
                // when producer ran to completion
                testScheduler.AdvanceBy(count * 3);

                // then
                sourceSequenceObserver.Messages.Count.Should().Be(count + 1);
                sourceSequenceObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
                
                analysisResultsObserver.Messages.Count.Should().Be(sourceSequenceObserver.Messages.Count);
                analysisResultsObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }
    }
}