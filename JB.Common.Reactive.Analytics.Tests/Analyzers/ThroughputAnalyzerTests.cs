using System;
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
    public class ThroughputAnalyzerTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 90)]
        [InlineData(0, 1000)]
        public void ThroughputAnalyzerShouldCalculateThroughputCorrectly(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();
            
            var analysisResultsObserver = testScheduler.CreateObserver<IThroughputAnalysisResult>();

            using (Observable.Range(start, count, testScheduler).AnalyzeThroughput(scheduler: testScheduler).Subscribe(analysisResultsObserver))
            {
                // when producer ran to completion
                testScheduler.AdvanceBy(count + 2);

                // then
                analysisResultsObserver.Messages.Count.Should().Be(count + 1); // total count of messages is count+1 because the last message is an oncompleted one

                var lastThroughputMessage = analysisResultsObserver.Messages[count - 1];
                var elapsedTimeForLastThroughputMessage = TimeSpan.FromTicks(lastThroughputMessage.Time);

                lastThroughputMessage.Value.Value.Count.Should().Be(count);
                lastThroughputMessage.Value.Value.ElapsedTime.Should().Be(elapsedTimeForLastThroughputMessage);

                lastThroughputMessage.Value.Value.ThroughputPerMillisecond.Should().Be(lastThroughputMessage.Value.Value.Count / elapsedTimeForLastThroughputMessage.TotalMilliseconds);

                analysisResultsObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }
    }
}