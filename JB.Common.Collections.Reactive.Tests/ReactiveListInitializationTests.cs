using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Tests
{
    public class ReactiveListInitializationTests
    {
        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var testScheduler = new TestScheduler();
            var initialList = new List<int>() { 1, 2, 3 };

            // when
            var reactiveList = new ReactiveList<int>(initialList, scheduler: testScheduler);

            // then
            reactiveList.Count.Should().Be(initialList.Count);
            reactiveList.ShouldAllBeEquivalentTo(initialList);
        }
        
        [Fact]
        public void ShouldUseProvidedScheduler()
        {
            // given
            var testScheduler = new TestScheduler();

            // when
            var reactiveList = new ReactiveList<int>(scheduler: testScheduler);

            // then
            reactiveList.Scheduler.Should().BeSameAs(testScheduler);
        }

        [Fact]
        public void ShouldUseProvidedSyncRoot()
        {
            // given
            var testScheduler = new TestScheduler();
            var syncRoot = new object();

            // when
            var reactiveList = new ReactiveList<int>(syncRoot: syncRoot);

            // then
            reactiveList.SyncRoot.Should().BeSameAs(syncRoot);
        }
    }
}