using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableChanges
    {
        /// <summary>
        /// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if wanted and applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        IDisposable SuppressChangeNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets a value indicating whether this instance is currently suppressing observable collection changed notifications.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suppressing observable collection changed notifications; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingChanges { get; }

        /// <summary>
        /// Gets or sets the threshold of the minimum amount of changes to switch individual notifications to a reset one.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered as a reset.
        /// </value>
        int ThresholdAmountWhenChangesAreNotifiedAsReset { get; set; }
    }
}