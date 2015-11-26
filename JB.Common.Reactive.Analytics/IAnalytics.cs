using System;

namespace JB.Reactive.Analytics
{
    /// <summary>
    /// Provides a hook into a given <typeparamref name="TSource">source sequence</typeparamref>
    /// of values and provides analysis results for those depending on its registered <see cref="IAnalyzer{TSource}">analyzers</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    public interface IAnalytics<TSource> : IObservable<TSource>, IObservable<IAnalysisResult<TSource>>
    {
        /// <summary>
        /// Gets the sequence of performed analyses.
        /// </summary>
        /// <value>
        /// The analyses performed as an observable sequence.
        /// </value>
        IObservable<IAnalysisResult<TSource>> Analyses { get; }

        /// <summary>
        /// Registers an analyzer with this instance that will be used in all future instance of the <see cref="Source"/> sequence.
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