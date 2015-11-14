using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
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
            using (var reactiveList = new ReactiveList<int>(initialList, scheduler: testScheduler))
            {
                // then
                reactiveList.Count.Should().Be(initialList.Count);
                reactiveList.ShouldAllBeEquivalentTo(initialList);
            }
        }
        
        [Fact]
        public void ShouldUseProvidedSyncRoot()
        {
            // given
            var syncRoot = new object();

            // when
            using (var reactiveList = new ReactiveList<int>(syncRoot: syncRoot))
            {
                // then
                reactiveList.SyncRoot.Should().BeSameAs(syncRoot);
            }
        }
    }
}