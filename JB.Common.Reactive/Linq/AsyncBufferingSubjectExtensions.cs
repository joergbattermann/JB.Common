// -----------------------------------------------------------------------
// <copyright file="AsyncBufferingSubjectExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using JB.Reactive.Subjects;

namespace JB.Reactive.Linq
{
    /// <summary>
    /// Extension methods for <see cref="AsyncBufferingSubject{T}"/> instances
    /// </summary>
    public static class AsyncBufferingSubjectExtensions
    {
        /// <summary>
        /// Sends an <see cref="OperationCanceledException" /> to the specified subject and returns it back for further usage.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="subject">The subject.</param>
        /// <returns>The provided <paramref name="subject"/>.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        internal static AsyncBufferingSubject<TSource> Cancel<TSource>(this AsyncBufferingSubject<TSource> subject)
        {
            if (subject == null) throw new ArgumentNullException(nameof(subject));

            subject.OnError(new OperationCanceledException());
            return subject;
        }

        /// <summary>
        /// Takes the <paramref name="cancellationToken"/> and makes sure, if it gets triggered, the <paramref name="subscription"/> gets disposed and the <paramref name="subject"/>
        /// gets an <see cref="Cancel{TSource}"/> notification.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="subject">The subject.</param>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        internal static void RegisterCancellation<TSource>(this AsyncBufferingSubject<TSource> subject, IDisposable subscription, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.CanBeCanceled == false)
                return;

            // if the cancellation token gets triggered, clean up subscription first and then forward cancellation to the subject
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                subscription?.Dispose();
                subject?.Cancel();
            });

            // and finally this makes sure the cancellationTokenRegistration gets cleaned up on completion / error, too
            subject.Subscribe(_ => { }, _ => cancellationTokenRegistration.Dispose(), cancellationTokenRegistration.Dispose);
        }
    }
}