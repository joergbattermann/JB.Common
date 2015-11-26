using System;

namespace JB.Reactive.Analytics
{
    /// <summary>
    /// Base interface that allows <see cref="IAnalyzer{TSource}"/>s to be registered for
    /// analysis of <typeparamref name="TSource"/> elements and produces a corresponding output
    /// sequence of <see cref="IAnalysisResult{TSource}"/> instances.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface IAnalyticsProvider<TSource> : IObservable<IAnalysisResult<TSource>>
    {
        /// <summary>
        /// Registers an analyzer with this instance that will be used in all future instance of the <typeparam name="TSource"/> sequence.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        void RegisterAnalyzer(IAnalyzer<TSource> analyzer);

        /// <summary>
        /// De-registers the analyzer, does not affect currently ongoing analyses.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        void DeregisterAnalyzer(IAnalyzer<TSource> analyzer);
    }
}