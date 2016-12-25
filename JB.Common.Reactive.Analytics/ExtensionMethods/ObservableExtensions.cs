// -----------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.Analyzers;

namespace JB.Reactive.Analytics.ExtensionMethods
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<IAnalysisResult> Analyze<TSource>(this IObservable<TSource> source, IAnalyzer<TSource> analyzer, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return source.Analyze<TSource, IAnalysisResult>(analyzer, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source, IAnalyzer<TSource, TAnalysisResult> analyzer, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                // first we wire up the analyzer with the source sequence
                var sourceAnalyticsProviderSubscription = scheduler != null
                    ? source.SubscribeOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                // then we wire up the returned observable with the analyzer's output sequence
                var sourceSequenceForwardingSubscription = analyzer.Subscribe(observer);

                return () => new CompositeDisposable(sourceSequenceForwardingSubscription, sourceAnalyticsProviderSubscription).Dispose();
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="actionToPerformOnAnalyzerUponSubscription">If provided, the <paramref name="actionToPerformOnAnalyzerUponSubscription"/> will be invoked immediately before subscribing to the <paramref name="source"/> sequence.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source, TAnalyzer analyzer, Action<TAnalyzer> actionToPerformOnAnalyzerUponSubscription = null, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                actionToPerformOnAnalyzerUponSubscription?.Invoke(analyzer);

                // first we wire up the analyzer with the source sequence
                var sourceAnalyticsProviderSubscription = scheduler != null
                    ? source.SubscribeOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                // then we wire up the returned observable with the analyzer's output sequence
                var sourceSequenceForwardingSubscription = analyzer.Subscribe(observer);

                return () => new CompositeDisposable(sourceSequenceForwardingSubscription, sourceAnalyticsProviderSubscription).Dispose();
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<IAnalysisResult> Analyze<TSource>(this IObservable<TSource> source, ICollection<IAnalyzer<TSource>> analyzers, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<IAnalysisResult>(observer =>
            {
                // first we wire up the analyzer with the source sequence
                var sourceAnalyticsProviderSubscription = new CompositeDisposable(
                    analyzers.Select(analyzer =>
                        scheduler != null
                            ? source.SubscribeOn(scheduler).Subscribe(analyzer)
                            : source.Subscribe(analyzer)));

                // then merge the analyzers' analysis sequence into one composite observable and subscribe the observer to it
                var compositeAnalyzersObservable =
                    analyzers.Aggregate<IAnalyzer<TSource>, IObservable<IAnalysisResult>>(null, (current, analyzer) => current == null ? analyzer : current.Merge(analyzer));

                var sourceSequenceForwardingSubscription = compositeAnalyzersObservable.Subscribe(observer);

                return () => new CompositeDisposable(sourceSequenceForwardingSubscription, sourceAnalyticsProviderSubscription).Dispose();
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source,
            ICollection<IAnalyzer<TSource, TAnalysisResult>> analyzers,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                // first we wire up the analyzer with the source sequence
                var sourceAnalyticsProviderSubscription = new CompositeDisposable(
                    analyzers.Select(analyzer =>
                        scheduler != null
                            ? source.SubscribeOn(scheduler).Subscribe(analyzer)
                            : source.Subscribe(analyzer)));

                // then merge the analyzers' analysis sequence into one composite observable and subscribe the observer to it
                var compositeAnalyzersObservable =
                    analyzers.Aggregate<IAnalyzer<TSource, TAnalysisResult>, IObservable<TAnalysisResult>>(null, (current, analyzer) => current == null ? analyzer : current.Merge(analyzer));

                var sourceSequenceForwardingSubscription = compositeAnalyzersObservable.Subscribe(observer);

                return () => new CompositeDisposable(sourceSequenceForwardingSubscription, sourceAnalyticsProviderSubscription).Dispose();
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<IAnalysisResult> Analyze<TSource>(this IObservable<TSource> source, params IAnalyzer<TSource>[] analyzers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Length == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.Analyze(analyzers.ToList());
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source, params IAnalyzer<TSource, TAnalysisResult>[] analyzers)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Length == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.Analyze(analyzers.ToList());
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<IAnalysisResult> Analyze<TSource>(this IObservable<TSource> source, IScheduler scheduler, params IAnalyzer<TSource>[] analyzers)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Length == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.Analyze(analyzers.ToList(), scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzers" /> on.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source, IScheduler scheduler, params IAnalyzer<TSource, TAnalysisResult>[] analyzers)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Length == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.Analyze(analyzers.ToList(), scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IObservable<ICountBasedAnalysisResult> AnalyzeCount<TSource>(this IObservable<TSource> source,
                            long initialCount = 0,
                            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source
                .Analyze(new CountAnalyzer<TSource>(initialCount), scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the overall throughput
        /// since the time of subscription of every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <param name="stopWatchProvider">The stop watch provider. If none is provided, the <paramref name="scheduler"/> will be used and if no scheduler is provided, the <see cref="Scheduler.Default"/> will be attempted to be used.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IObservable<IThroughputAnalysisResult> AnalyzeOverallThroughput<TSource>(
            this IObservable<TSource> source,
            long initialCount = 0,
            IScheduler scheduler = null,
            IStopwatchProvider stopWatchProvider = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (stopWatchProvider == null)
            {
                stopWatchProvider = (scheduler ?? Scheduler.Default).AsStopwatchProvider();
            }

            if (stopWatchProvider == null)
            {
                if (scheduler != null)
                {
                    throw new ArgumentException(
                        $"{nameof(scheduler)} provides no {nameof(IStopwatchProvider)} implementation. Please provide one explicitly.",
                        nameof(stopWatchProvider));
                }
                else
                {
                    throw new ArgumentException(
                        $"Default platform scheduler provides no {nameof(IStopwatchProvider)} implementation. Please provide one explicitly.",
                        nameof(stopWatchProvider));
                }
            }

            return source
                .Analyze<TSource, OverallThroughputAnalyzer<TSource>, IThroughputAnalysisResult>(
                    new OverallThroughputAnalyzer<TSource>(stopWatchProvider, initialCount, false),
                    throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                    scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the throughput of <typeparamref name="TSource" /> instances
        /// at a given timespan <paramref name="resolution" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="resolution">The resolution at which to sample the <paramref name="source" /> sequence.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <returns>
        /// A sequence of <see cref="IThroughputAnalysisResult" /> instances at the provided <paramref name="resolution" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">resolution - Must be at least 2 Ticks</exception>
        public static IObservable<IThroughputAnalysisResult> AnalyzeThroughput<TSource>(
            this IObservable<TSource> source,
            TimeSpan resolution,
            long initialCount = 0,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (resolution.Ticks <= 1) throw new ArgumentOutOfRangeException(nameof(resolution), "Must be at least 2 Ticks");

            return source
                .Analyze<TSource, ThroughputAnalyzer<TSource>, IThroughputAnalysisResult>(
                    new ThroughputAnalyzer<TSource>(resolution, initialCount, false, scheduler),
                    throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                    scheduler);
        }
    }
}