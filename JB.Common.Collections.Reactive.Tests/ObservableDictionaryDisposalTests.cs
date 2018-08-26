// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryDisposalTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using FluentAssertions;
using Xunit;

namespace JB.Collections.Reactive.Tests
{
    public class ObservableDictionaryDisposalTests
    {
        [Fact]
        public void ShouldAllowMultipleDisposals()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action disposalAction = () => observableDictionary.Dispose();

            disposalAction.Should().NotThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldIndicateDisposalAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // then
            observableDictionary.IsDisposing.Should().Be(false);
            observableDictionary.IsDisposed.Should().Be(true);
        }

        [Fact]
        public void AccessingInfrastructureRelevantPropertiesAfterDisposalShouldNotThrowDisposedException()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action isDisposingPropertyAccess = () => { var isDisposing = observableDictionary.IsDisposing; };
            Action isDisposedPropertyAccess = () => { var isDisposed = observableDictionary.IsDisposed; };

            // then
            isDisposingPropertyAccess.Should().NotThrow<ObjectDisposedException>();
            isDisposedPropertyAccess.Should().NotThrow<ObjectDisposedException>();
        }

        [Fact]
        public void AccessingPropertiesAfterDisposalShouldThrowDisposedException()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action countPropertyAccess = () => { var count = observableDictionary.Count; };

            Action isTrackingChangesPropertyAccess = () => { var isTrackingChanges = observableDictionary.IsTrackingChanges; };
            Action isTrackingCountChangesPropertyAccess = () => { var isTrackingCountChanges = observableDictionary.IsTrackingCountChanges; };
            Action isTrackingItemChangesPropertyAccess = () => { var isTrackingItemChanges = observableDictionary.IsTrackingItemChanges; };
            Action isTrackingResetsPropertyAccess = () => { var isTrackingResets = observableDictionary.IsTrackingResets; };

            Action isEmptyPropertyAccess = () => { var isEmpty = observableDictionary.IsEmpty; };

            Action keysPropertyAccess = () => { var keys = observableDictionary.Keys; };
            Action valuesPropertyAccess = () => { var values = observableDictionary.Values; };

            Action thresholdAmountWhenItemChangesAreNotifiedAsResetPropertyGetAccess = () => { var thresholdAmountWhenItemChangesAreNotifiedAsReset = observableDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset; };
            Action thresholdAmountWhenItemChangesAreNotifiedAsResetPropertySetAccess = () => { observableDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset = 1; };

            // then
            countPropertyAccess.Should().Throw<ObjectDisposedException>();

            isTrackingChangesPropertyAccess.Should().Throw<ObjectDisposedException>();
            isTrackingCountChangesPropertyAccess.Should().Throw<ObjectDisposedException>();
            isTrackingItemChangesPropertyAccess.Should().Throw<ObjectDisposedException>();
            isTrackingResetsPropertyAccess.Should().Throw<ObjectDisposedException>();

            thresholdAmountWhenItemChangesAreNotifiedAsResetPropertyGetAccess.Should().Throw<ObjectDisposedException>();
            thresholdAmountWhenItemChangesAreNotifiedAsResetPropertySetAccess.Should().Throw<ObjectDisposedException>();

            isEmptyPropertyAccess.Should().Throw<ObjectDisposedException>();

            keysPropertyAccess.Should().Throw<ObjectDisposedException>();
            valuesPropertyAccess.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void KeyIndexerValueGetShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var value = observableDictionary[1]; };

            // then
            action.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void KeyIndexerValueSetShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { observableDictionary[1] = "One"; };

            // then
            action.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void TryGetShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<string, string>();
            observableDictionary.Dispose();
            // when
            string value;
            Action action = () => observableDictionary.TryGetValue("One", out value);

            // then
            action.Should().Throw<ObjectDisposedException>();  
        }

        [Fact]
        public void ContainsKeyShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<string, string>();
            observableDictionary.Dispose();
            // when
            Action action = () =>
            {
                var value = observableDictionary.ContainsKey("One");
            };

            // then
            action.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void ClearShouldThrowDisposedExceptionAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<string, string>();
            observableDictionary.Dispose();
            // when
            Action action = () =>
            {
                observableDictionary.Clear();
            };

            // then
            action.Should().Throw<ObjectDisposedException>();
        }
    }
}