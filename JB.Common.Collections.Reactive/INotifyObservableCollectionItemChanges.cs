using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="CollectionItemChanges">observable stream</see> of item changes IF
    /// <typeparam name="TItem"/> implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface INotifyObservableCollectionItemChanges<out TItem>
    {
        /// <summary>
        /// (Temporarily) suppresses change notifications for <see cref="ObservableCollectionChangeType.ItemChanged"/> events until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if that interface is implemented, too.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingItemChanges { get; }

        /// <summary>
        /// Gets the minimum amount of items that have been changed to be notified / considered a
        /// <see cref="ObservableCollectionChangeType.Reset"/> rather than individual <see cref="ObservableCollectionChangeType"/> notifications.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered reset.
        /// </value>
        int ThresholdAmountWhenItemChangesAreNotifiedAsReset { get; set; }

        /// <summary>
        /// Gets the observable streams of collection item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IObservableCollectionChange<TItem>> CollectionItemChanges { get; }
    }
}