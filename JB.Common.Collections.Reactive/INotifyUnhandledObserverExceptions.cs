using System;
using JB.Reactive;

namespace JB.Collections.Reactive
{
    public interface INotifyUnhandledObserverExceptions
    {
        /// <summary>
        /// Provides an observable sequence of unhandled <see cref="ObserverException">exceptions</see> thrown by observers.
        /// An <see cref="ObserverException"/> provides a <see cref="ObserverException.Handled"/> property, if set to [true] by
        /// any of the observers of <see cref="UnhandledObserverExceptions"/> observable, it is assumed to be safe to continue
        /// without re-throwing the exception.
        /// </summary>
        /// <value>
        /// An observable stream of unhandled exceptions.
        /// </value>
        IObservable<ObserverException> UnhandledObserverExceptions { get; }
    }
}