using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace JB.Reactive.Cache.Tests
{
    public class ObservableInMemoryCacheItemsTests
    {
        [Fact]
        public async Task ShouldAllowAddingOfNewItems()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                // when
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // then
                cache.Count.Should().Be(2);
            }
        }

        [Fact]
        public async Task ShouldContainAddedItem()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                var containsAddedKey = await cache.Contains(1);
                var containsNonAddedKey = await cache.Contains(2);

                // then
                containsAddedKey.Should().BeTrue();
                containsNonAddedKey.Should().BeFalse();
            }
        }
        
        [Fact]
        public async Task ContainsAllShouldReturnCorrespondingly()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");

                // when
                var containsAllAddedKey = await cache.ContainsAll(new [] {1,2});
                var doesNotContainOneOftheAddedKeysAndOneUnAddedOne = await cache.ContainsAll(new [] {2,3});
                var resultForEmptyContainsAllKeys = await cache.ContainsAll(new List<int>());

                // then
                containsAllAddedKey.Should().BeTrue();
                doesNotContainOneOftheAddedKeysAndOneUnAddedOne.Should().BeFalse();
                resultForEmptyContainsAllKeys.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldAllowClearingOfCache()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");
                await cache.Add(2, "Two");
                await cache.Add(3, "Three");

                // when
                await cache.Clear();

                // then
                cache.Count.Should().Be(0);
            }
        }

        [Fact]
        public async Task ShouldNotAllowAddingOfMultipleItemsWithSameKey()
        {
            // given
            using (var cache = new ObservableInMemoryCache<int, string>())
            {
                await cache.Add(1, "One");

                // when
                Func<Task> action = async () =>
                {
                    await cache.Add(1, "One");
                };


                // then
                action.ShouldThrow<ArgumentException>().WithMessage("The key already existed in the dictionary.");
            }
        }
    }
}