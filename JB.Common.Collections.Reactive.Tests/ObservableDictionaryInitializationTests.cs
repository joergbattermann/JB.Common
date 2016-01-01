using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryInitializationTests
    {
        [Fact]
        public void ShouldBeEmptyByDefault()
        {
            // when
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // then
                observableDictionary.IsEmpty.Should().Be(true); 
                observableDictionary.Count.Should().Be(0);
                observableDictionary.IsThrowingUnhandledObserverExceptions.Should().Be(true);
            }
        }

        [Fact]
        public void ShouldContainAllInitiallyProvidedElements()
        {
            // given
            var initialList = new List<int>() { 1, 2, 3 }.ToDictionary(value => value, value => $"#{value}");

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
    }
}