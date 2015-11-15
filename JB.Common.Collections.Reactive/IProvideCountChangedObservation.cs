using System;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="CountChanges">observable stream</see> of changes to the instances .Count value.
    /// </summary>
    public interface IProvideCountChangedObservation
    {
        /// <summary>
        /// Gets a value indicating whether this instance signals changes to its items' count.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking counts; otherwise, <c>false</c>.
        /// </value>
        bool IsNotifyingAboutCountChanges { get; }

        /// <summary>
        /// Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The count changes.
        /// </value>
        IObservable<int> CountChanges { get; }

        /// <summary>
        /// (Temporarily) suppresses item count change notification until the returned <see cref="IDisposable" />
        /// has been Disposed.
        /// </summary>
        /// <param name="signalCurrentCountWhenFinished">if set to <c>true</c> signals a the current count when disposed.</param>
        /// <returns></returns>
        IDisposable SuppressCountChangedNotifications(bool signalCurrentCountWhenFinished = true);
    }
}