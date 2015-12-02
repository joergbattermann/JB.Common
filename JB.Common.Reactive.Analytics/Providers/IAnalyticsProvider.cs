using System;
using System.Reactive.Subjects;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.Analyzers;

namespace JB.Reactive.Analytics.Providers
{
    /// <summary>
    /// Base interface that allows <see cref="IAnalyzer{TSource}"/>s to be registered for
    /// analysis of <typeparamref name="TSource"/> elements and produces a corresponding output
    /// sequence of <see cref="IAnalysisResult"/> instances.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface IAnalyticsProvider<in TSource> : IAnalyticsProvider<TSource, IAnalysisResult>
    {
    }

    public interface IAnalyticsProvider<in TSource, out TAnalysisResult> : ISubject<TSource, TAnalysisResult>
        where TAnalysisResult : IAnalysisResult
    {
    }
}