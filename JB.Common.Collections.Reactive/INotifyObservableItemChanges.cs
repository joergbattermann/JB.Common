using System;
using System.ComponentModel;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableItemChanges
    {
        /// <summary>
        /// (Temporarily) suppresses item change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        IDisposable SuppressItemChangeNotifications(bool signalResetWhenFinished = true);

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if that interface is implemented, too.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsTrackingItemChanges { get; }
    }
}