// -----------------------------------------------------------------------
// <copyright file="CountAnalyzerTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
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

namespace JB.Reactive.Analytics.Tests.Analyzers
{
    public class CountAnalyzerTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 90)]
        [InlineData(0, 1000)]
        public void CountAnalyzerShouldCountAllMessages(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();
            var analysisResultsObserver = testScheduler.CreateObserver<ICountBasedAnalysisResult>();

            using (Observable.Range(start, count, testScheduler).AnalyzeCount(scheduler: testScheduler).Subscribe(analysisResultsObserver))
            {
                // when producer ran to completion
                testScheduler.Start();

                // then
                analysisResultsObserver.Messages.Count.Should().Be(count + 1); // total count of messages is count+1 because the last message is an oncompleted one

                analysisResultsObserver.Messages.First().Value.Value.Count.Should().Be(1);
                analysisResultsObserver.Messages[count -1].Value.Value.Count.Should().Be(count);

                analysisResultsObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }
    }
}