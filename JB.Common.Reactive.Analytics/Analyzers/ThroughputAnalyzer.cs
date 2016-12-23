using System;
using System.Diagnostics;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class ThroughputAnalyzer<TSource> : Analyzer<TSource, ThrougputAnalysisResult>
    {
        private long _totalCount;

        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        /// <value>
        /// The elapsed time.
        /// </value>
        public TimeSpan ElapsedTime
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public long TotalCount
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return Interlocked.Read(ref _totalCount);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the unerlyind timer has been started and is running.
        /// </summary>
        /// <value>
        /// A value indicating whether the unerlyind timer has been started and is running.
        /// </value>
        public bool IsRunning
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _stopwatch.IsRunning;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountAnalyzer{TSource}" /> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="startTimerImmediately">Indicates whether the underlying timer shall start immediately upon construction.</param>
        public ThroughputAnalyzer(long initialCount = 0, bool startTimerImmediately = true)
        {
            _totalCount = initialCount;
            _stopwatch = new Stopwatch();

            if(startTimerImmediately)
                StartTimer();
        }

        /// <summary>
        /// Starts the underlying timer.
        /// </summary>
        public void StartTimer()
        {
            if(IsRunning)
                throw new InvalidOperationException("The Timer is already running and can only be started once.");

            _stopwatch.Start();
        }
        
        #region Overrides of Analyzer<TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public override void OnNext(TSource value)
        {
            if (IsRunning)
            {
                var totalCount = Interlocked.Increment(ref _totalCount);

                AnalysisResultsSubject.OnNext(new ThrougputAnalysisResult(totalCount, ElapsedTime));
            }
        }

        #endregion
    }
}