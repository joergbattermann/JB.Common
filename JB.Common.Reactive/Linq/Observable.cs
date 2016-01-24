// -----------------------------------------------------------------------
// <copyright file="Observable.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace JB.Reactive.Linq
{
    public static class Observable
    {
        /// <summary>
        /// Invokes the <paramref name="action"/> synchronously upon subscription.
        /// </summary>
        /// <param name="action">Action to run on subscription.</param>
        /// <returns>
        /// An observable sequence exposing the result value upon completion of the given <see cref="action"/>, or an exception if one occured.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static IObservable<Unit> Run(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            return System.Reactive.Linq.Observable.Create<Unit>(observer =>
            {
                try
                {
                    action.Invoke();

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Invokes the <paramref name="func"/> synchronously upon subscription and returns its result.
        /// </summary>
        /// <param name="func">Function to run on subscription.</param>
        /// <returns>
        /// An observable sequence exposing the result value upon completion of the given <see cref="func"/>, or an exception if one occured.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="func"/> is null.</exception>
        public static IObservable<TResult> Run<TResult>(Func<TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            return System.Reactive.Linq.Observable.Create<TResult>(observer =>
            {
                try
                {
                    observer.OnNext(func.Invoke());
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Upon subscription the <paramref name="action"/> is scheduled on the <paramref name="scheduler"/> for immediate execution.
        /// </summary>
        /// <param name="action">Action to run on subscription on the <paramref name="scheduler"/>.</param>
        /// <param name="scheduler">Scheduler to run the <paramref name="action"/> on.</param>
        /// <returns>
        /// An observable sequence exposing a Unit value upon scheduling of the <paramref name="action"/>, or an exception.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="action"/> or <paramref name="scheduler"/> is  null.
        /// </exception>
        public static IObservable<Unit> Run(Action action, IScheduler scheduler)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            return System.Reactive.Linq.Observable.Create<Unit>(observer =>
            {
                IDisposable schedulerCancellationToken = Disposable.Empty;
                try
                {
                    schedulerCancellationToken = scheduler.Schedule(action);

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return schedulerCancellationToken;
            });
        }
    }
}