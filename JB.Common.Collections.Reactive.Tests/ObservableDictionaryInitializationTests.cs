// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryInitializationTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
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
            constructionFunc.Should().NotThrow<ArgumentException>();
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
                observableDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset = value;

                // then
                observableDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset.Should().Be(value);
            }
        }

        [Fact]
        public void ShouldThrowForThresholdAmountWhenItemChangesAreNotifiedAsResetValueLessThanZero()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => observableDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset = -1;

                // then
                action
                    .Should().Throw<ArgumentOutOfRangeException>()
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
                observableDictionary.Should().BeEquivalentTo(initialList);
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
            constructionFunc.Should().Throw<ArgumentException>();
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
            constructionFunc.Should().Throw<ArgumentException>();
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