namespace JB.Reactive.Analytics.AnalysisResults
{
    /// <summary>
    /// An analysis result based on a single element.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface ISingleElementAnalysisResult<out TSource> : IAnalysisResult
    {
        /// <summary>
        /// Gets the element analyzed.
        /// </summary>
        /// <value>
        /// The element analyzed.
        /// </value>
        TSource ElementAnalyzed { get; }
    }
}