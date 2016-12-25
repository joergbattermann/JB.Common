using System;
using System.Reactive.Concurrency;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class OverallThroughputAnalyzer<TSource> : Analyzer<TSource, IThroughputAnalysisResult>
    {
        private long _totalCount;
        
        private IStopwatch _stopwatch;

        /// <summary>
        /// Gets the stopwatch provider.
        /// </summary>
        /// <value>
        /// The stopwatch provider.
        /// </value>
        private IStopwatchProvider StopwatchProvider { get; }

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

                return !IsDisposing && !IsDisposed && _stopwatch != null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountAnalyzer{TSource}" /> class.
        /// </summary>
        /// <param name="stopwatchProvider">The stopwatch provider.</param>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="startTimerImmediately">Indicates whether the underlying timer shall start immediately upon construction.</param>
        /// <exception cref="System.ArgumentNullException">stopwatchProvider</exception>
        public OverallThroughputAnalyzer(IStopwatchProvider stopwatchProvider, long initialCount = 0, bool startTimerImmediately = true)
        {
            if (stopwatchProvider == null) throw new ArgumentNullException(nameof(stopwatchProvider));

            _totalCount = initialCount;
            StopwatchProvider = stopwatchProvider;
            
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

            _stopwatch = StopwatchProvider.StartStopwatch();
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

                AnalysisResultsSubject.OnNext(new ThroughputAnalysisResult(totalCount, ElapsedTime));
            }
        }
        
        #endregion
    }
}