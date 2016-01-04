// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryInitializationTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryInitializationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void KeysShouldContainAllInitialKeys(int itemsInDictionary)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, itemsInDictionary)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // then
                observableDictionary.Keys.Should().Contain(keyValuePairs.Select(kvp => kvp.Key));
            }
        }

        [Fact]
        public void ShouldAllowDisablingOfIsThrowingUnhandledObserverExceptions()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;

                // then
                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(false);
            }
        }

        [Fact]
        public void ShouldAllowMultipleConsecutiveDisablingsOfIsThrowingUnhandledObserverExceptions()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;

                // then
                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(false);
            }
        }

        [Fact]
        public void ShouldAllowMultipleConsecutiveReEnablingsOfIsThrowingUnhandledObserverExceptions()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;
                observableDictionary.IsThrowingUnhandledObserverExceptions = true;
                observableDictionary.IsThrowingUnhandledObserverExceptions = true;
                observableDictionary.IsThrowingUnhandledObserverExceptions = true;

                // then
                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(true);
            }
        }

        [Fact]
        public void ShouldAllowReEnablingOfIsThrowingUnhandledObserverExceptions()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.IsThrowingUnhandledObserverExceptions = false;
                observableDictionary.IsThrowingUnhandledObserverExceptions = true;

                // then
                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(true);
            }
        }

        [Fact]
        public void ShouldAllowSimiliarKeysOnConstructionWithCustomComparerThatDifferentiatesKeys()
        {
            // given
            var initialList = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("A", "Some Value"),
                new KeyValuePair<string, string>("B", "Some Other Value"),
                new KeyValuePair<string, string>("a", "Some Value - again"),
            };

            // when
            ObservableDictionary<string, string> createdDictionary = null;
            Action constructionFunc = () => createdDictionary = new ObservableDictionary<string, string>(initialList, StringComparer.InvariantCulture);

            // then
            constructionFunc.ShouldNotThrow<ArgumentException>();
            createdDictionary.Should().NotBeNull();

            createdDictionary.Count.Should().Be(initialList.Count);
            createdDictionary.Should().ContainKeys(initialList.Select(keyValuePair => keyValuePair.Key), "Should contain all keys");
        }

        [Fact]
        public void ShouldBeEmptyAndTrackingEverythingByDefault()
        {
            // when
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // then
                observableDictionary.Count.Should().Be(0);

                observableDictionary.Keys.Should().BeEmpty();
                observableDictionary.Values.Should().BeEmpty();

                observableDictionary.IsEmpty.Should().Be(true);

                observableDictionary.IsTrackingChanges.Should().Be(true);
                observableDictionary.IsTrackingCountChanges.Should().Be(true);
                observableDictionary.IsTrackingItemChanges.Should().Be(true);
                observableDictionary.IsTrackingResets.Should().Be(true);

                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(true);

                observableDictionary.IsDisposing.Should().Be(false);
                observableDictionary.IsDisposed.Should().Be(false);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void ShouldAllowZeroOrHigherValuesForThresholdAmountWhenItemChangesAreNotifiedAsReset(int value)
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = value;

                // then
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset.Should().Be(value);
            }
        }

        [Fact]
        public void ShouldThrowForThresholdAmountWhenItemChangesAreNotifiedAsResetValueLessThanZero()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = -1;

                // then
                action
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("Must be 0 or higher.\r\nParameter name: value");
            }
        }

        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var initialList = new List<int>() {1, 2, 3}.ToDictionary(value => value, value => $"#{value}");

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(initialList))
            {
                // then
                observableDictionary.IsEmpty.Should().Be(false);
                observableDictionary.Count.Should().Be(initialList.Count);
                observableDictionary.ShouldAllBeEquivalentTo(initialList);
            }
        }

        [Fact]
        public void ShouldPreventDuplicateKeysOnConstructionWithCustomComparer()
        {
            // given
            var initialList = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("A", "Some Value"),
                new KeyValuePair<string, string>("B", "Some Other Value"),
                new KeyValuePair<string, string>("a", "Some Value - again"),
            };

            // when
            Action constructionFunc = () => new ObservableDictionary<string, string>(initialList, StringComparer.InvariantCultureIgnoreCase);

            // then
            constructionFunc.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ShouldPreventDuplicateKeysOnConstructionWithDefaultComparer()
        {
            // given
            var initialList = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "Some Value"),
                new KeyValuePair<int, string>(2, "Some Other Value"),
                new KeyValuePair<int, string>(1, "Some Value - again"),
            };

            // when
            Action constructionFunc = () => new ObservableDictionary<int, string>(initialList);

            // then
            constructionFunc.ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ValuesShouldContainAllInitialValues(int itemsInDictionary)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, itemsInDictionary)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // then
                observableDictionary.Values.Should().Contain(keyValuePairs.Select(kvp => kvp.Value));
            }
        }
    }
}