using System;
using System.Diagnostics;

namespace JB.Reactive.Analytics.AnalysisResults
{
    [DebuggerDisplay("{" + nameof(TotalCount) + "}/{" + nameof(ElapsedTime) + "}")]
    public class ThrougputAnalysisResult : ElapsedTimeAnalysisResult
    {
        /// <summary>
        /// Gets the total amount / count of items observed.
        /// </summary>
        /// <value>
        /// The total amount / count of items observed.
        /// </value>
        public long TotalCount { get; }

        /// <summary>
        /// Gets the throughput / millisecond.
        /// </summary>
        /// <value>
        /// The throughput / millisecond.
        /// </value>
        public double ThroughputPerMillisecond => TotalCount / ElapsedTime.TotalMilliseconds;

        /// <summary>
        /// Gets the throughput / second.
        /// </summary>
        /// <value>
        /// The throughput / second.
        /// </value>
        public double ThroughputPerSecond => TotalCount / ElapsedTime.TotalSeconds;

        /// <summary>
        /// Gets the throughput / minute.
        /// </summary>
        /// <value>
        /// The throughput / minute.
        /// </value>
        public double ThroughputPerMinute => TotalCount / ElapsedTime.TotalMinutes;

        /// <summary>
        /// Gets the throughput / hour.
        /// </summary>
        /// <value>
        /// The throughput / hour.
        /// </value>
        public double ThroughputPerHour => TotalCount / ElapsedTime.TotalHours;

        /// <summary>
        /// Gets the throughput / day.
        /// </summary>
        /// <value>
        /// The throughput / day.
        /// </value>
        public double ThroughputPerDay => TotalCount / ElapsedTime.TotalDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrougputAnalysisResult"/> class.
        /// </summary>
        /// <param name="totalCount">The total amount / count of items observed.</param>
        /// <param name="elapsedTime">The elapsed time.</param>
        public ThrougputAnalysisResult(long totalCount, TimeSpan elapsedTime)
            :base(elapsedTime)
        {
            if(totalCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCount));

            TotalCount = totalCount;
        }
    }
}