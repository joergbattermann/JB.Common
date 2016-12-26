using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class ThroughputAnalyzer<TSource> : Analyzer<TSource, IThroughputAnalysisResult>
    {
        private long _currentCount;

        /// <summary>
        /// Gets or sets the interval subscription.
        /// </summary>
        /// <value>
        /// The interval subscription.
        /// </value>
        private IDisposable IntervalSubscription { get; set; }

        /// <summary>
        /// Gets the scheduler used for the <see cref="IntervalSubscription"/>.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public IScheduler Scheduler { get; }

        /// <summary>
        /// Gets the resolution of the internally used timer interval.
        /// </summary>
        /// <value>
        /// The resolution.
        /// </value>
        private TimeSpan Resolution { get; }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public long CurrentCount
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return Interlocked.Read(ref _currentCount);
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

                return !IsDisposing && !IsDisposed && IntervalSubscription != null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThroughputAnalyzer{TSource}" /> class.
        /// </summary>
        /// <param name="resolution">The resolution at which to emit the throughput rate.</param>
        /// <param name="startTimerImmediately">Indicates whether the underlying timer shall start immediately upon construction.</param>
        /// <param name="scheduler">The scheduler to run the internal timer on.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">resolution - Must be at least 2 Ticks or more</exception>
        public ThroughputAnalyzer(TimeSpan resolution, bool startTimerImmediately = true, IScheduler scheduler = null)
        {
            if (resolution.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(resolution));

            Resolution = resolution;
            Scheduler = scheduler;
            
            if (startTimerImmediately)
                StartTimer();
        }

        /// <summary>
        /// Starts the underlying timer.
        /// </summary>
        public void StartTimer()
        {
            if (IsRunning)
                throw new InvalidOperationException("The Timer is already running and can only be started once.");

            // using an Observable.Interval subscription as timer
            IntervalSubscription = (Scheduler != null ? Observable.Interval(Resolution, Scheduler) : Observable.Interval(Resolution))
                .Subscribe(_ =>
                {
                    // and on every interval we capture and reset the counter
                    var currentCountBeforeReset = Interlocked.Exchange(ref _currentCount, 0);

                    // and expose its current value
                    AnalysisResultsSubject.OnNext(new ThroughputAnalysisResult(currentCountBeforeReset, Resolution));
                });
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
                Interlocked.Increment(ref _currentCount);
            }
        }
        
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                IntervalSubscription?.Dispose();
            }

            base.Dispose(disposeManagedResources);
        }

        #endregion
    }
}