// -----------------------------------------------------------------------
// <copyright file="ObservableCollectionInitializationTests.cs" company="Joerg Battermann">
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
    public class ObservableCollectionInitializationTests
    {
        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var initialList = new List<int>() {1, 2, 3};

            // when
            using (var observableCollection = new ObservableCollection<int>(initialList))
            {
                // then
                observableCollection.Should().HaveCount(initialList.Count);
                observableCollection.Should().BeEquivalentTo(initialList);
            }
        }

        [Fact]
        public void ShouldUseProvidedSyncRoot()
        {
            // given
            var syncRoot = new object();

            // when
            using (var observableCollection = new ObservableCollection<int>(syncRoot: syncRoot))
            {
                // then
                observableCollection.SyncRoot.Should().BeSameAs(syncRoot);
            }
        }
    }
}