// -----------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace JB.Reactive.Linq
{
    /// <summary>
    /// Extension Methods for <see cref="IObservable{T}"/> instances.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Takes a source observable and splits its sequence forwarding into two target observers based on a given <paramref name="predicate"/> condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="predicate">A function to test each element for a condition whether to pipe the value into <paramref name="targetForTrue"/> or <paramref name="targetForFalse"/>.</param>
        /// <param name="targetForTrue">The target observer if <paramref name="predicate"/> returned [true].</param>
        /// <param name="targetForFalse">The target observer if <paramref name="predicate"/> returned [false].</param>
        /// <param name="scheduler">The scheduler to schedule observer notifications on, if any.</param>
        /// <returns></returns>
        public static IDisposable SplitTwoWays<TSource>(this IObservable<TSource> source,
            Func<TSource, bool> predicate,
            IObserver<TSource> targetForTrue,
            IObserver<TSource> targetForFalse,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (targetForTrue == null) throw new ArgumentNullException(nameof(targetForTrue));
            if (targetForFalse == null) throw new ArgumentNullException(nameof(targetForFalse));

            var actualTargetForTrue = scheduler != null
                ? targetForTrue.NotifyOn(scheduler)
                : targetForTrue;

            var actualTargetForFalse = scheduler != null
                ? targetForFalse.NotifyOn(scheduler)
                : targetForFalse;

            return source.Subscribe(value =>
            {
                if (predicate.Invoke(value) == true)
                {
                    actualTargetForTrue.OnNext(value);
                }
                else
                {
                    actualTargetForFalse.OnNext(value);
                }
            },
            exception =>
            {
                actualTargetForTrue.OnError(exception);
                actualTargetForFalse.OnError(exception);
            },
            () =>
            {
                actualTargetForTrue.OnCompleted();
                actualTargetForFalse.OnCompleted();
            });
        }

        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="targetObservers">The target observers to forward the sequence to.</param>
        /// <returns>An <see cref="IDisposable"/> representing the inner forwarding <paramref name="source"/> subscription.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IDisposable Split<TSource>(this IObservable<TSource> source, params IObserver<TSource>[] targetObservers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            return source.Subscribe(value =>
            {
                foreach (var targetObserver in targetObservers)
                {
                    targetObserver.OnNext(value);
                }
            },
            exception =>
            {
                foreach (var targetObserver in targetObservers)
                {
                    targetObserver.OnError(exception);
                }
            },
            () =>
            {
                foreach (var targetObserver in targetObservers)
                {
                    targetObserver.OnCompleted();
                }
            });
        }

        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="targetObservers">The target observers to forward the sequence to.</param>
        /// <param name="scheduler">The scheduler to schedule observer notifications on, if any.</param>
        /// <returns>
        /// An <see cref="IDisposable" /> representing the inner forwarding <paramref name="source" /> subscription.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IDisposable Split<TSource>(this IObservable<TSource> source, IEnumerable<IObserver<TSource>> targetObservers, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            var actualTargetObservers = scheduler != null
                ? targetObservers.Select(targetObserver => targetObserver.NotifyOn(scheduler)).ToList()
                : targetObservers.ToList();

            return source.Subscribe(value =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnNext(value);
                }
            },
            exception =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnError(exception);
                }
            },
            () =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnCompleted();
                }
            });
        }

        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to schedule observer notifications on.</param>
        /// <param name="targetObservers">The target observers to forward the sequence to.</param>
        /// <returns>An <see cref="IDisposable"/> representing the inner forwarding <paramref name="source"/> subscription.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IDisposable Split<TSource>(this IObservable<TSource> source, IScheduler scheduler, params IObserver<TSource>[] targetObservers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            var actualTargetObservers = targetObservers.Select(targetObserver => targetObserver.NotifyOn(scheduler)).ToList();
            return source.Subscribe(value =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnNext(value);
                }
            },
            exception =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnError(exception);
                }
            },
            () =>
            {
                foreach (var targetObserver in actualTargetObservers)
                {
                    targetObserver.OnCompleted();
                }
            });
        }

        /// <summary>
        /// Merges elements from multiple observable sequences into a single observable sequence,
        /// when specified a scheduler will be used for enumeration of and subscription to the sources.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="observable">First observable sequence.</param>
        /// <param name="others">The observable sequence(s) to merge <paramref name="observable"/> with.</param>
        /// <returns>
        /// The observable sequence that merges the elements of the given sequences.
        /// </returns>
        public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> observable, params IObservable<TSource>[] others)
        {
            if (observable == null) throw new ArgumentNullException(nameof(observable));

            if (others == null || others.Length == 0)
                return observable;

            // else
            return others.Aggregate(observable, (current, analyzer) => current.Merge(analyzer));
        }

        /// <summary>
        /// Merges elements from multiple observable sequences into a single observable sequence,
        /// when specified a scheduler will be used for enumeration of and subscription to the sources.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="observable">First observable sequence.</param>
        /// <param name="others">The observable sequence(s) to merge <paramref name="observable"/> with.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given sequences.</param>
        /// <returns>
        /// The observable sequence that merges the elements of the given sequences.
        /// </returns>
        public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> observable, IScheduler scheduler, params IObservable<TSource>[] others)
        {
            if (observable == null) throw new ArgumentNullException(nameof(observable));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            if (others == null || others.Length == 0)
                return observable;

            // else
            return others.Aggregate(observable, (current, analyzer) => current.Merge(analyzer, scheduler));
        }

        /// <summary>
        /// Merges elements from multiple observable sequences into a single observable sequence,
        /// when specified a scheduler will be used for enumeration of and subscription to the sources.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="observable">First observable sequence.</param>
        /// <param name="others">The observable sequence(s) to merge <paramref name="observable"/> with.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given sequences.</param>
        /// <returns>
        /// The observable sequence that merges the elements of the given sequences.
        /// </returns>
        public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> observable, IEnumerable<IObservable<TSource>> others, IScheduler scheduler = null)
        {
            if (observable == null) throw new ArgumentNullException(nameof(observable));

            if (others == null)
                return observable;

            // else
            return others.Aggregate(observable,
                (current, analyzer) => scheduler != null
                    ? current.Merge(analyzer, scheduler)
                    : current.Merge(analyzer));
        }

        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers and returns the raw sequence back again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="targetObservers">The target observers to forward the sequence to.</param>
        /// <returns>A new <see cref="IObservable{TSource}"/> providing the full <paramref name="source"/> sequence</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TSource> ForwardTo<TSource>(this IObservable<TSource> source, params IObserver<TSource>[] targetObservers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            return System.Reactive.Linq.Observable.Create<TSource>(observer =>
            {
                var subscription = source.Subscribe(value =>
                {
                    foreach (var targetObserver in targetObservers)
                    {
                        targetObserver.OnNext(value);
                    }

                    observer.OnNext(value);
                },
                exception =>
                {
                    foreach (var targetObserver in targetObservers)
                    {
                        targetObserver.OnError(exception);
                    }

                    observer.OnError(exception);
                },
                () =>
                {
                    foreach (var targetObserver in targetObservers)
                    {
                        targetObserver.OnCompleted();
                    }

                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }
        
        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers and returns the raw sequence back again..
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to schedule observer notifications on.</param>
        /// <param name="targetObservers">The target observers.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TSource> ForwardTo<TSource>(this IObservable<TSource> source, IScheduler scheduler, params IObserver<TSource>[] targetObservers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            return System.Reactive.Linq.Observable.Create<TSource>(observer =>
            {
                var actualTargetObservers = targetObservers.Select(targetObserver => targetObserver.NotifyOn(scheduler)).ToList();
                var subscription = source.Subscribe(value =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnNext(value);
                    }

                    observer.OnNext(value);
                },
                exception =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnError(exception);
                    }

                    observer.OnError(exception);
                },
                () =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnCompleted();
                    }

                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Takes a source observable and forwards its sequence into target observers and returns the raw sequence back again..
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to schedule observer notifications on, if any.</param>
        /// <param name="targetObservers">The target observers.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TSource> ForwardTo<TSource>(this IObservable<TSource> source, IEnumerable<IObserver<TSource>> targetObservers, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetObservers == null) throw new ArgumentNullException(nameof(targetObservers));

            return System.Reactive.Linq.Observable.Create<TSource>(observer =>
            {
                var actualTargetObservers = scheduler != null
                    ? targetObservers.Select(targetObserver => targetObserver.NotifyOn(scheduler)).ToList()
                    : targetObservers.ToList();

                var subscription = source.Subscribe(value =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnNext(value);
                    }

                    observer.OnNext(value);
                },
                exception =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnError(exception);
                    }

                    observer.OnError(exception);
                },
                () =>
                {
                    foreach (var actualTargetObserver in actualTargetObservers)
                    {
                        actualTargetObserver.OnCompleted();
                    }

                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based a specified condition OR when it is full.
        /// While the test is [true], the buffer will filled until the the source produces an element at the same time the condition is [false],
        /// then or whenever the <paramref name="count"/> is reach, the current buffer will be released.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence, and in the lists in the result sequence.</typeparam>
        /// <param name="source">Source sequence to produce buffers over.</param>
        /// <param name="predicate">A function to test each element for a condition whether current buffer can be released.</param>
        /// <param name="count"> Maximum element count of a window.</param>
        /// <returns>
        /// An observable sequence of buffers.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="count"/> is less than or equal to zero.</exception>
        public static IObservable<IList<TSource>> BufferWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

            return System.Reactive.Linq.Observable.Create<IList<TSource>>(observer =>
            {
                var bufferLocker = new object();
                var currentBuffer = new List<TSource>();

                var subscription = source.Subscribe(value =>
                {
                    var doBuffer = predicate.Invoke(value);
                    if (doBuffer)
                    {
                        lock (bufferLocker)
                        {
                            currentBuffer.Add(value);
                        }
                    }

                    // release buffer if max count is reached or the predicated indicated current buffer period is over but we haven't released the buffer in this cycle, yet
                    if (doBuffer && currentBuffer.Count >= count || !doBuffer && currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            if (!doBuffer) // adding value in case it wasn't added to buffer above due to the predicate
                                result.Add(value);

                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                },
                exception =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnError event
                    observer.OnError(exception);

                },
                () =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnCompleted event
                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based a specified condition OR when it is full.
        /// While the test is [true], the buffer will filled until the the source produces an element at the same time the condition is [false],
        /// then or whenever the <paramref name="count"/> is reach, the current buffer will be released.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence, and in the lists in the result sequence.</typeparam>
        /// <param name="source">Source sequence to produce buffers over.</param>
        /// <param name="predicate">A function to test each element for a condition whether current buffer can be released.</param>
        /// <param name="count"> Maximum element count of a window.</param>
        /// <returns>
        /// An observable sequence of buffers.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="count"/> is less than or equal to zero.</exception>
        public static IObservable<IList<TSource>> BufferWhile<TSource>(this IObservable<TSource> source, Func<bool> predicate, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

            return System.Reactive.Linq.Observable.Create<IList<TSource>>(observer =>
            {
                var bufferLocker = new object();
                var currentBuffer = new List<TSource>();

                var subscription = source.Subscribe(value =>
                {
                    var doBuffer = predicate.Invoke();
                    if (doBuffer)
                    {
                        lock (bufferLocker)
                        {
                            currentBuffer.Add(value);
                        }
                    }

                    // release buffer if max count is reached or the predicated indicated current buffer period is over but we haven't released the buffer in this cycle, yet
                    if (doBuffer && currentBuffer.Count >= count || !doBuffer && currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            if (!doBuffer) // adding value in case it wasn't added to buffer above due to the predicate
                                result.Add(value);

                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                },
                exception =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnError event
                    observer.OnError(exception);

                },
                () =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnCompleted event
                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based a specified condition.
        /// While the test is [true], the buffer will be filled until the the source produces an element at the same time the condition is [false],
        /// then the current buffer will be released.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence, and in the lists in the result sequence.</typeparam>
        /// <param name="source">Source sequence to produce buffers over.</param>
        /// <param name="predicate">A function to test each element for a condition whether current buffer can be released.</param>
        /// <returns>
        /// An observable sequence of buffers.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IObservable<IList<TSource>> BufferWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return System.Reactive.Linq.Observable.Create<IList<TSource>>(observer =>
            {
                var bufferLocker = new object();
                var currentBuffer = new List<TSource>();

                var subscription = source.Subscribe(value =>
                {
                    var doBuffer = predicate.Invoke(value);
                    if (doBuffer)
                    {
                        lock (bufferLocker)
                        {
                            currentBuffer.Add(value);
                        }
                    }
                    else
                    {
                        if (currentBuffer.Count > 0)
                        {
                            lock (bufferLocker)
                            {
                                var result = currentBuffer.ToList();
                                result.Add(value); // adding value in case it wasn't added to buffer above due to the predicate

                                currentBuffer.Clear();
                                observer.OnNext(result);
                            }
                        }
                    }
                },
                exception =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnError event
                    observer.OnError(exception);

                },
                () =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnCompleted event
                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based a specified condition.
        /// While the test is [true], the buffer will filled until the the source produces an element at the same time the condition is [false], the current buffer will be released.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence, and in the lists in the result sequence.</typeparam>
        /// <param name="source">Source sequence to produce buffers over.</param>
        /// <param name="predicate">A function to test each element for a condition whether current buffer can be released.</param>
        /// <returns>
        /// An observable sequence of buffers.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IObservable<IList<TSource>> BufferWhile<TSource>(this IObservable<TSource> source, Func<bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return System.Reactive.Linq.Observable.Create<IList<TSource>>(observer =>
            {
                var bufferLocker = new object();
                var currentBuffer = new List<TSource>();

                var subscription = source.Subscribe(value =>
                {
                    var doBuffer = predicate.Invoke();
                    if (doBuffer)
                    {
                        lock (bufferLocker)
                        {
                            currentBuffer.Add(value);
                        }
                    }
                    else
                    {
                        if (currentBuffer.Count > 0)
                        {
                            lock (bufferLocker)
                            {
                                var result = currentBuffer.ToList();
                                result.Add(value); // adding value in case it wasn't added to buffer above due to the predicate

                                currentBuffer.Clear();
                                observer.OnNext(result);
                            }
                        }
                    }
                },
                exception =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnError event
                    observer.OnError(exception);

                },
                () =>
                {
                    // release current buffer
                    if (currentBuffer.Count > 0)
                    {
                        lock (bufferLocker)
                        {
                            var result = currentBuffer.ToList();
                            currentBuffer.Clear();
                            observer.OnNext(result);
                        }
                    }
                    // then signal actual OnCompleted event
                    observer.OnCompleted();
                });

                return () => subscription.Dispose();
            });
        }

        /// <summary>
        /// Bypasses elements in an observable sequence while a specified condition is [true] and returns the elements whenever the condition is [false].
        /// 
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An observable sequence that contains the elements from the input sequence that occur while the test specified by predicate does not pass.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IObservable<TSource> SkipContinuouslyWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Where(value => predicate.Invoke(value) == false);
        }

        /// <summary>
        /// Bypasses elements in an observable sequence while a specified condition is [true] and returns the elements whenever the condition is [false].
        /// 
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An observable sequence that contains the elements from the input sequence that occur while the test specified by predicate does not pass.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IObservable<TSource> SkipContinuouslyWhile<TSource>(this IObservable<TSource> source, Func<bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Where(_ => predicate.Invoke() == false);
        }

        /// <summary>
        /// Returns elements from an observable sequence while a specified condition is [true] and discards the elements while the condition is [false].
        /// 
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">A sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An observable sequence that contains the elements from the input sequence that occur while the test specified by predicate passes.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IObservable<TSource> TakeContinuouslyWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Where(predicate.Invoke);
        }

        /// <summary>
        /// Returns elements from an observable sequence while a specified condition is [true] and discards the elements while the condition is [false].
        /// 
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">A sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An observable sequence that contains the elements from the input sequence that occur while the test specified by predicate passes.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IObservable<TSource> TakeContinuouslyWhile<TSource>(this IObservable<TSource> source, Func<bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Where(_ => predicate.Invoke());
        }
    }
}