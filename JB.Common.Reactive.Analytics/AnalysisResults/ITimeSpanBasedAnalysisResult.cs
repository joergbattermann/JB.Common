using System;

namespace JB.Reactive.Analytics.AnalysisResults
{
    public interface ITimeSpanBasedAnalysisResult : IAnalysisResult
    {
        /// <summary>
        /// Gets the period analyzed.
        /// </summary>
        /// <value>
        /// The period analyzed.
        /// </value>
        TimeSpan PeriodAnalyzed { get; }
    }
}