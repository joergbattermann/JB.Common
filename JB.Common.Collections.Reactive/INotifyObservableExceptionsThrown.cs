using System;

namespace JB.Collections.Reactive
{
    public interface INotifyObservableExceptionsThrown
    {
        /// <summary>
        /// Provides an observable sequence of <see cref="Exception">exceptions</see> thrown.
        /// </summary>
        /// <value>
        /// The thrown exceptions.
        /// </value>
        IObservable<Exception> ThrownExceptions { get; }
    }
}