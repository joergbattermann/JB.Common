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
        public void AddOfCollectionOfKeyValuePairsIsReadOnlyShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).IsReadOnly.Should().Be(false);
            }
        }

        [Fact]
        public void IsSynchronizedOfCollectionShouldBeFalse()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                var isSynchronized = ((ICollection)observableDictionary).IsSynchronized;

                // then
                isSynchronized.Should().Be(false);
            }
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
        public void ShouldThrowDisposedExceptionWhenAccessingCollectionPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action syncRootPropertyAccess = () => { var syncRoot = ((ICollection)observableDictionary).SyncRoot; };
            Action countPropertyAccess = () => { var count = ((ICollection)observableDictionary).Count; };
            Action isSynchronizedPropertyAccess = () => { var isSynchronized = ((ICollection)observableDictionary).IsSynchronized; };

            // then
            syncRootPropertyAccess.ShouldThrow<ObjectDisposedException>();
            countPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isSynchronizedPropertyAccess.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingCollectionOfKeyValuePairsPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var isReadOnly = ((ICollection<KeyValuePair<int, string>>)observableDictionary).IsReadOnly; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void AddOfCollectionOfKeyValuePairsAddsNonExistingItem()
        {
            // given
            using (var observableDictionary = new ObservableDictionary<int, string>())
            {
                // when
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).Add(new KeyValuePair<int, string>(1, "One"));

                // then
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
            Action action = () => { ((ICollection<KeyValuePair<int, string>>)observableDictionary).Add(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();

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
                Action action = () => ((ICollection<KeyValuePair<int, string>>)observableDictionary).Add(new KeyValuePair<int, string>(1, "Two"));

                // then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("The key already existed in the dictionary.");
                
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
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
                ((ICollection)observableDictionary).CopyTo(targetArray, 0);
            };

            // then
            action.ShouldThrow<ObjectDisposedException>();

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
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).CopyTo(targetArray, 0);
            };

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
                var enumerator = ((IEnumerable)observableDictionary).GetEnumerator();

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
            Action action = () =>
            {
                var enumerator = ((IEnumerable)observableDictionary).GetEnumerator();
            };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void KeysOfReadOnlyDictionaryOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () =>
            {
                var keys = ((IReadOnlyDictionary<int, string>)observableDictionary).Keys;
            };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ValuesOfReadOnlyDictionaryOfKeyValuePairsShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () =>
            {
                var values = ((IReadOnlyDictionary<int, string>)observableDictionary).Values;
            };

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
                var keys = ((IReadOnlyDictionary<int, string>)observableDictionary).Keys;

                // then
                keys.ShouldAllBeEquivalentTo(initialKeys);
            }
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
                var values = ((IReadOnlyDictionary<int, string>)observableDictionary).Values;

                // then
                values.ShouldAllBeEquivalentTo(initialValues);
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
                var enumerator = ((IEnumerable<KeyValuePair<int, string>>)observableDictionary).GetEnumerator();

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
            Action action = () =>
            {
                var enumerator = ((IEnumerable<KeyValuePair<int, string>>)observableDictionary).GetEnumerator();
            };

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
                ((ICollection)observableDictionary).CopyTo(targetArray, 0);

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
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).CopyTo(targetArray, 0);

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
                var result = ((ICollection<KeyValuePair<int, string>>)observableDictionary).Contains(new KeyValuePair<int, string>(1, "One"));

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
            Action action = () => { ((ICollection<KeyValuePair<int, string>>)observableDictionary).Contains(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();

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
                var result = ((ICollection<KeyValuePair<int, string>>)observableDictionary).Contains(new KeyValuePair<int, string>(2, "Two"));

                // then
                result.Should().Be(false);
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
                var result = ((ICollection<KeyValuePair<int, string>>)observableDictionary).Contains(new KeyValuePair<int, string>(1, "Two"));

                // then
                result.Should().Be(false);
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
            Action action = () => { ((ICollection<KeyValuePair<int, string>>)observableDictionary).Remove(new KeyValuePair<int, string>(1, "One")); };

            // then
            action.ShouldThrow<ObjectDisposedException>();

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
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).Remove(new KeyValuePair<int, string>(1, "One"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(0);
                observableDictionary.Should().NotContain(1, "One");
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
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).Remove(new KeyValuePair<int, string>(2, "Two"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
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
                ((ICollection<KeyValuePair<int, string>>)observableDictionary).Remove(new KeyValuePair<int, string>(1, "Two"));

                // then check whether all items have been accounted for
                observableDictionary.Count.Should().Be(1);
                observableDictionary.Should().Contain(1, "One");
            }
        }
    }
}