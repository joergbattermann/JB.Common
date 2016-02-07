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
using System.Reactive.Linq;
using System.Reflection;

namespace JB.Reactive.Linq
{
    /// <summary>
    /// Helper methods for <see cref="IObservable{T}"/> instances
    /// </summary>
    public static class Observable
    {
        /// <summary>
        /// Invokes the <paramref name="action"/> synchronously upon subscription.
        /// </summary>
        /// <param name="action">Action to run on subscription.</param>
        /// <param name="scheduler">Scheduler to perform the <paramref name="action"/> on.</param>
        /// <returns>
        /// An observable sequence signaling the completion of the given <paramref name="action"/>, or an exception if one occured.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static IObservable<Unit> Run(Action action, IScheduler scheduler = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Func<IObserver<Unit>, IDisposable> observableAction = (observer) =>
            {
                try
                {
                    action.Invoke();

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    if (targetInvocationException.InnerException != null)
                        observer.OnError(targetInvocationException.InnerException);
                    else
                        observer.OnError(targetInvocationException);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            };

            return scheduler != null
                ? System.Reactive.Linq.Observable.Create(observableAction).SubscribeOn(scheduler)
                : System.Reactive.Linq.Observable.Create(observableAction);
        }

        /// <summary>
        /// Invokes the <paramref name="action"/> synchronously upon subscription and returns its return value.
        /// </summary>
        /// <param name="action">Action to run on subscription.</param>
        /// <param name="scheduler">Scheduler to perform the <paramref name="action"/> on.</param>
        /// <returns>
        /// An observable sequence exposing the result value upon completion of the given <paramref name="action"/>, or an exception if one occured.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static IObservable<TResult> Run<TResult>(Func<TResult> action, IScheduler scheduler = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Func<IObserver<TResult>, IDisposable> observableAction = (observer) =>
            {
                try
                {
                    observer.OnNext(action.Invoke());
                    observer.OnCompleted();
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    if (targetInvocationException.InnerException != null)
                        observer.OnError(targetInvocationException.InnerException);
                    else
                        observer.OnError(targetInvocationException);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            };

            return scheduler != null
                ? System.Reactive.Linq.Observable.Create(observableAction).SubscribeOn(scheduler)
                : System.Reactive.Linq.Observable.Create(observableAction);
        }
    }
}