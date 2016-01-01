using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryModificationTests
    {
        [Fact]
        public void ClearClearsDictionary()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary.Clear();

                // then 
                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void ResetDoesNotModifyDictionary()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary.Reset();

                // then 
                observableDictionary.Count.Should().Be(2);
                observableDictionary.Should().Contain(1, "One");
                observableDictionary.Should().Contain(2, "Two");
            }
        }

        [Fact]
        public void AddOrUpdateAddsNewItem()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.AddOrUpdate(key, value);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void AddOrUpdateUpdatesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary.AddOrUpdate(1, "Two");

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "Two");
            }
        }

        [Fact]
        public void TryAddAddsNonExistingNewItem()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                var tryAddResult = observableDictionary.TryAdd(key, value);

                // then check whether all items have been accounted for
                tryAddResult.Should().Be(true);
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void TryAddDoesNotAddExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var tryAddResult = observableDictionary.TryAdd(1, "Two");

                // then check whether all items have been accounted for
                tryAddResult.Should().Be(false);
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void TryUpdateUpdatesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var updateResult = observableDictionary.TryUpdate(1, "Two");

                // then
                updateResult.Should().Be(true);
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "Two");
            }
        }

        [Fact]
        public void TryUpdateDoesNotUpdateNonExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var updateResult = observableDictionary.TryUpdate(2, "One");

                // then
                updateResult.Should().Be(false);
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void AddAddsItem()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.Add(key, value);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void RemoveRemovesItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary.Remove(1);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");
            }
        }

        [Fact]
        public void TryRemoveRemovesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var removalResult = observableDictionary.TryRemove(1);

                // then check whether all items have been accounted for
                removalResult.Should().Be(true);
                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");
            }
        }

        [Fact]
        public void TryRemoveDoesNotRemoveNonExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var removalResult = observableDictionary.TryRemove(2);

                // then check whether all items have been accounted for
                removalResult.Should().Be(false);
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void TryRemoveWithValueRetrievalRemovesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                string value;
                var removalResult = observableDictionary.TryRemove(1, out value);

                // then check whether all items have been accounted for
                removalResult.Should().Be(true);
                value.Should().Be("One");

                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");
            }
        }

        [Fact]
        public void TTryRemoveWithValueRetrievalDoesNotRemoveNonExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                string value;
                var removalResult = observableDictionary.TryRemove(2, out value);

                // then check whether all items have been accounted for
                removalResult.Should().Be(false);
                value.Should().Be(default(string));

                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void TryGetRetrievesExistingValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                string retrievedValue;
                var tryGetResult = observableDictionary.TryGetValue(1, out retrievedValue);

                // then check whether all items have been accounted for
                tryGetResult.Should().Be(true);

                retrievedValue.Should().Be("One");
            }
        }

        [Fact]
        public void TryGetDoesNotRetrieveNonExistingValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                string retrievedValue;
                var tryGetResult = observableDictionary.TryGetValue(2, out retrievedValue);

                // then check whether all items have been accounted for
                tryGetResult.Should().Be(false);

                retrievedValue.Should().Be(default(string));
            }
        }
    }
}