// -----------------------------------------------------------------------
// <copyright file="ObservableListInitializationTests.cs" company="Joerg Battermann">
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
    public class ObservableListInitializationTests
    {
        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var initialList = new List<int>() {1, 2, 3};

            // when
            using (var observableList = new ObservableList<int>(initialList))
            {
                // then
                observableList.Count.Should().Be(initialList.Count);
                observableList.ShouldAllBeEquivalentTo(initialList);
            }
        }

        [Fact]
        public void ShouldBeEmptyOnConstructionWithNoInitialItems()
        {
            // given
            // when
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // then
                observableDictionary.IsEmpty.Should().BeTrue();
            }
        }

        [Fact]
        public void ShouldNotBeEmptyOnConstructionWithInitialItems()
        {
            // given
            var initialItems = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(initialItems))
            {
                // then
                observableDictionary.IsEmpty.Should().BeFalse();
            }
        }

        [Fact]
        public void ShouldBeEmptyAfterClear()
        {
            // given
            var initialItems = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };
            using (var observableDictionary = new ObservableDictionary<int, string>(initialItems))
            {
                // when
                observableDictionary.Clear();

                // then
                observableDictionary.IsEmpty.Should().BeTrue();
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