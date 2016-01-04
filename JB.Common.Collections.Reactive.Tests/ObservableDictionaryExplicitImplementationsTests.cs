// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryExplicitImplementationsTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryExplicitImplementationsTests
    {
        [Fact]
        public void AddOfCollectionOfKeyValuePairsAddsNonExistingItem()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).Add(new KeyValuePair<int, string>(1, "One"));

                // then
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void AddOfCollectionOfKeyValuePairsDoesNotAddItemForExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                Action action = () => ((ICollection<KeyValuePair<int, string>>) observableDictionary).Add(new KeyValuePair<int, string>(1, "Two"));

                // then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("The key already existed in the dictionary.");

                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void AddOfCollectionOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { ((ICollection<KeyValuePair<int, string>>) observableDictionary).Add(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ClearOfDictionaryClearsDictionary()
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
                ((IDictionary) observableDictionary).Clear();

                // then 
                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void ClearOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { ((IDictionary) observableDictionary).Clear(); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ContainsKeyOfReadOnlyDictionaryOfKeyValuePairsShouldReturnFalseForNonExistingValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((IReadOnlyDictionary<int, string>) observableDictionary).ContainsKey(2);

                // then
                result.Should().Be(false);
            }
        }

        [Fact]
        public void ContainsKeyOfReadOnlyDictionaryOfKeyValuePairsShouldReturnTrueForExistingValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((IReadOnlyDictionary<int, string>) observableDictionary).ContainsKey(1);

                // then
                result.Should().Be(true);
            }
        }

        [Fact]
        public void ContainsKeyOfReadOnlyDictionaryOfKeyValuePairsThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action retrieval = () => ((IReadOnlyDictionary<string, string>) observableDictionary).ContainsKey((string) null);

                // then
                retrieval
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void ContainsOfCollectionOfKeyValuePairsDoesNotFindExistingWithDifferentValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((ICollection<KeyValuePair<int, string>>) observableDictionary).Contains(new KeyValuePair<int, string>(1, "Two"));

                // then
                result.Should().Be(false);
            }
        }

        [Fact]
        public void ContainsOfCollectionOfKeyValuePairsDoesNotFindNonExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((ICollection<KeyValuePair<int, string>>) observableDictionary).Contains(new KeyValuePair<int, string>(2, "Two"));

                // then
                result.Should().Be(false);
            }
        }

        [Fact]
        public void ContainsOfCollectionOfKeyValuePairsFindsExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((ICollection<KeyValuePair<int, string>>) observableDictionary).Contains(new KeyValuePair<int, string>(1, "One"));

                // then
                result.Should().Be(true);
            }
        }

        [Fact]
        public void ContainsOfCollectionOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            var observableDictionary = new ObservableDictionary<int, string>(initialKvPs);
            observableDictionary.Dispose();

            // when
            Action action = () => { ((ICollection<KeyValuePair<int, string>>) observableDictionary).Contains(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void CopyToOfCollectionCopiesItems()
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
                var targetArray = new KeyValuePair<int, string>[observableDictionary.Count];
                ((ICollection) observableDictionary).CopyTo(targetArray, 0);

                // then
                targetArray.Should().NotBeEmpty();
                targetArray.Length.Should().Be(observableDictionary.Count);

                foreach (var keyValuePair in initialKvPs)
                {
                    targetArray.Should().Contain(keyValuePair);
                }
            }
        }

        [Fact]
        public void CopyToOfCollectionOfKeyValuePairsCopiesItems()
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
                var targetArray = new KeyValuePair<int, string>[observableDictionary.Count];
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).CopyTo(targetArray, 0);

                // then
                targetArray.Should().NotBeEmpty();
                targetArray.Length.Should().Be(observableDictionary.Count);

                foreach (var keyValuePair in initialKvPs)
                {
                    targetArray.Should().Contain(keyValuePair);
                }
            }
        }


        [Fact]
        public void KeyIndexerGetOfDictionaryGetsValueForExistingKey()
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
                ((IDictionary)observableDictionary)[1].Should().Be("One");
            }
        }

        [Fact]
        public void KeyIndexerGetOfDictionaryShouldThrowForNonExistingKey()
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
                Action action = () => { var value = ((IDictionary)observableDictionary)[2]; };

                // then
                action.ShouldThrow<KeyNotFoundException>();
            }
        }

        [Fact]
        public void KeyIndexerGetOfDictionaryShouldReturnNullForKeyOfIncorrectType()
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
                object value = ((IDictionary)observableDictionary)[1];

                // then
                value.Should().BeNull();
            }
        }

        [Fact]
        public void KeyIndexerGetOfDictionaryShouldThrowForNullKey()
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
                Action action = () => { var value = ((IDictionary)observableDictionary)[null]; };

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void KeyIndexerSetOfDictionaryShouldThrowForKeyOfIncorrectType()
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
                Action action = () => { ((IDictionary)observableDictionary)["1"] = "Two"; };

                // then
                action
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage($"Must be an instance of {typeof(int).Name}\r\nParameter name: key");
            }
        }

        [Fact]
        public void KeyIndexerSetOfDictionaryShouldThrowForValueOfIncorrectType()
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
                Action action = () => { ((IDictionary)observableDictionary)[1] = 2; };

                // then
                action
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage($"Must be an instance of {typeof(string).Name}\r\nParameter name: value");
            }
        }

        [Fact]
        public void KeyIndexerSetOfDictionaryShouldThrowForNullKey()
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
                Action action = () => { ((IDictionary)observableDictionary)[null] = "Two"; };

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void KeyIndexerSetOfDictionaryAddsNewItemOfCorrectType()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                ((IDictionary)observableDictionary)[key] = (object)value;

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");

                observableDictionary.Keys.Should().Contain(1);
                observableDictionary.Values.Should().Contain("One");
            }
        }

        [Fact]
        public void KeyIndexerSetOfDictionaryUpdatesValueOfCorrectTypeForExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                ((IDictionary)observableDictionary)[1] = "Two";

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "Two");

                observableDictionary.Keys.Should().Contain(1);
                observableDictionary.Values.Should().Contain("Two");

                observableDictionary.Values.Should().NotContain("One");
            }
        }

        [Fact]
        public void CopyToOfCollectionOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };
            var observableDictionary = new ObservableDictionary<int, string>(initialKvPs);
            observableDictionary.Dispose();

            // when
            Action action = () =>
            {
                var targetArray = new KeyValuePair<int, string>[observableDictionary.Count];
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).CopyTo(targetArray, 0);
            };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void CopyToOfCollectionShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };
            var observableDictionary = new ObservableDictionary<int, string>(initialKvPs);
            observableDictionary.Dispose();

            // when
            Action action = () =>
            {
                var targetArray = new KeyValuePair<int, string>[observableDictionary.Count];
                ((ICollection) observableDictionary).CopyTo(targetArray, 0);
            };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void GetEnumeratorOfDictionaryShouldGetDictionaryEnumerator()
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
                var enumerator = ((IDictionary) observableDictionary).GetEnumerator();

                // then
                enumerator.Should().NotBeNull();
                enumerator.Should().BeAssignableTo<IDictionaryEnumerator>();

                enumerator.MoveNext().Should().BeTrue();
                enumerator.MoveNext().Should().BeTrue();
                enumerator.MoveNext().Should().BeFalse();
            }
        }

        [Fact]
        public void GetEnumeratorOfEnumerableOfKeyValuePairsShouldGetEnumerator()
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
                var enumerator = ((IEnumerable<KeyValuePair<int, string>>) observableDictionary).GetEnumerator();

                // then
                enumerator.Should().NotBeNull();

                enumerator.MoveNext().Should().BeTrue();
                enumerator.Current.Should().BeOfType<KeyValuePair<int, string>>();
                enumerator.MoveNext().Should().BeTrue();
                enumerator.Current.Should().BeOfType<KeyValuePair<int, string>>();
                enumerator.MoveNext().Should().BeFalse();
            }
        }

        [Fact]
        public void GetEnumeratorOfEnumerableOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var enumerator = ((IEnumerable<KeyValuePair<int, string>>) observableDictionary).GetEnumerator(); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void GetEnumeratorOfEnumerableShouldGetEnumerator()
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
                var enumerator = ((IEnumerable) observableDictionary).GetEnumerator();

                // then
                enumerator.Should().NotBeNull();

                enumerator.MoveNext().Should().BeTrue();
                enumerator.Current.Should().BeOfType<KeyValuePair<int, string>>();
                enumerator.MoveNext().Should().BeTrue();
                enumerator.Current.Should().BeOfType<KeyValuePair<int, string>>();
                enumerator.MoveNext().Should().BeFalse();
            }
        }

        [Fact]
        public void GetEnumeratorOfEnumerableShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var enumerator = ((IEnumerable) observableDictionary).GetEnumerator(); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void IsFixedSizeOfDictionaryShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                ((IDictionary) observableDictionary).IsFixedSize.Should().Be(false);
            }
        }

        [Fact]
        public void IsReadOnlyOfCollectionOfKeyValuePairsShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).IsReadOnly.Should().Be(false);
            }
        }

        [Fact]
        public void IsReadOnlyOfDictionaryShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                ((IDictionary) observableDictionary).IsReadOnly.Should().Be(false);
            }
        }

        [Fact]
        public void IsSynchronizedOfCollectionShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                var isSynchronized = ((ICollection) observableDictionary).IsSynchronized;

                // then
                isSynchronized.Should().Be(false);
            }
        }

        [Fact]
        public void KeyIndexerValueGetOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var value = ((IDictionary) observableDictionary)[1]; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void KeyIndexerValueSetOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { ((IDictionary) observableDictionary)[1] = "One"; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void KeysOfDictionaryShouldBeExpectedKeys()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            var initialKeys = initialKvPs.Select(kvp => kvp.Key).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var keys = ((IDictionary) observableDictionary).Keys;

                // then
                keys.Count.Should().Be(observableDictionary.Count);

                keys.Should().NotBeNullOrEmpty();
                keys.Should().ContainItemsAssignableTo<int>();

                keys.OfType<int>().ShouldAllBeEquivalentTo(initialKeys);
            }
        }

        [Fact]
        public void KeysOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var keys = ((IDictionary) observableDictionary).Keys; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void KeysOfReadOnlyDictionaryOfKeyValuePairsShouldBeExpectedKeys()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            var initialKeys = initialKvPs.Select(kvp => kvp.Key).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var keys = ((IReadOnlyDictionary<int, string>) observableDictionary).Keys;

                // then
                keys.ShouldAllBeEquivalentTo(initialKeys);
            }
        }

        [Fact]
        public void RemoveOfCollectionOfKeyValuePairsDoesNotRemoveExistingItemWithDifferentValue()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).Remove(new KeyValuePair<int, string>(1, "Two"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void RemoveOfCollectionOfKeyValuePairsDoesNotRemoveNonExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).Remove(new KeyValuePair<int, string>(2, "Two"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }

        [Fact]
        public void RemoveOfCollectionOfKeyValuePairsRemovesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                ((ICollection<KeyValuePair<int, string>>) observableDictionary).Remove(new KeyValuePair<int, string>(1, "One"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");
            }
        }

        [Fact]
        public void RemoveOfCollectionOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            var observableDictionary = new ObservableDictionary<int, string>(initialKvPs);
            observableDictionary.Dispose();

            // when
            Action action = () => { ((ICollection<KeyValuePair<int, string>>) observableDictionary).Remove(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingCollectionOfKeyValuePairsPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action isReadOnlyPropertyAccessAction = () => { var isReadOnly = ((ICollection<KeyValuePair<int, string>>) observableDictionary).IsReadOnly; };
            Action countPropertyAccessAction = () => { var count = ((ICollection<KeyValuePair<int, string>>) observableDictionary).Count; };

            // then
            isReadOnlyPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
            countPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingCollectionPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action syncRootPropertyAccess = () => { var syncRoot = ((ICollection) observableDictionary).SyncRoot; };
            Action countPropertyAccess = () => { var count = ((ICollection) observableDictionary).Count; };
            Action isSynchronizedPropertyAccess = () => { var isSynchronized = ((ICollection) observableDictionary).IsSynchronized; };

            // then
            syncRootPropertyAccess.ShouldThrow<ObjectDisposedException>();
            countPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isSynchronizedPropertyAccess.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingDictionaryPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action keysPropertyAccessAction = () => { var keys = ((IDictionary) observableDictionary).Keys; };
            Action valuesPropertyAccessAction = () => { var values = ((IDictionary) observableDictionary).Values; };

            Action isFixedSizePropertyAccessAction = () => { var isFixedSize = ((IDictionary) observableDictionary).IsFixedSize; };
            Action isReadOnlyPropertyAccessAction = () => { var isReadOnly = ((IDictionary) observableDictionary).IsReadOnly; };

            // then
            keysPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
            valuesPropertyAccessAction.ShouldThrow<ObjectDisposedException>();

            isFixedSizePropertyAccessAction.ShouldThrow<ObjectDisposedException>();
            isReadOnlyPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingReadOnlyDictionaryOfKeyValuePairsPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action keysPropertyAccessAction = () => { var keys = ((IReadOnlyDictionary<int, string>) observableDictionary).Keys; };
            Action valuesPropertyAccessAction = () => { var values = ((IReadOnlyDictionary<int, string>) observableDictionary).Values; };

            // then
            keysPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
            valuesPropertyAccessAction.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenContainsKeyOfReadOnlyDictionaryOfKeyValuePairsIsCalledAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<string, string>();
            observableDictionary.Dispose();
            // when
            Action action = () => { var value = ((IReadOnlyDictionary<string, string>) observableDictionary).ContainsKey("One"); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenReadOnlyDictionaryOfKeyValuePairsContainsKeyIsCalledAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();
            // when
            Action action = () => { var value = ((IReadOnlyDictionary<int, string>) observableDictionary).ContainsKey(1); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenReadOnlyDictionaryOfKeyValuePairsTryGetIsCalledAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();
            // when
            string value;
            Action action = () => ((IReadOnlyDictionary<int, string>) observableDictionary).TryGetValue(1, out value);

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenTryGetOfReadOnlyDictionaryOfKeyValuePairsIsCalledAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<string, string>();
            observableDictionary.Dispose();
            // when
            string value;
            Action action = () => ((IReadOnlyDictionary<string, string>) observableDictionary).TryGetValue("One", out value);

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void SyncRootPropertyAccessOfCollectionShouldThrow()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                Action action = () => { var syncRoot = ((ICollection) observableDictionary).SyncRoot; };

                // then
                action
                    .ShouldThrow<NotSupportedException>()
                    .WithMessage("The SyncRoot property may not be used for the synchronization of concurrent collections.");
            }
        }


        [Fact]
        public void TryGetOfReadOnlyDictionaryOfKeyValuePairsDoesNotRetrieveNonExistingValue()
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
                var tryGetResult = ((IReadOnlyDictionary<int, string>) observableDictionary).TryGetValue(2, out retrievedValue);

                // then check whether all items have been accounted for
                tryGetResult.Should().Be(false);

                retrievedValue.Should().Be(default(string));
            }
        }

        [Fact]
        public void TryGetOfReadOnlyDictionaryOfKeyValuePairsRetrievesExistingValue()
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
                var tryGetResult = ((IReadOnlyDictionary<int, string>) observableDictionary).TryGetValue(1, out retrievedValue);

                // then check whether all items have been accounted for
                tryGetResult.Should().Be(true);

                retrievedValue.Should().Be("One");
            }
        }

        [Fact]
        public void TryGetOfReadOnlyDictionaryOfKeyValuePairsThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                string value;
                Action retrieval = () => ((IReadOnlyDictionary<string, string>) observableDictionary).TryGetValue((string) null, out value);

                // then
                retrieval
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void ValuesOfDictionaryShouldBeExpectedValues()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            var initialValues = initialKvPs.Select(kvp => kvp.Value).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var collection = ((IDictionary) observableDictionary).Values;

                // then
                collection.Count.Should().Be(observableDictionary.Count);

                collection.Should().NotBeNullOrEmpty();
                collection.Should().ContainItemsAssignableTo<string>();

                collection.OfType<string>().ShouldAllBeEquivalentTo(initialValues);
            }
        }

        [Fact]
        public void ValuesOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var collection = ((IDictionary) observableDictionary).Values; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ValuesOfReadOnlyDictionaryOfKeyValuePairsShouldBeExpectedValues()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            };

            var initialValues = initialKvPs.Select(kvp => kvp.Value).ToList();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var values = ((IReadOnlyDictionary<int, string>) observableDictionary).Values;

                // then
                values.ShouldAllBeEquivalentTo(initialValues);
            }
        }


        [Fact]
        public void RemoveOfDictionaryRemovesExistingItem()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                ((IDictionary)observableDictionary).Remove(1);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");

                observableDictionary.Keys.Should().NotContain(1);
                observableDictionary.Values.Should().NotContain("One");
            }
        }

        [Fact]
        public void RemoveOfDictionaryShouldNotThrowOnNonExistingItem()
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
                Action invalidRemoveRangeForNonExistingKey = () => ((IDictionary)observableDictionary).Remove(10);

                // then

                invalidRemoveRangeForNonExistingKey
                    .ShouldNotThrow<ArgumentOutOfRangeException>();

                observableDictionary.Count.Should().Be(2);
            }
        }

        [Fact]
        public void RemoveOfDictionaryForIncorrectKeyTypeShouldNotRemoveOrChangeDictionary()
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
                ((IDictionary)observableDictionary).Remove("1");

                // then
                observableDictionary.Count.Should().Be(2);
            }
        }

        [Fact]
        public void RemoveOfDictionaryThrowsOnNullKey()
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
                Action action = () => ((IDictionary)observableDictionary).Remove((string)null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(2);
            }
        }
        
        [Fact]
        public void ContainsOfDictionaryShouldReturnFalseForNonExistingKey()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((IDictionary)observableDictionary).Contains(2);

                // then
                result.Should().Be(false);
            }
        }

        [Fact]
        public void ContainsOfDictionaryShouldReturnTrueForExistingKeyOfCorrectType()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((IDictionary)observableDictionary).Contains(1);

                // then
                result.Should().Be(true);
            }
        }

        [Fact]
        public void ContainsOfDictionaryThrowsOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action retrieval = () => ((IDictionary)observableDictionary).Contains((string)null);

                // then
                retrieval
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");
            }
        }

        [Fact]
        public void ContainsOfDictionaryShouldReturnFalseForKeyOfIncorrectType()
        {
            // given
            var initialKvPs = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "One")
            };

            using (var observableDictionary = new ObservableDictionary<int, string>(initialKvPs))
            {
                // when
                var result = ((IDictionary)observableDictionary).Contains("One");

                // then
                result.Should().BeFalse();
            }
        }

        [Fact]
        public void AddOfDictionaryAddsItem()
        {
            // given
            var key = 1;
            var value = "One";

            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                ((IDictionary)observableDictionary).Add(key, value);

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(key, value);

                observableDictionary.Keys.Should().Contain(key);
                observableDictionary.Values.Should().Contain(value);
            }
        }

        [Fact]
        public void AddOfDictionaryShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { ((IDictionary)observableDictionary).Add(1, "One"); };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void AddOfDictionaryShouldThrowOnNullKey()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => ((IDictionary)observableDictionary).Add(null, null);

                // then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void AddOfDictionaryShouldAllowDefaultValueForValue()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                ((IDictionary) observableDictionary).Add("1", default(string));

                // then
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain("1", default(string));
            }
        }

        [Fact]
        public void AddOfDictionaryShouldThrowOnKeyOfDifferentType()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => ((IDictionary)observableDictionary).Add(1, "One");

                // then
                action
                    .ShouldThrow<ArgumentOutOfRangeException>()
                    .WithMessage("Must be an instance of String\r\nParameter name: key");

                observableDictionary.Count.Should().Be(0);
            }
        }

        [Fact]
        public void AddOfDictionaryShouldThrowOnValueOfDifferentType()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<string, string>())
            {
                // when
                Action action = () => ((IDictionary)observableDictionary).Add("One", 1);

                // then
                action
                    .ShouldThrow<InvalidCastException>()
                    .WithMessage($"Unable to cast object of type '{typeof(int).FullName}' to type '{typeof(string).FullName}'.");

                observableDictionary.Count.Should().Be(0);
            }
        }
    }
}