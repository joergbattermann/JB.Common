using System;

namespace JB.Reactive.Analytics.AnalysisResults
{
    public interface IElapsedTimeAnalysisResult : IAnalysisResult
    {
        /// <summary>
        /// Gets the elapsed time since analysis was started.
        /// </summary>
        /// <value>
        /// The elapsed time since analysis was started.
        /// </value>
        TimeSpan ElapsedTime { get; }
    }
}