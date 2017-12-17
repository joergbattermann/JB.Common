// -----------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
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
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, IAnalyzer<TSource> analyzer, IObserver<IAnalysisResult> analysisObserver, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return source.AnalyzeWith<TSource>(analyzer, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, IAnalyzer<TSource> analyzer, IObserver<IAnalysisResult> analysisObserver, Action<IAnalyzer<TSource>> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return Observable.Create<TSource>(observer =>
            {
                actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);

                var sourceConsumerSubscription = scheduler != null
                    ? source.ObserveOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                var analyzerProducerSubscription = analyzer.Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, IAnalyzer<TSource, TAnalysisResult> analyzer, IObserver<TAnalysisResult> analysisObserver, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return source.AnalyzeWith(analyzer, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, IAnalyzer<TSource, TAnalysisResult> analyzer, IObserver<TAnalysisResult> analysisObserver, Action<IAnalyzer<TSource, TAnalysisResult>> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return Observable.Create<TSource>(observer =>
            {
                actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);

                var sourceConsumerSubscription = scheduler != null
                    ? source.ObserveOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                var analyzerProducerSubscription = analyzer.Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source, TAnalyzer analyzer, IObserver<TAnalysisResult> analysisObserver, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return source.AnalyzeWith(analyzer, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzer</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source, TAnalyzer analyzer, IObserver<TAnalysisResult> analysisObserver, Action<TAnalyzer> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return Observable.Create<TSource>(observer =>
            {
                actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);

                var sourceConsumerSubscription = scheduler != null
                    ? source.ObserveOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                var analyzerProducerSubscription = analyzer.Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, ICollection<IAnalyzer<TSource>> analyzers, IObserver<IAnalysisResult> analysisObserver, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.AnalyzeWith<TSource>(analyzers, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, ICollection<IAnalyzer<TSource>> analyzers, IObserver<IAnalysisResult> analysisObserver, Action<IAnalyzer<TSource>> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<TSource>(observer =>
            {
                foreach (var analyzer in analyzers)
                {
                    actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);
                }

                var sourceConsumerSubscription = Linq.ObservableExtensions.SubscribeAll(source, analyzers, scheduler);

                var analyzerProducerSubscription = analyzers.Merge().Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, ICollection<IAnalyzer<TSource, TAnalysisResult>> analyzers, IObserver<TAnalysisResult> analysisObserver, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.AnalyzeWith<TSource, TAnalysisResult>(analyzers, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source,
            ICollection<IAnalyzer<TSource, TAnalysisResult>> analyzers,
            IObserver<TAnalysisResult> analysisObserver,
            Action<IAnalyzer<TSource, TAnalysisResult>> actionToPerformWithAnalyzerOnSubscription,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<TSource>(observer =>
            {
                foreach (var analyzer in analyzers)
                {
                    actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);
                }

                var sourceConsumerSubscription = Linq.ObservableExtensions.SubscribeAll(source, analyzers, scheduler);

                var analyzerProducerSubscription = analyzers.Merge().Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source,
            ICollection<TAnalyzer> analyzers,
            IObserver<TAnalysisResult> analysisObserver,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return source.AnalyzeWith<TSource, TAnalyzer, TAnalysisResult>(analyzers, analysisObserver, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the original <see cref="source" /> back sequence again.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription" /> will be invoked immediately before subscribing to the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analyzers</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">analyzers</exception>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source,
            ICollection<TAnalyzer> analyzers,
            IObserver<TAnalysisResult> analysisObserver,
            Action<IAnalyzer<TSource, TAnalysisResult>> actionToPerformWithAnalyzerOnSubscription,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<TSource>(observer =>
            {
                foreach (var analyzer in analyzers)
                {
                    actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);
                }

                var sourceConsumerSubscription = Linq.ObservableExtensions.SubscribeAll(source, analyzers.OfType<IObserver<TSource>>(), scheduler);

                var analyzerProducerSubscription = analyzers.OfType<IObservable<TAnalysisResult>>().Merge().Subscribe(analysisObserver);

                var sourceForwardingSubscription = source.Subscribe(observer);

                return new CompositeDisposable(sourceForwardingSubscription, sourceConsumerSubscription, analyzerProducerSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
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

            return source.Analyze(analyzer, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription"/> will be invoked immediately before subscribing to the <paramref name="source"/> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source, IAnalyzer<TSource, TAnalysisResult> analyzer, Action<IAnalyzer<TSource, TAnalysisResult>> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);

                var sourceConsumerSubscription = scheduler != null
                    ? source.ObserveOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                var analyzerProducerSubscription = analyzer.Subscribe(observer);

                return new CompositeDisposable(sourceConsumerSubscription, analyzerProducerSubscription);
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
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given
        /// source sequence and thereby running the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source, TAnalyzer analyzer, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return source.Analyze<TSource, TAnalyzer, TAnalysisResult>(analyzer, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzer" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription"/> will be invoked immediately before subscribing to the <paramref name="source"/> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzer" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzer" /> produces.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalyzer, TAnalysisResult>(this IObservable<TSource> source, TAnalyzer analyzer, Action<TAnalyzer> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
            where TAnalyzer : IAnalyzer<TSource, TAnalysisResult>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);

                var sourceConsumerSubscription = scheduler != null
                    ? source.ObserveOn(scheduler).Subscribe(analyzer)
                    : source.Subscribe(analyzer);

                var analyzerProducerSubscription = analyzer.Subscribe(observer);

                return new CompositeDisposable(sourceConsumerSubscription, analyzerProducerSubscription);
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

            return source.Analyze(analyzers, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription"/> will be invoked immediately before subscribing to the <paramref name="source"/> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<IAnalysisResult> Analyze<TSource>(this IObservable<TSource> source, ICollection<IAnalyzer<TSource>> analyzers, Action<IAnalyzer<TSource>> actionToPerformWithAnalyzerOnSubscription, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<IAnalysisResult>(observer =>
            {
                foreach (var analyzer in analyzers)
                {
                    actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);
                }

                var sourceConsumerSubscriptions = new CompositeDisposable(
                    analyzers.Select(analyzer =>
                        scheduler != null
                            ? source.ObserveOn(scheduler).Subscribe(analyzer)
                            : source.Subscribe(analyzer)));

                var analyzerProducersSubscription = analyzers.Merge().Subscribe(observer);

                return new CompositeDisposable(sourceConsumerSubscriptions, analyzerProducersSubscription);
            });
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
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

            return source.Analyze(analyzers, null, scheduler);
        }

        /// <summary>
        /// Takes the source sequence, subscribes the <paramref name="analyzers" /> to it and returns the analyzer's <see cref="IAnalysisResult" /> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="actionToPerformWithAnalyzerOnSubscription">If provided, the <paramref name="actionToPerformWithAnalyzerOnSubscription"/> will be invoked immediately before subscribing to the <paramref name="source"/> sequence.</param>
        /// <param name="scheduler">The Scheduler used to run the <paramref name="analyzers" /> on.</param>
        /// <returns>
        /// The observable sequence that contains the analysis results the <paramref name="analyzers" /> produce.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static IObservable<TAnalysisResult> Analyze<TSource, TAnalysisResult>(this IObservable<TSource> source,
            ICollection<IAnalyzer<TSource, TAnalysisResult>> analyzers,
            Action<IAnalyzer<TSource, TAnalysisResult>> actionToPerformWithAnalyzerOnSubscription,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analyzers.Count == 0) throw new ArgumentOutOfRangeException(nameof(analyzers));

            return Observable.Create<TAnalysisResult>(observer =>
            {
                foreach (var analyzer in analyzers)
                {
                    actionToPerformWithAnalyzerOnSubscription?.Invoke(analyzer);
                }

                var sourceConsumerSubscriptions = new CompositeDisposable(
                    analyzers.Select(analyzer =>
                        scheduler != null
                            ? source.ObserveOn(scheduler).Subscribe(analyzer)
                            : source.Subscribe(analyzer)));
               
                var analyzerProducersSubscription = analyzers.Merge().Subscribe(observer);

                return new CompositeDisposable(sourceConsumerSubscriptions, analyzerProducersSubscription);
            });
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IObservable<ICountBasedAnalysisResult> AnalyzeCount<TSource>(this IObservable<TSource> source,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source
                .AnalyzeCount(0, scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IObservable<ICountBasedAnalysisResult> AnalyzeCount<TSource>(this IObservable<TSource> source,
            long initialCount,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source
                .Analyze(new CountAnalyzer<TSource>(initialCount), scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analysisObserver</exception>
        public static IObservable<TSource> AnalyzeCountWith<TSource>(
            this IObservable<TSource> source,
            IObserver<ICountBasedAnalysisResult> analysisObserver,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            return source.AnalyzeCountWith(analysisObserver, 0, scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// analysisObserver</exception>
        public static IObservable<TSource> AnalyzeCountWith<TSource>(
            this IObservable<TSource> source,
            IObserver<ICountBasedAnalysisResult> analysisObserver,
            long initialCount,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            var analyzer = new CountAnalyzer<TSource>(initialCount);
            return source.AnalyzeWith<TSource, ICountBasedAnalysisResult>(analyzer, analysisObserver, scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the overall throughput
        /// since the time of subscription of every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <param name="stopWatchProvider">The stop watch provider. If none is provided, the <paramref name="scheduler"/> will be used and if no scheduler is provided, the <see cref="Scheduler.Default"/> will be attempted to be used.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IObservable<IThroughputAnalysisResult> AnalyzeOverallThroughput<TSource>(
            this IObservable<TSource> source,
            IStopwatchProvider stopWatchProvider,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (stopWatchProvider == null) throw new ArgumentNullException(nameof(stopWatchProvider));

            return source
                .Analyze<TSource, OverallThroughputAnalyzer<TSource>, IThroughputAnalysisResult>(
                    new OverallThroughputAnalyzer<TSource>(stopWatchProvider, false),
                    throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                    scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the overall throughput
        /// since the time of subscription of every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IObservable<IThroughputAnalysisResult> AnalyzeOverallThroughput<TSource>(
            this IObservable<TSource> source,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var stopWatchProvider = (scheduler ?? Scheduler.Default).AsStopwatchProvider();

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
                .AnalyzeOverallThroughput(stopWatchProvider, scheduler);
        }
        
        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the overall throughput
        /// since the time of subscription of every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="stopWatchProvider">The stop watch provider.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentException">scheduler - stopWatchProvider
        /// or
        /// IStopwatchProvider - stopWatchProvider</exception>
        public static IObservable<TSource> AnalyzeOverallThroughputWith<TSource>(
            this IObservable<TSource> source,
            IObserver<IThroughputAnalysisResult> analysisObserver,
            IStopwatchProvider stopWatchProvider,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));
            if (stopWatchProvider == null) throw new ArgumentNullException(nameof(stopWatchProvider));
            
            var analyzer = new OverallThroughputAnalyzer<TSource>(stopWatchProvider, false);

            return source.AnalyzeWith(
                analyzer,
                analysisObserver,
                throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the overall throughput
        /// since the time of subscription of every received <typeparamref name="TSource" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="scheduler">The scheduler to run the analyzer on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentException">scheduler - stopWatchProvider
        /// or
        /// IStopwatchProvider - stopWatchProvider</exception>
        public static IObservable<TSource> AnalyzeOverallThroughputWith<TSource>(
            this IObservable<TSource> source,
            IObserver<IThroughputAnalysisResult> analysisObserver,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));

            var stopWatchProvider = (scheduler ?? Scheduler.Default).AsStopwatchProvider();

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

            return source.AnalyzeOverallThroughputWith(analysisObserver, stopWatchProvider, scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the throughput of <typeparamref name="TSource" /> instances
        /// at a given timespan <paramref name="resolution" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="resolution">The resolution at which to sample the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <returns>
        /// A sequence of <see cref="IThroughputAnalysisResult" /> instances at the provided <paramref name="resolution" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">resolution - Must be at least 2 Ticks</exception>
        public static IObservable<IThroughputAnalysisResult> AnalyzeThroughput<TSource>(
            this IObservable<TSource> source,
            TimeSpan resolution,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (resolution.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(resolution));

            return source
                .Analyze<TSource, ThroughputAnalyzer<TSource>, IThroughputAnalysisResult>(
                    new ThroughputAnalyzer<TSource>(resolution, false, scheduler),
                    throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                    scheduler);
        }

        /// <summary>
        /// Provides an observable stream of <see cref="IThroughputAnalysisResult" /> elements reporting the throughput of <typeparamref name="TSource" /> instances
        /// at a given timespan <paramref name="resolution" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisObserver">The analysis observer.</param>
        /// <param name="resolution">The resolution at which to sample the <paramref name="source" /> sequence.</param>
        /// <param name="scheduler">The scheduler to run the <see cref="IAnalyzer{TSource}" /> on.</param>
        /// <returns>
        /// A sequence of <see cref="IThroughputAnalysisResult" /> instances at the provided <paramref name="resolution" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">resolution - Must be at least 2 Ticks</exception>
        public static IObservable<TSource> AnalyzeThroughputWith<TSource>(
            this IObservable<TSource> source,
            IObserver<IThroughputAnalysisResult> analysisObserver,
            TimeSpan resolution,
            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisObserver == null) throw new ArgumentNullException(nameof(analysisObserver));
            if (resolution.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(resolution));

            var analyzer = new ThroughputAnalyzer<TSource>(resolution, false, scheduler);

            return source.AnalyzeWith(
                analyzer,
                analysisObserver,
                throughputAnalyzer => { throughputAnalyzer.StartTimer(); },
                scheduler);
        }
    }
}