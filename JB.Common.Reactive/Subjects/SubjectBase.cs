// -----------------------------------------------------------------------
// <copyright file="Class1.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
using System;
using System.Reactive.Subjects;


namespace JB.Reactive.Subjects
{
    /// <summary>
    /// Base class for objects that are both an observable sequence as well as an observer.
    /// (Temporarily, until RX 2.3.x is out, taken from https://github.com/Reactive-Extensions/Rx.NET/blob/master/Rx.NET/Source/System.Reactive.Linq/Reactive/Subjects/SubjectBase.cs)
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [Obsolete("Once RX.Net 2.3 (or 3?) is available, remove this again and use core RX System.Reactive.Subjects.SubjectBase<T> instead")]
    public abstract class SubjectBase<T> : ISubject<T>, IDisposable
    {
        /// <summary>
        /// Indicates whether the subject has observers subscribed to it.
        /// </summary>
        public abstract bool HasObservers { get; }

        /// <summary>
        /// Indicates whether the subject has been disposed.
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// Releases all resources used by the current instance of the subject and unsubscribes all observers.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Notifies all subscribed observers about the end of the sequence.
        /// </summary>
        public abstract void OnCompleted();

        /// <summary>
        /// Notifies all subscribed observers about the specified exception.
        /// </summary>
        /// <param name="error">The exception to send to all currently subscribed observers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public abstract void OnError(Exception error);

        /// <summary>
        /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
        /// </summary>
        /// <param name="value">The value to send to all currently subscribed observers.</param>
        public abstract void OnNext(T value);

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
        public abstract IDisposable Subscribe(IObserver<T> observer);
    }
}