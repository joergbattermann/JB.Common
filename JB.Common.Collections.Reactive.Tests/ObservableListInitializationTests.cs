using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableListInitializationTests
    {
        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var testScheduler = new TestScheduler();
            var initialList = new List<int>() { 1, 2, 3 };

            // when
            using (var observableList = new ObservableList<int>(initialList, scheduler: testScheduler))
            {
                // then
                observableList.Count.Should().Be(initialList.Count);
                observableList.ShouldAllBeEquivalentTo(initialList);
            }
        }
        
        [Fact]
        public void ShouldUseProvidedSyncRoot()
        {
            // given
            var syncRoot = new object();

            // when
            using (var observableList = new ObservableList<int>(syncRoot: syncRoot))
            {
                // then
                observableList.SyncRoot.Should().BeSameAs(syncRoot);
            }
        }
    }
}