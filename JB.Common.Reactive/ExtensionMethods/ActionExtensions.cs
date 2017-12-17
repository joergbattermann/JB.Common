// -----------------------------------------------------------------------
// <copyright file="ActionExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace JB.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="Action"/> instances
    /// </summary>
    public static class ActionExtensions
    {
        /// <summary>
        /// Returns an observable sequence that schedules the given <paramref name="action"/> on the <paramref name="scheduler"/> for immediate execution upon subscription.
        /// </summary>
        /// <param name="action">Action to run on subscription on the <paramref name="scheduler"/>.</param>
        /// <param name="scheduler">Scheduler to run the <paramref name="action"/> on.</param>
        /// <returns>
        /// An observable sequence exposing a Unit value upon scheduling of the <paramref name="action"/>, or an exception.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="action"/> or <paramref name="scheduler"/> is  null.
        /// </exception>
        public static IObservable<Unit> ToObservable(this Action action, IScheduler scheduler = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            return scheduler != null
                ? Observable.Start(action, scheduler)
                : Observable.Start(action);
        }
    }
}