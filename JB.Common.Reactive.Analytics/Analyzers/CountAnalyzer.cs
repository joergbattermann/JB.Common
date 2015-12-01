// -----------------------------------------------------------------------
// <copyright file="CountAnalyzer.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive.Concurrency;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class CountAnalyzer<TSource> : Analyzer<TSource>
    {
        private long _currentCount;
        private readonly Func<TSource, bool> _predicate;

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
        /// Initializes a new instance of the <see cref="CountAnalyzer{TSource}" /> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="predicate">The test to perform for each received <typeparamref name="TSource" /> instance whether or not to increase the count.
        /// If none is provided, all instances are counted.</param>
        /// <param name="scheduler">The scheduler to schedule notifications on, if any.</param>
        public CountAnalyzer(long initialCount = 0, Func<TSource, bool> predicate = null, IScheduler scheduler = null)
            : base(scheduler)
        {
            _currentCount = initialCount;
            _predicate = predicate ?? (source => true);
        }

        #region Overrides of Analyzer<TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public override void OnNext(TSource value)
        {
            if (_predicate.Invoke(value))
            {
                var currentCount = Interlocked.Increment(ref _currentCount);

                AnalysisResultsSubject.OnNext(new CountAnalysisResult(currentCount));
            }
        }

        #endregion
    }
}