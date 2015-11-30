// -----------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.Analyzers;
using JB.Reactive.Analytics.Providers;

namespace JB.Reactive.Analytics.ExtensionMethods
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Takes the source sequence and attaches the <paramref name="analyticsProvider" /> to it while forwarding the source sequence
        /// back to the caller, the provided actions will be invoked whenever the analytics provider produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyticsProvider">The analytics provider to use.</param>
        /// <param name="analysisResultsObserver">The analysis results observer.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source,
                    IAnalyticsProvider<TSource> analyticsProvider,
                    IObserver<IAnalysisResult> analysisResultsObserver,
                    IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyticsProvider == null) throw new ArgumentNullException(nameof(analyticsProvider));
            if (analysisResultsObserver == null) throw new ArgumentNullException(nameof(analysisResultsObserver));

            return Observable.Create<TSource>(observer =>
            {
                // first we wire up the analytics provider with the actual source sequence as input sequence
                var sourceAnalyticsProviderSubscription = source.Subscribe(analyticsProvider);

                // then we wire up our to-be-returned observable with the analytics provider's output sequence
                var sourceSequenceForwardingSubscription = analyticsProvider.Subscribe(observer);

                // and finally we wire up the analytics provider's analysis results with the provided observer
                var analysisResultsSubscription = analyticsProvider.AnalysisResults.Subscribe(
                    scheduler != null
                        ? analysisResultsObserver.NotifyOn(scheduler)
                        : analysisResultsObserver);

                return () => new CompositeDisposable(sourceAnalyticsProviderSubscription, analysisResultsSubscription, sourceSequenceForwardingSubscription).Dispose();
            })
            .Publish()
            .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches the <paramref name="analyticsProvider"/> to it while forwarding the source sequence
        /// back to the caller, the provided actions will be invoked whenever the analytics provider produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyticsProvider">The analytics provider to use.</param>
        /// <param name="onNextAnalysisResult">The action to invoke whenever the <paramref name="analyticsProvider"/> produced an <see cref="IAnalysisResult"/>.</param>
        /// <param name="onErrorOnAnalysisResultSequence">The action to invoke if the <paramref name="analyticsProvider"/> reports an error.</param>
        /// <param name="onCompleteOnAnalysisResultSequence">The action to invoke whenever the <paramref name="analyticsProvider"/> signaled completion of its analysis sequence.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>A new <see cref="IObservable{TSource}"/> providing the full <paramref name="source"/> sequence back to the caller.</returns>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source,
                    IAnalyticsProvider<TSource> analyticsProvider,
                    Action<IAnalysisResult> onNextAnalysisResult = null,
                    Action<Exception> onErrorOnAnalysisResultSequence = null,
                    Action onCompleteOnAnalysisResultSequence = null,
                    IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyticsProvider == null) throw new ArgumentNullException(nameof(analyticsProvider));

            var analysisResultsObserver = Observer.Create<IAnalysisResult>(analysisResult =>
            {
                onNextAnalysisResult?.Invoke(analysisResult);
            },
            exception =>
            {
                onErrorOnAnalysisResultSequence?.Invoke(exception);
            },
            () =>
            {
                onCompleteOnAnalysisResultSequence?.Invoke();
            });

            return source.AnalyzeWith(analyticsProvider, analysisResultsObserver, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches the <paramref name="analyticsProvider" /> to it while forwarding the source sequence
        /// back to the caller, the provided actions will be invoked whenever the analytics provider produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The filter of <see cref="IAnalysisResult" /> types to consider from the
        /// <paramref name="analyticsProvider" /><see cref="IAnalysisResult" /> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyticsProvider">The analytics provider to use.</param>
        /// <param name="analysisResultsObserver">The analysis results observer.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source,
            IAnalyticsProvider<TSource> analyticsProvider,
            IObserver<TAnalysisResult> analysisResultsObserver,
            IScheduler scheduler = null)
            where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyticsProvider == null) throw new ArgumentNullException(nameof(analyticsProvider));
            if (analysisResultsObserver == null) throw new ArgumentNullException(nameof(analysisResultsObserver));

            return Observable.Create<TSource>(observer =>
            {
                // first we wire up the analytics provider with the actual source sequence as input sequence
                var sourceAnalyticsProviderSubscription = source.Subscribe(analyticsProvider);

                // then we wire up our to-be-returned observable with the analytics provider's output sequence
                var sourceSequenceForwardingSubscription = analyticsProvider.Subscribe(observer);

                // and finally we wire up the analytics provider's analysis results with the provided action to perform on every .OnNext() call
                var analysisResultsSubscription = analyticsProvider.AnalysisResults.OfType<TAnalysisResult>().Subscribe(
                    scheduler != null
                        ? analysisResultsObserver.NotifyOn(scheduler)
                        : analysisResultsObserver);

                return () => new CompositeDisposable(sourceAnalyticsProviderSubscription, analysisResultsSubscription, sourceSequenceForwardingSubscription).Dispose();
            })
            .Publish()
            .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches the <paramref name="analyticsProvider"/> to it while forwarding the source sequence
        /// back to the caller, the provided actions will be invoked whenever the analytics provider produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The filter of <see cref="IAnalysisResult"/> types to consider from the
        /// <paramref name="analyticsProvider" /> <see cref="IAnalysisResult"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyticsProvider">The analytics provider to use.</param>
        /// <param name="onNextAnalysisResult">The action to invoke whenever the <paramref name="analyticsProvider" /> produced an <see cref="IAnalysisResult" />.</param>
        /// <param name="onErrorOnAnalysisResultSequence">The action to invoke if the <paramref name="analyticsProvider" /> reports an error.</param>
        /// <param name="onCompleteOnAnalysisResultSequence">The action to invoke whenever the <paramref name="analyticsProvider" /> signaled completion of its analysis sequence.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, IAnalyticsProvider<TSource> analyticsProvider,
                    Action<TAnalysisResult> onNextAnalysisResult = null, Action<Exception> onErrorOnAnalysisResultSequence = null, Action onCompleteOnAnalysisResultSequence = null,
                    IScheduler scheduler = null)
                    where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyticsProvider == null) throw new ArgumentNullException(nameof(analyticsProvider));

            var analysisResultsObserver = Observer.Create<TAnalysisResult>(analysisResult =>
            {
                onNextAnalysisResult?.Invoke(analysisResult);
            },
            exception =>
            {
                onErrorOnAnalysisResultSequence?.Invoke(exception);
            },
            () =>
            {
                onCompleteOnAnalysisResultSequence?.Invoke();
            });

            return source
                .AnalyzeWith(analyticsProvider, analysisResultsObserver, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches a new <see cref="IAnalyticsProvider{TSource}" /> for the provided <paramref name="analyzers" /> to it
        /// while forwarding the source sequence back to the caller, the provided actions will be invoked whenever the analytics provider
        /// produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisResultsObserver">The analysis results observer.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, IEnumerable<IAnalyzer<TSource>> analyzers,
                    IObserver<IAnalysisResult> analysisResultsObserver,
                    IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analysisResultsObserver == null) throw new ArgumentNullException(nameof(analysisResultsObserver));

            return source
                .AnalyzeWith(new AnalyticsProvider<TSource>(analyzers, scheduler), analysisResultsObserver, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches a new <see cref="IAnalyticsProvider{TSource}"/> for the provided <paramref name="analyzers"/> to it
        /// while forwarding the source sequence back to the caller, the provided actions will be invoked whenever the analytics provider
        /// produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="onNextAnalysisResult">The action to invoke whenever the <paramref name="analyzers"/> produce an <see cref="IAnalysisResult"/>.</param>
        /// <param name="onErrorOnAnalysisResultSequence">The action to invoke if one of the <paramref name="analyzers"/> reports an error.</param>
        /// <param name="onCompleteOnAnalysisResultSequence">The action to invoke whenever the internally used <see cref="IAnalyticsProvider{TSource}"/> signaled
        /// completion of its analysis sequence.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>A new <see cref="IObservable{TSource}"/> providing the full <paramref name="source"/> sequence back to the caller.</returns>
        public static IObservable<TSource> AnalyzeWith<TSource>(this IObservable<TSource> source, IEnumerable<IAnalyzer<TSource>> analyzers,
                    Action<IAnalysisResult> onNextAnalysisResult = null, Action<Exception> onErrorOnAnalysisResultSequence = null, Action onCompleteOnAnalysisResultSequence = null,
                    IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));

            return source
                .AnalyzeWith(new AnalyticsProvider<TSource>(analyzers, scheduler), onNextAnalysisResult, onErrorOnAnalysisResultSequence, onCompleteOnAnalysisResultSequence, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches a new <see cref="IAnalyticsProvider{TSource}" /> for the provided <paramref name="analyzers" /> to it
        /// while forwarding the source sequence back to the caller, the provided actions will be invoked whenever the analytics provider
        /// produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The filter of <see cref="IAnalysisResult" /> types to consider from the
        /// <paramref name="analyzers" /><see cref="IAnalysisResult" /> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="analysisResultsObserver">The analysis results observer.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult" /> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, IEnumerable<IAnalyzer<TSource>> analyzers,
                    IObserver<TAnalysisResult> analysisResultsObserver,
                    IScheduler scheduler = null)
                    where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
            if (analysisResultsObserver == null) throw new ArgumentNullException(nameof(analysisResultsObserver));

            return source.AnalyzeWith(new AnalyticsProvider<TSource>(analyzers, scheduler), analysisResultsObserver, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Takes the source sequence and attaches a new <see cref="IAnalyticsProvider{TSource}" /> for the provided <paramref name="analyzers" /> to it
        /// while forwarding the source sequence back to the caller, the provided actions will be invoked whenever the analytics provider
        /// produces a new <see cref="IAnalysisResult" /> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAnalysisResult">The filter of <see cref="IAnalysisResult"/> types to consider from the
        /// <paramref name="analyzers" /> <see cref="IAnalysisResult"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <param name="onNextAnalysisResult">The action to invoke whenever the <paramref name="analyzers" /> produce an <see cref="IAnalysisResult" />.</param>
        /// <param name="onErrorOnAnalysisResultSequence">The action to invoke if one of the <paramref name="analyzers" /> reports an error.</param>
        /// <param name="onCompleteOnAnalysisResultSequence">The action to invoke whenever the internally used <see cref="IAnalyticsProvider{TSource}" /> signaled
        /// completion of its analysis sequence.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeWith<TSource, TAnalysisResult>(this IObservable<TSource> source, IEnumerable<IAnalyzer<TSource>> analyzers,
                    Action<TAnalysisResult> onNextAnalysisResult = null, Action<Exception> onErrorOnAnalysisResultSequence = null, Action onCompleteOnAnalysisResultSequence = null,
                    IScheduler scheduler = null)
                    where TAnalysisResult : IAnalysisResult
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));

            return source
                .AnalyzeWith(new AnalyticsProvider<TSource>(analyzers, scheduler), onNextAnalysisResult, onErrorOnAnalysisResultSequence, onCompleteOnAnalysisResultSequence, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult"/> elements reporting the current count
        /// for every received <typeparamref name="TSource"/> instance. If a <paramref name="predicate"/> is provided, the count
        /// will only be increased if the test returns [true].
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="analysisResultsObserver">The analysis results observer.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="predicate">A function to test each element whether or not to increase the count. If none is provided,
        /// the count will be increased with every <typeparamref name="TSource"/> element reported.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeCount<TSource>(this IObservable<TSource> source,
                            IObserver<ICountBasedAnalysisResult> analysisResultsObserver,
                            long initialCount = 0,
                            Func<TSource, bool> predicate = null,
                            IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (analysisResultsObserver == null) throw new ArgumentNullException(nameof(analysisResultsObserver));

            return source
                .AnalyzeWith(new[] { new CountAnalyzer<TSource>(initialCount, predicate, scheduler) }, analysisResultsObserver, scheduler)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// Provides an observable stream of <see cref="ICountBasedAnalysisResult" /> elements reporting the current count
        /// for every received <typeparamref name="TSource" /> instance. If a <paramref name="predicate" /> is provided, the count
        /// will only be increased if the test returns [true].
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="predicate">A function to test each element whether or not to increase the count. If none is provided,
        /// the count will be increased with every <typeparamref name="TSource"/> element reported.</param>
        /// <param name="onNextAnalysisResult">The action to invoke whenever the count has increased.</param>
        /// <param name="onErrorOnAnalysisResultSequence">The action to invoke if one the analyzer reports an error.</param>
        /// <param name="onCompleteOnAnalysisResultSequence">The action to invoke whenever the internally used <see cref="IAnalyticsProvider{TSource}" /> signaled
        /// completion of its analysis sequence.</param>
        /// <param name="scheduler">The scheduler to invoke the <see cref="IAnalysisResult"/> notifications on.</param>
        /// <returns>
        /// A new <see cref="IObservable{TSource}" /> providing the full <paramref name="source" /> sequence back to the caller.
        /// </returns>
        public static IObservable<TSource> AnalyzeCount<TSource>(this IObservable<TSource> source,
                    long initialCount = 0,
                    Func<TSource, bool> predicate = null,
                    Action<ICountBasedAnalysisResult> onNextAnalysisResult = null, Action<Exception> onErrorOnAnalysisResultSequence = null, Action onCompleteOnAnalysisResultSequence = null,
                    IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.AnalyzeWith(new[] { new CountAnalyzer<TSource>(initialCount, predicate, scheduler) },
                onNextAnalysisResult,
                onErrorOnAnalysisResultSequence,
                onCompleteOnAnalysisResultSequence,
                scheduler)
                .Publish()
                .RefCount();
        }
    }
}