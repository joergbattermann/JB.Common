using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryResetAndClearTests
    {
        [Fact]
        public void ClearEmptiesDictionaryAndNotifiesAsReset()
        {
            // given
            var initialList = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, "Some Value"),
                new KeyValuePair<int, string>(2, "Some Other Value"),
                new KeyValuePair<int, string>(3, "Some Totally Different Value"),
            };

            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<IObservableDictionaryChange<int, string>>();
            var resetsObserver = scheduler.CreateObserver<Unit>();

            using (var observableDictionary = new ObservableDictionary<int, string>(initialList, scheduler: scheduler))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable dictionaryChangesSubscription = null;
                IDisposable resetsSubscription = null;

                try
                {
                    dictionaryChangesSubscription = observableDictionary.DictionaryChanges.Subscribe(observer);
                    resetsSubscription = observableDictionary.Resets.Subscribe(resetsObserver);

                    observableDictionary.Clear();
                    scheduler.AdvanceBy(2);

                    // then
                    observableDictionary.Count.Should().Be(0);

                    resetsObserver.Messages.Count.Should().Be(1);
                    observer.Messages.Count.Should().Be(1);

                    observer.Messages.First().Value.Value.ChangeType.Should().Be(ObservableDictionaryChangeType.Reset);
                    observer.Messages.First().Value.Value.Key.Should().Be(default(int));
                    observer.Messages.First().Value.Value.Value.Should().Be(default(string));
                    observer.Messages.First().Value.Value.ReplacedValue.Should().Be(default(string));
                    observer.Messages.First().Value.Value.ChangedPropertyName.Should().BeEmpty();
                }
                finally
                {
                    dictionaryChangesSubscription?.Dispose();
                    resetsSubscription?.Dispose();
                }
            }
        }

        [Fact]
        public void ResetNotifiesResetTest()
        {
            // given
            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<IObservableDictionaryChange<int, string>>();
            var resetsObserver = scheduler.CreateObserver<Unit>();

            using (var observableDictionary = new ObservableDictionary<int, string>(scheduler: scheduler))
            {
                // when
                observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = int.MaxValue;

                IDisposable dictionaryChangesSubscription = null;
                IDisposable resetsSubscription = null;

                try
                {
                    dictionaryChangesSubscription = observableDictionary.DictionaryChanges.Subscribe(observer);
                    resetsSubscription = observableDictionary.Resets.Subscribe(resetsObserver);

                    observableDictionary.Reset();
                    scheduler.AdvanceBy(2);

                    // then
                    resetsObserver.Messages.Count.Should().Be(1);
                    observer.Messages.Count.Should().Be(1);

                    observer.Messages.First().Value.Value.ChangeType.Should().Be(ObservableDictionaryChangeType.Reset);
                    observer.Messages.First().Value.Value.Key.Should().Be(default(int));
                    observer.Messages.First().Value.Value.Value.Should().Be(default(string));
                    observer.Messages.First().Value.Value.ReplacedValue.Should().Be(default(string));
                    observer.Messages.First().Value.Value.ChangedPropertyName.Should().BeEmpty();
                }
                finally
                {
                    dictionaryChangesSubscription?.Dispose();
                    resetsSubscription?.Dispose();
                }
            }
        }
    }
}