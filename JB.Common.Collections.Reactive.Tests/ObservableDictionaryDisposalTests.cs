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

            Action countPropertyAccess = () => { var count = observableDictionary.Count; };

            // then
            isDisposingPropertyAccess.ShouldNotThrow<ObjectDisposedException>();
            isDisposedPropertyAccess.ShouldNotThrow<ObjectDisposedException>();

            countPropertyAccess.ShouldNotThrow<ObjectDisposedException>();
        }

        [Fact]
        public void ShouldThrowDisposedExceptionWhenAccessingPropertiesAfterDisposal()
        {
            // given
            var observableDictionary = new ObservableDictionary<int, string>();
            observableDictionary.Dispose();

            // when
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
    }
}