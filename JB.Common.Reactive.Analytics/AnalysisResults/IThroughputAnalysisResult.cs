namespace JB.Reactive.Analytics.AnalysisResults
{
    public interface IThroughputAnalysisResult : IElapsedTimeAnalysisResult, ICountBasedAnalysisResult
    {
        /// <summary>
        /// Gets the throughput / millisecond.
        /// </summary>
        /// <value>
        /// The throughput / millisecond.
        /// </value>
        double ThroughputPerMillisecond { get; }

        /// <summary>
        /// Gets the throughput / second.
        /// </summary>
        /// <value>
        /// The throughput / second.
        /// </value>
        double ThroughputPerSecond { get; }

        /// <summary>
        /// Gets the throughput / minute.
        /// </summary>
        /// <value>
        /// The throughput / minute.
        /// </value>
        double ThroughputPerMinute { get; }

        /// <summary>
        /// Gets the throughput / hour.
        /// </summary>
        /// <value>
        /// The throughput / hour.
        /// </value>
        double ThroughputPerHour { get; }

        /// <summary>
        /// Gets the throughput / day.
        /// </summary>
        /// <value>
        /// The throughput / day.
        /// </value>
        double ThroughputPerDay { get; }
    }
}