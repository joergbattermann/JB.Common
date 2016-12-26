using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.ExtensionMethods;
using JB.Reactive.Testing.ExtensionMethods;
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
        public void OverallThroughputAnalyzerShouldCalculateThroughputCorrectly(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();
            
            var analysisResultsObserver = testScheduler.CreateObserver<IThroughputAnalysisResult>();

            using (Observable.Range(start, count, testScheduler).AnalyzeOverallThroughput(scheduler: testScheduler).Subscribe(analysisResultsObserver))
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

        [Fact]
        public void ThroughputAnalyzerShouldCalculateThroughputCorrectly()
        {
            // given
            var testScheduler = new TestScheduler();

            var analysisResultsObserver = testScheduler.CreateObserver<IThroughputAnalysisResult>();

            var interval = TimeSpan.FromMilliseconds(1);
            var resolution = TimeSpan.FromMilliseconds(10);

            using (Observable.Interval(interval, testScheduler).Take(20).AnalyzeThroughput(resolution, scheduler: testScheduler).Subscribe(analysisResultsObserver))
            {
                analysisResultsObserver.Messages.Count.Should().Be(0);

                // when some time has passed
                testScheduler.AdvanceTo(resolution + interval);

                // then
                analysisResultsObserver.Messages.Count.Should().Be(1);
                analysisResultsObserver.Messages[0].Value.Value.ElapsedTime.Should().Be(resolution);
                analysisResultsObserver.Messages[0].Value.Value.ThroughputPerMillisecond.Should().Be(analysisResultsObserver.Messages[0].Value.Value.Count / analysisResultsObserver.Messages[0].Value.Value.ElapsedTime.TotalMilliseconds);

                // when
                testScheduler.AdvanceTo(resolution + resolution + interval);

                // then
                analysisResultsObserver.Messages.Count.Should().Be(3);

                analysisResultsObserver.Messages[1].Value.Value.ElapsedTime.Should().Be(resolution);
                analysisResultsObserver.Messages[1].Value.Value.ThroughputPerMillisecond.Should().Be(analysisResultsObserver.Messages[1].Value.Value.Count / analysisResultsObserver.Messages[1].Value.Value.ElapsedTime.TotalMilliseconds);
                
                analysisResultsObserver.Messages.Last().Value.Kind.Should().Be(NotificationKind.OnCompleted);
            }
        }

        [Fact]
        public void ThroughputAnalyzerShouldCalculateThroughputCorrectlyAndForwardSourceSequence()
        {
            // given
            var testScheduler = new TestScheduler();

            var sourceSequenceObserver = testScheduler.CreateObserver<long>();
            var throughPutAnalysisResultsObserver = testScheduler.CreateObserver<IThroughputAnalysisResult>();

            var interval = TimeSpan.FromMilliseconds(1);
            var resolution = TimeSpan.FromMilliseconds(10);

            using (Observable.Interval(interval, testScheduler).Take(20).AnalyzeThroughputWith(throughPutAnalysisResultsObserver, resolution, testScheduler).Subscribe(sourceSequenceObserver))
            {
                throughPutAnalysisResultsObserver.Messages.Count.Should().Be(0);

                // when some time has passed
                testScheduler.AdvanceTo(resolution + interval);

                // then
                throughPutAnalysisResultsObserver.Messages.Count.Should().Be(1);
                throughPutAnalysisResultsObserver.Messages[0].Value.Value.ElapsedTime.Should().Be(resolution);
                throughPutAnalysisResultsObserver.Messages[0].Value.Value.ThroughputPerMillisecond.Should().Be(throughPutAnalysisResultsObserver.Messages[0].Value.Value.Count / throughPutAnalysisResultsObserver.Messages[0].Value.Value.ElapsedTime.TotalMilliseconds);

                // when
                testScheduler.AdvanceTo(resolution + resolution + interval);

                // then
                throughPutAnalysisResultsObserver.Messages.Count.Should().Be(2);

                throughPutAnalysisResultsObserver.Messages[1].Value.Value.ElapsedTime.Should().Be(resolution);
                throughPutAnalysisResultsObserver.Messages[1].Value.Value.ThroughputPerMillisecond.Should().Be(throughPutAnalysisResultsObserver.Messages[1].Value.Value.Count / throughPutAnalysisResultsObserver.Messages[1].Value.Value.ElapsedTime.TotalMilliseconds);
            }
        }
    }
}