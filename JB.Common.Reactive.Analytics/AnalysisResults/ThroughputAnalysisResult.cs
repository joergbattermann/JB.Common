using System;

namespace JB.Reactive.Analytics.AnalysisResults
{
    public class ThroughputAnalysisResult : ElapsedTimeAnalysisResult, IThroughputAnalysisResult
    {
        /// <summary>
        /// Gets the total amount / count of items observed.
        /// </summary>
        /// <value>
        /// The total amount / count of items observed.
        /// </value>
        public long Count { get; }

        /// <summary>
        /// Gets the throughput / millisecond.
        /// </summary>
        /// <value>
        /// The throughput / millisecond.
        /// </value>
        public double ThroughputPerMillisecond => Count == 0 ? 0 : Count / ElapsedTime.TotalMilliseconds;

        /// <summary>
        /// Gets the throughput / second.
        /// </summary>
        /// <value>
        /// The throughput / second.
        /// </value>
        public double ThroughputPerSecond => Count == 0 ? 0 : Count / ElapsedTime.TotalSeconds;

        /// <summary>
        /// Gets the throughput / minute.
        /// </summary>
        /// <value>
        /// The throughput / minute.
        /// </value>
        public double ThroughputPerMinute => Count == 0 ? 0 : Count / ElapsedTime.TotalMinutes;

        /// <summary>
        /// Gets the throughput / hour.
        /// </summary>
        /// <value>
        /// The throughput / hour.
        /// </value>
        public double ThroughputPerHour => Count == 0 ? 0 : Count / ElapsedTime.TotalHours;

        /// <summary>
        /// Gets the throughput / day.
        /// </summary>
        /// <value>
        /// The throughput / day.
        /// </value>
        public double ThroughputPerDay => Count == 0 ? 0 : Count / ElapsedTime.TotalDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThroughputAnalysisResult"/> class.
        /// </summary>
        /// <param name="count">The total amount / count of items observed.</param>
        /// <param name="elapsedTime">The elapsed time.</param>
        public ThroughputAnalysisResult(long count, TimeSpan elapsedTime)
            :base(elapsedTime)
        {
            if(count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Count = count;
        }
    }
}