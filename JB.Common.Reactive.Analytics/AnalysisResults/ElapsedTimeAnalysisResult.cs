using System;
using System.Diagnostics;

namespace JB.Reactive.Analytics.AnalysisResults
{
    [DebuggerDisplay("{" + nameof(ElapsedTime) + "}")]
    public class ElapsedTimeAnalysisResult : IElapsedTimeAnalysisResult
    {
        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        /// <value>
        /// The elapsed time.
        /// </value>
        public TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElapsedTimeAnalysisResult"/> class.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public ElapsedTimeAnalysisResult(TimeSpan elapsedTime)
        {
            if(elapsedTime < TimeSpan.MinValue)
                throw new ArgumentOutOfRangeException(nameof(elapsedTime));

            ElapsedTime = elapsedTime;
        }
    }
}