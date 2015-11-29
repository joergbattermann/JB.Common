// -----------------------------------------------------------------------
// <copyright file="CountAnalyzer.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class CountAnalyzer<TSource> : Analyzer<TSource, ICountBasedAnalysisResult>
    {
        private long _currentCount;

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
        /// Initializes a new instance of the <see cref="CountAnalyzer{TSource}"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        public CountAnalyzer(long initialCount = 0)
        {
            _currentCount = initialCount;
        }

        #region Overrides of Analyzer<TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public override void OnNext(TSource value)
        {
            var currentCount = Interlocked.Increment(ref _currentCount);

            AnalysisResultSubject.OnNext(new CountAnalysisResult(currentCount));
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public override void OnError(Exception error)
        {
            // nothing to do here
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public override void OnCompleted()
        {
            // nothing to do here
        }

        #endregion
    }
}