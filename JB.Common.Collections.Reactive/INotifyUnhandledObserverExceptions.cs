using System;

namespace JB.Collections.Reactive
{
    public interface INotifyUnhandledObserverExceptions
    {
        /// <summary>
        /// Gets a value indicating whether this instance is notifying about unhandled observer exceptions via <see cref="UnhandledObserverExceptions"/>.
        /// </summary>
        /// <remarks>
        /// If this is set to [false], all unhandled Observer exceptions will be forwarded to <see cref="UnhandledObserverExceptions"/>
        /// and will not be thrown any further. If set to [true] however, the Exceptions will also be (re-)thrown where they are caught.
        /// </remarks>
        /// <value>
        /// <c>true</c> if this instance is notifying about unhandled observer exceptions; otherwise, <c>false</c>.
        /// </value>
        bool IsThrowingUnhandledObserverExceptions { get; set; }

        /// <summary>
        /// Provides an observable sequence of unhandled <see cref="Exception">exceptions</see> thrown by observers.
        /// </summary>
        /// <value>
        /// An observable stream of unhandled exceptions.
        /// </value>
        IObservable<Exception> UnhandledObserverExceptions { get; }
    }
}