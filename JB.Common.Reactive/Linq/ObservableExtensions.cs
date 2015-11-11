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
using System.Reactive.Linq;

namespace JB.Reactive.Linq
{
    /// <summary>
    /// Extension Methods for <see cref="IObservable{T}"/> instances.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based a specified condition OR when it is full.
        /// While the test is [true], the buffer will filled until the the source produces an element at the same time the condition is [false], the current buffer will be released.
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

            return Observable.Create<IList<TSource>>(observer =>
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
                ex =>
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
                    observer.OnError(ex);

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
        /// While the test is [true], the buffer will filled until the the source produces an element at the same time the condition is [false], the current buffer will be released.
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

            return Observable.Create<IList<TSource>>(observer =>
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
                ex =>
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
                    observer.OnError(ex);

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
        public static IObservable<IList<TSource>> BufferWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Observable.Create<IList<TSource>>(observer =>
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
                ex =>
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
                    observer.OnError(ex);

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

            return Observable.Create<IList<TSource>>(observer =>
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
                ex =>
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
                    observer.OnError(ex);

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
        public static IObservable<TSource> SkipWhileContinuously<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Observable.Create<TSource>(observer =>
            {
                var subscription = source.Subscribe(value =>
                {
                    if (predicate.Invoke(value) == false)
                        observer.OnNext(value);
                },
                ex => observer.OnError(ex),
                () => observer.OnCompleted());

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
        public static IObservable<TSource> SkipWhileContinuously<TSource>(this IObservable<TSource> source, Func<bool> predicate)
        {
            return Observable.Create<TSource>(observer =>
            {
                var subscription = source.Subscribe(value =>
                {
                    if (predicate.Invoke() == false)
                        observer.OnNext(value);
                },
                ex => observer.OnError(ex),
                () => observer.OnCompleted());

                return () => subscription.Dispose();
            });
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
        public static IObservable<TSource> TakeWhileContinuously<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Observable.Create<TSource>(observer =>
            {
                var subscription = source.Subscribe(value =>
                {
                    if (predicate.Invoke(value))
                        observer.OnNext(value);
                },
                ex => observer.OnError(ex),
                () => observer.OnCompleted());

                return () => subscription.Dispose();
            });
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
        public static IObservable<TSource> TakeWhileContinuously<TSource>(this IObservable<TSource> source, Func<bool> predicate)
        {
            return Observable.Create<TSource>(observer =>
            {
                var subscription = source.Subscribe(value =>
                {
                    if (predicate.Invoke())
                        observer.OnNext(value);
                },
                ex => observer.OnError(ex),
                () => observer.OnCompleted());

                return () => subscription.Dispose();
            });
        }
    }
}