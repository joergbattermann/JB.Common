using System.Collections.Generic;

namespace JB.Reactive.Analytics.AnalysisResults
{
    /// <summary>
    /// An analysis result based on multiple elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface IMultipleElementsAnalysisResult<out TSource> : IAnalysisResult
    {
        /// <summary>
        /// Gets the elements analyzed.
        /// </summary>
        /// <value>
        /// The elements analyzed.
        /// </value>
        IReadOnlyCollection<TSource> ElementsAnalyzed { get; }
    }
}