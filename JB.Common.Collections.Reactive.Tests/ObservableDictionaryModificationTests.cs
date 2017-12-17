// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryModificationTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
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
    public class ObservableDictionaryModificationTests
    {
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
                observableDictionary.Should().Contain(key, value);

                observableDictionary.Keys.Should().Contain(key);
                observableDictionary.Values.Should().Contain(value);
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
        public void AddOrUpdateAllowsUpdateForExistingKeyWithSameValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary.AddOrUpdate(1, "One");

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void AddOrUpdateShouldAllowUpdateWithDefaultValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("1", "One Value")
            };

            using (var observableDictionary = new ObservableDictionary<string, string>(initialKvPs))
            {
                // when
                Action action = () => observableDictionary.AddOrUpdate("1", default(string));

                // then
                action.ShouldNotThrow<ArgumentNullException>();

                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain("1", default(string));
            }
        }

        [Fact]
        public void AddOrUpdateShouldAllowAddWithDefaultValue()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.AddOrUpdate("1", default(string));

                // then
                action.ShouldNotThrow<ArgumentNullException>();

                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain("1", default(string));
            }
        }

        [Fact]
        public void AddOrUpdateThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.AddOrUpdate(null, null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void AddRangeOfKeyValuePairsAddsItems(int amountOfItemsToAdd)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, amountOfItemsToAdd)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary.AddRange(keyValuePairs);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(amountOfItemsToAdd);

                foreach (var keyValuePair in keyValuePairs)
                {
                    observableDictionary.Should().Contain(keyValuePair);
                }
            }
        }

        [Fact]
        public void AddRangeOfKeyValuePairsThrowsOnNonExistingKeys()
        {
            // given
            var keyValuePairs = Enumerable.Range(0, 2)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                Action invalidRemoveRangeForNonExistingKey = ()
                    => observableDictionary.AddRange(
                        new List<KeyValuePair<int, string>>
                        {
                            new KeyValuePair<int, string>(0, "#0"),
                            new KeyValuePair<int, string>(1, "One"),
                            new KeyValuePair<int, string>(2, "Two")
                        });

                // then
                invalidRemoveRangeForNonExistingKey
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("The following key(s) are already in this dictionary and cannot be added to it: 0, 1\r\nParameter name: items");

                observableDictionary.Count.Should().Be(3);

                observableDictionary.Should().Contain(0, "#0");
                observableDictionary.Should().Contain(1, "#1");
                observableDictionary.Should().Contain(2, "Two");
            }
        }

        [Fact]
        public void AddRangeOfKeyValuePairsThrowsOnNullItems()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => observableDictionary.AddRange(null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: items");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void AddShouldNotThrowOnDefaultValue()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.Add("1", default(string));

                // then
                action.ShouldNotThrow<ArgumentNullException>();

                observableDictionary.Count.Should().Be(1);
            }
        }

        [Fact]
        public void AddShouldThrowOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.Add(null, null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

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
        public void KeyIndexerGetGetsValueForExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // then
                observableDictionary[1].Should().Be("One");
            }
        }

        [Fact]
        public void KeyIndexerGetShouldThrowForNonExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            // when
            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                Action action = () => { var value = observableDictionary[2]; };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }

        [Fact]
        public void KeyIndexerGetShouldThrowForNullKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("1", "One")
            };

            // when
            using (var observableDictionary = new ObservableDictionary<string, string>(initialKvPs))
            {
                // when
                Action action = () => { var value = observableDictionary[null]; };

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void KeyIndexerSetShouldThrowForNullKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("1", "One")
            };

            // when
            using (var observableDictionary = new ObservableDictionary<string, string>(initialKvPs))
            {
                // when
                Action action = () => { observableDictionary[null] = "Two"; };

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void KeyIndexerSetAddsNewItem()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                observableDictionary[key] = value;

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");

                observableDictionary.Keys.Should().Contain(1);
                observableDictionary.Values.Should().Contain("One");
            }
        }

        [Fact]
        public void KeyIndexerSetUpdatesValueForExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                observableDictionary[1] = "Two";

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "Two");

                observableDictionary.Keys.Should().Contain(1);
                observableDictionary.Values.Should().Contain("Two");

                observableDictionary.Values.Should().NotContain("One");
            }
        }

        [Fact]
        public void RemoveOfKeyRemovesExistingItem()
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

                observableDictionary.Keys.Should().NotContain(1);
                observableDictionary.Values.Should().NotContain("One");
            }
        }

        [Fact]
        public void RemoveOfKeyShouldNotThrowOnNonExistingItem()
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
                Action invalidRemoveRangeForNonExistingKey = () => observableDictionary.Remove(10);

                // then

                invalidRemoveRangeForNonExistingKey
                    .ShouldNotThrow<ArgumentOutOfRangeException>();

                observableDictionary.Count.Should().Be(2);
            }
        }

        [Fact]
        public void RemoveOfKeyShouldReportBackCorrespondinglyOnNonExistingItems()
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
                var removalResult = observableDictionary.Remove(10);

                // then
                removalResult.Should().Be(false);
                observableDictionary.Count.Should().Be(2);
            }
        }

        [Fact]
        public void RemoveOfKeyThrowsOnNullKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("1", "One"),
                new KeyValuePair<string, string>("2", "Two")
            };
            using (var observableDictionary = new ObservableDictionary<string, string>(initialKvPs))
            {
                // when
                Action action = () => observableDictionary.Remove((string) null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(2);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 5)]
        public void RemoveRangeOfKeysRemovesItems(int initialAmountOfItems, int amountsOfItemsToRemove)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keysToRemove = Enumerable.Range(0, amountsOfItemsToRemove).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                observableDictionary.RemoveRange(keysToRemove);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(initialAmountOfItems - amountsOfItemsToRemove);

                foreach (var removedKey in keysToRemove)
                {
                    observableDictionary.Should().NotContainKey(removedKey);
                }
            }
        }

        [Fact]
        public void RemoveRangeOfKeysThrowsOnNonExistingItems()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action invalidRemoveRangeForNonExistingKey = () => observableDictionary.RemoveRange(new List<int>() {10});

                // then
                invalidRemoveRangeForNonExistingKey
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("The following key(s) are not in this dictionary and cannot be removed from it: 10\r\nParameter name: keys");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void RemoveRangeOfKeysThrowsOnNullKeys()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => observableDictionary.RemoveRange((List<int>) null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: keys");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 5)]
        public void RemoveRangeOfKeyValuePairsRemovesItems(int initialAmountOfItems, int amountsOfItemsToRemove)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keyValuePairsToRemove = Enumerable.Range(0, amountsOfItemsToRemove)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                observableDictionary.RemoveRange(keyValuePairsToRemove);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(initialAmountOfItems - amountsOfItemsToRemove);

                foreach (var removedKeyValuePair in keyValuePairsToRemove)
                {
                    observableDictionary.Should().NotContain(removedKeyValuePair);
                }
            }
        }

        [Fact]
        public void RemoveRangeOfKeyValuePairsThrowsOnExistingItemWhenValueIsDifferent()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>() {new KeyValuePair<int, string>(1, "One")};

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                Action invalidRemoveRangeForNonExistingKey = () => observableDictionary.RemoveRange(new List<KeyValuePair<int, string>>() {new KeyValuePair<int, string>(1, "Two")});

                // then
                invalidRemoveRangeForNonExistingKey
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("The following key/value pair(s) are not in this dictionary and cannot be removed from it: [1, Two]\r\nParameter name: items");

                observableDictionary.Count.Should().Be(1);
            }
        }

        [Fact]
        public void RemoveRangeOfKeyValuePairsThrowsOnNonExistingItems()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action invalidRemoveRangeForNonExistingKey = () => observableDictionary.RemoveRange(new List<KeyValuePair<int, string>>() {new KeyValuePair<int, string>(10, "Ten")});

                // then
                invalidRemoveRangeForNonExistingKey
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("The following key/value pair(s) are not in this dictionary and cannot be removed from it: [10, Ten]\r\nParameter name: items");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void RemoveRangeOfKeyValuePairsThrowsOnNullItems()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => observableDictionary.RemoveRange((List<KeyValuePair<int, string>>) null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: items");

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

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(99, 100)]
        [InlineData(100, 100)]
        public void TryAddRangeAddsNonExistingItemsAndReportsNonAddedBack(int amountOfInitialItems, int amountOfItemsToAdd)
        {
            // given
            var initialKeyValuePairs = Enumerable.Range(0, amountOfInitialItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keyValuePairsToAdd = Enumerable.Range(0, amountOfItemsToAdd)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKeyValuePairs))
            {
                // when
                IDictionary<int, string> itemsThatCouldNotBeAdded;
                var tryAddResult = observableDictionary.TryAddRange(keyValuePairsToAdd, out itemsThatCouldNotBeAdded);

                // then check whether all items have been accounted for
                tryAddResult.Should().Be(false);
                itemsThatCouldNotBeAdded.Should().NotBeNull();
                itemsThatCouldNotBeAdded.Should().NotBeEmpty();

                observableDictionary.Count.Should().Be(amountOfInitialItems + amountOfItemsToAdd - itemsThatCouldNotBeAdded.Count);

                foreach (var keyValuePair in keyValuePairsToAdd.Except(itemsThatCouldNotBeAdded))
                {
                    observableDictionary.Should().Contain(keyValuePair);
                }

                foreach (var keyValuePair in initialKeyValuePairs.Intersect(keyValuePairsToAdd))
                {
                    itemsThatCouldNotBeAdded.Should().Contain(keyValuePair);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void TryAddRangeAddsNonExistingNewItems(int amountOfItemsToAdd)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, amountOfItemsToAdd)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                IDictionary<int, string> nonAddedKeyValuePairs;
                var tryAddResult = observableDictionary.TryAddRange(keyValuePairs, out nonAddedKeyValuePairs);

                // then check whether all items have been accounted for
                tryAddResult.Should().Be(true);

                nonAddedKeyValuePairs.Should().NotBeNull();
                nonAddedKeyValuePairs.Should().BeEmpty();

                observableDictionary.Count.Should().Be(amountOfItemsToAdd);
                foreach (var keyValuePair in keyValuePairs)
                {
                    observableDictionary.Should().Contain(keyValuePair);
                }
            }
        }

        [Fact]
        public void ContainsKeyShouldReturnFalseForNonExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = observableDictionary.ContainsKey(2);

                // then
                result.Should().Be(false);
            }
        }

        [Fact]
        public void ContainsKeyShouldReturnTrueForExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = observableDictionary.ContainsKey(1);

                // then
                result.Should().Be(true);
            }
        }

        [Fact]
        public void ContainsKeyThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action retrieval = () => observableDictionary.ContainsKey((string)null);

                // then
                retrieval
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
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
        public void TryGetThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                string value;
                Action retrieval = () => observableDictionary.TryGetValue((string) null, out value);

                // then
                retrieval
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void TryRemoveOfKeyDoesNotRemoveNonExistingItem()
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
        public void TryRemoveOfKeyShouldNotThrowOnNonExistingItem()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action invalidRemoveRangeForNonExistingKey = () => observableDictionary.TryRemove(10);

                // then

                invalidRemoveRangeForNonExistingKey
                    .ShouldNotThrow<ArgumentOutOfRangeException>();

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void TryRemoveOfKeyThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.TryRemove((string) null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void TryRemoveOfKeyValuePairRemovesExistingItem()
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
        public void TryRemoveOfKeyWithValueRetrievalDoesNotRemoveNonExistingItem()
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
        public void TryRemoveOfKeyWithValueRetrievalRemovesExistingItem()
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
        public void TryRemoveOfKeyWithValueRetrievalThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                string value;
                Action action = () => observableDictionary.TryRemove((string) null, out value);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 10)]
        [InlineData(100, 101)]
        public void TryRemoveRangeOfKeysForCornerCasesRemovesExistingItemsAndReportsNonremovablesBack(int initialAmountOfItems, int amountOfItemsToRemove)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keyValuePairsToRemove = Enumerable.Range(0, amountOfItemsToRemove)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keysForKeyValuePairsToRemove = keyValuePairsToRemove.Select(kvp => kvp.Key).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IList<int> nonRemovedKeys;
                var tryRemoveResult = observableDictionary.TryRemoveRange(keysForKeyValuePairsToRemove, out nonRemovedKeys);

                // then check whether all items have been accounted for
                tryRemoveResult.Should().Be(false);

                nonRemovedKeys.Should().NotBeNull();
                nonRemovedKeys.Should().NotBeEmpty();

                observableDictionary.Count.Should().Be(initialAmountOfItems - amountOfItemsToRemove + nonRemovedKeys.Count);

                // check whether everything that was reported as removable is removed
                foreach (var keyValuePair in keysForKeyValuePairsToRemove.Except(nonRemovedKeys))
                {
                    observableDictionary.Should().NotContainKey(keyValuePair);
                }

                foreach (var keyValuePair in nonRemovedKeys)
                {
                    observableDictionary.Should().NotContainKey(keyValuePair);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(100)]
        public void TryRemoveRangeOfKeysRemovesExistingItems(int initialAmountOfItems)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keysForKeyValuePairs = keyValuePairs.Select(kvp => kvp.Key);

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IList<int> nonRemovables = new List<int>();
                Action action = () => observableDictionary.TryRemoveRange(keysForKeyValuePairs, out nonRemovables);

                // then check whether all items have been accounted for
                action.ShouldNotThrow();
                observableDictionary.Count.Should().Be(0);

                nonRemovables.Should().NotBeNull();
                nonRemovables.Should().BeEmpty();
            }
        }

        [Fact]
        public void TryRemoveRangeOfKeysRemovesExistingItemsAndReportsNonremovablesBack()
        {
            // given
            var keyValuePairs = Enumerable.Range(0, 100)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keysForKeyValuePairs = keyValuePairs.Select(kvp => kvp.Key).ToList();

            var keyValuePairsToRemove = Enumerable.Range(50, 100)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keysForKeyValuePairsToRemove = keyValuePairsToRemove.Select(kvp => kvp.Key).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IList<int> nonRemovedKeys;
                var tryRemoveResult = observableDictionary.TryRemoveRange(keysForKeyValuePairsToRemove, out nonRemovedKeys);

                // then check whether all items have been accounted for
                tryRemoveResult.Should().Be(false);

                nonRemovedKeys.Should().NotBeNull();
                nonRemovedKeys.Should().NotBeEmpty();

                nonRemovedKeys.Count.Should().Be(50);
                observableDictionary.Count.Should().Be(50);

                // check whether everything that was reported as removable is removed
                foreach (var keyValuePair in keysForKeyValuePairsToRemove.Except(nonRemovedKeys))
                {
                    observableDictionary.Should().NotContainKey(keyValuePair);
                }

                foreach (var keyValuePair in nonRemovedKeys)
                {
                    observableDictionary.Should().NotContainKey(keyValuePair);
                }

                // and check whether all other one(s) are still there, too
                foreach (var keyValuePair in keysForKeyValuePairs.Except(keysForKeyValuePairsToRemove))
                {
                    observableDictionary.Should().ContainKey(keyValuePair);
                }
            }
        }

        [Fact]
        public void TryRemoveRangeOfKeysThrowsOnNullKeys()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                IList<int> nonRemovables;
                bool removalResult = false;
                Action action = () => observableDictionary.TryRemoveRange(null, out nonRemovables);

                // then check whether all items have been accounted for
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: keys");
            }
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 10)]
        [InlineData(100, 101)]
        public void TryRemoveRangeOfKeyValuePairsForCornerCasesRemovesExistingItemsAndReportsNonremovablesBack(int initialAmountOfItems, int amountOfItemsToRemove)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keyValuePairsToRemove = Enumerable.Range(0, amountOfItemsToRemove)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IDictionary<int, string> nonRemovedKeyValuePairs;
                var tryRemoveResult = observableDictionary.TryRemoveRange(keyValuePairsToRemove, out nonRemovedKeyValuePairs);

                // then check whether all items have been accounted for
                tryRemoveResult.Should().Be(false);

                nonRemovedKeyValuePairs.Should().NotBeNull();
                nonRemovedKeyValuePairs.Should().NotBeEmpty();

                observableDictionary.Count.Should().Be(initialAmountOfItems - amountOfItemsToRemove + nonRemovedKeyValuePairs.Count);

                // check whether everything that was reported as removable is removed
                foreach (var keyValuePair in keyValuePairsToRemove.Except(nonRemovedKeyValuePairs))
                {
                    observableDictionary.Should().NotContain(keyValuePair);
                }

                foreach (var keyValuePair in nonRemovedKeyValuePairs)
                {
                    observableDictionary.Should().NotContain(keyValuePair);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(100)]
        public void TryRemoveRangeOfKeyValuePairsRemovesExistingItems(int initialAmountOfItems)
        {
            // given
            var keyValuePairs = Enumerable.Range(0, initialAmountOfItems)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IDictionary<int, string> nonRemovables;
                var removalResult = observableDictionary.TryRemoveRange(keyValuePairs, out nonRemovables);

                // then check whether all items have been accounted for
                removalResult.Should().Be(true);

                observableDictionary.Count.Should().Be(0);

                nonRemovables.Should().NotBeNull();
                nonRemovables.Should().BeEmpty();
            }
        }

        [Fact]
        public void TryRemoveRangeOfKeyValuePairsRemovesExistingItemsAndReportsNonremovablesBack()
        {
            // given
            var keyValuePairs = Enumerable.Range(0, 100)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            var keyValuePairsToRemove = Enumerable.Range(50, 100)
                .Select(i => new KeyValuePair<int, string>(i, $"#{i}"))
                .ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(keyValuePairs))
            {
                // when
                IDictionary<int, string> nonRemovedKeyValuePairs;
                var tryRemoveResult = observableDictionary.TryRemoveRange(keyValuePairsToRemove, out nonRemovedKeyValuePairs);

                // then check whether all items have been accounted for
                tryRemoveResult.Should().Be(false);

                nonRemovedKeyValuePairs.Should().NotBeNull();
                nonRemovedKeyValuePairs.Should().NotBeEmpty();

                nonRemovedKeyValuePairs.Count.Should().Be(50);
                observableDictionary.Count.Should().Be(50);

                // check whether everything that was reported as removable is removed
                foreach (var keyValuePair in keyValuePairsToRemove.Except(nonRemovedKeyValuePairs))
                {
                    observableDictionary.Should().NotContain(keyValuePair);
                }

                foreach (var keyValuePair in nonRemovedKeyValuePairs)
                {
                    observableDictionary.Should().NotContain(keyValuePair);
                }

                // and check whether all other one(s) are still there, too
                foreach (var keyValuePair in keyValuePairs.Except(keyValuePairsToRemove))
                {
                    observableDictionary.Should().Contain(keyValuePair);
                }
            }
        }

        [Fact]
        public void TryRemoveRangeOfKeyValuePairsThrowsOnNullKeyValuePairs()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                IDictionary<int, string> nonRemovables;
                bool removalResult = false;
                Action action = () => observableDictionary.TryRemoveRange(null, out nonRemovables);

                // then check whether all items have been accounted for
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: items");
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
        public void TryUpdateThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => observableDictionary.TryUpdate((string) null, "Null");

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
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

                observableDictionary.Keys.Should().Contain(1);

                observableDictionary.Values.Should().NotContain("One");
                observableDictionary.Values.Should().Contain("Two");
            }
        }
    }
}