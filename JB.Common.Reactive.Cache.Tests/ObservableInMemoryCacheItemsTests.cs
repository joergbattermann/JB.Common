using System.Reactive.Linq;
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

                // then
                cache.Count.Should().Be(1);
            }
        }
    }
}