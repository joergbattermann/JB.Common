using System;
using System.Collections.Generic;
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
        public void CollectionOfKeyValuePairsIsReadOnlyShouldThrowDisposedExceptionWhenAccessingPropertiesAfterDisposal()
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
        public void AddOfCollectionOfKeyValuePairsShouldThrowDisposedExceptioAfterDisposal()
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
        public void CopyToOfCollectionOfKeyValuePairsShouldThrowDisposedExceptioAfterDisposal()
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
        public void ContainsOfCollectionOfKeyValuePairsShouldThrowDisposedExceptioAfterDisposal()
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
        public void RemoveOfCollectionOfKeyValuePairsShouldThrowDisposedExceptioAfterDisposal()
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