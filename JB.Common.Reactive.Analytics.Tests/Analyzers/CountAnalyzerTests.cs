// -----------------------------------------------------------------------
// <copyright file="CountAnalyzerTests.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

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
            var analysisResultsObserver = testScheduler.CreateObserver<ICountBasedAnalysisResult>();

            using (Observable.Range(start, count, testScheduler).AnalyzeCount(scheduler: testScheduler).Subscribe(analysisResultsObserver))
            {
                // when producer ran to completion
                testScheduler.Start();

                // then
                analysisResultsObserver.Messages.Count.Should().Be(count + 1); // +1 because the last message is an oncompleted one
                analysisResultsObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }
    }
}