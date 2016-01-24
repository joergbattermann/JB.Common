// -----------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace JB.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="object"/> instances.
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="instance" /> to an observerable stream of <typeparamref name="TResult" />.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <remarks>
        /// If <paramref name="instance"/> is an <see cref="IObservable{TResult}"/>, it is returned as is.
        /// If <paramref name="instance"/> is an <see cref="IEnumerable{TResult}"/>, it will be iterated and its values piped into the observable stream.
        /// Otherwise, the <paramref name="instance"/> will be returned as-is.
        /// </remarks>
        /// <returns>An observable sequence of <typeparamref name="TResult"/> instances.</returns>
        public static IObservable<TResult> AsObservable<TResult>(this TResult instance)
        {
            if (instance == null)
                return Observable.Empty<TResult>();

            // check whether the instance actually / already is an observable
            var instanceAsObservable = instance as IObservable<TResult>;
            if (instanceAsObservable != null)
                return instanceAsObservable;

            // or an ienumerable
            var instanceAsEnumerable = instance as IEnumerable<TResult>;
            if (instanceAsEnumerable != null)
            {
                return Observable.Create<TResult>(observer =>
                {
                    try
                    {
                        foreach (var instanceAsType in instanceAsEnumerable)
                        {
                            observer.OnNext(instanceAsType);
                        }

                        observer.OnCompleted();
                    }
                    catch (Exception exception)
                    {
                        observer.OnError(exception);
                    }

                    return Disposable.Empty;
                });
            }

            // none of the above matched, oh well - return the item as is as an observable
            return Observable.Return(instance);
        }
    }
}