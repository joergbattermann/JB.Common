using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    /// <summary>
    /// Classes implementing this interface provide an <see cref="DictionaryItemChanges">observable stream</see> of item changes IF
    /// <typeparam name="TValue" /> implements the <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface INotifyObservableDictionaryItemChanged<out TKey, out TValue>
    {
        /// <summary>
        /// (Temporarily) suppresses change notifications for <see cref="ObservableDictionaryChangeType.ItemChanged"/> events until the returned <see cref="IDisposable" />
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
        /// Gets the observable streams of item changes, however these will only have their
        /// <see cref="IObservableDictionaryChange{TKey, TValue}.ChangeType" /> set to <see cref="ObservableDictionaryChangeType.ItemChanged" />.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        IObservable<IObservableDictionaryChange<TKey, TValue>> DictionaryItemChanges { get; }
    }
}