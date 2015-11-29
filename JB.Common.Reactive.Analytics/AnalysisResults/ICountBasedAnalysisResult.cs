namespace JB.Reactive.Analytics.AnalysisResults
{
    /// <summary>
    /// An analysis result based on a number of elements counted.
    /// </summary>
    public interface ICountBasedAnalysisResult : IAnalysisResult
    {
        /// <summary>
        /// Gets the elements count.
        /// </summary>
        /// <value>
        /// The elements count.
        /// </value>
        long Count { get; }
    }
}