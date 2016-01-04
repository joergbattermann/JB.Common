// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryDisposalTests.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
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

            disposalAction.ShouldNotThrow<ObjectDisposedException>();
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
        public void ShouldNotThrowDisposedExceptionWhenAccessingInfrastructureRelevantPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action isDisposingPropertyAccess = () => { var isDisposing = observableDictionary.IsDisposing; };
            Action isDisposedPropertyAccess = () => { var isDisposed = observableDictionary.IsDisposed; };

            // then
            isDisposingPropertyAccess.ShouldNotThrow<ObjectDisposedException>();
            isDisposedPropertyAccess.ShouldNotThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingPropertiesAfterDisposal()
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
            Action isThrowingUnhandledObserverExceptionsPropertyAccess = () => { var isThrowingUnhandledObserverExceptions = observableDictionary.IsThrowingUnhandledObserverExceptions; };

            Action isEmptyPropertyAccess = () => { var isEmpty = observableDictionary.IsEmpty; };

            Action keysPropertyAccess = () => { var keys = observableDictionary.Keys; };
            Action valuesPropertyAccess = () => { var values = observableDictionary.Values; };

            Action thresholdAmountWhenItemChangesAreNotifiedAsResetPropertyGetAccess = () => { var thresholdAmountWhenItemChangesAreNotifiedAsReset = observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset; };
            Action thresholdAmountWhenItemChangesAreNotifiedAsResetPropertySetAccess = () => { observableDictionary.ThresholdAmountWhenItemChangesAreNotifiedAsReset = 1; };

            // then
            countPropertyAccess.ShouldThrow<ObjectDisposedException>();

            isTrackingChangesPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isTrackingCountChangesPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isTrackingItemChangesPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isTrackingResetsPropertyAccess.ShouldThrow<ObjectDisposedException>();
            isThrowingUnhandledObserverExceptionsPropertyAccess.ShouldThrow<ObjectDisposedException>();

            thresholdAmountWhenItemChangesAreNotifiedAsResetPropertyGetAccess.ShouldThrow<ObjectDisposedException>();
            thresholdAmountWhenItemChangesAreNotifiedAsResetPropertySetAccess.ShouldThrow<ObjectDisposedException>();

            isEmptyPropertyAccess.ShouldThrow<ObjectDisposedException>();

            keysPropertyAccess.ShouldThrow<ObjectDisposedException>();
            valuesPropertyAccess.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenGettingValueViaKeyIndexerAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { var value = observableDictionary[1]; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenSettingValueViaKeyIndexerAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
            Action action = () => { observableDictionary[1] = "One"; };

            // then
            action.ShouldThrow<ObjectDisposedException>();
        }
    }
}