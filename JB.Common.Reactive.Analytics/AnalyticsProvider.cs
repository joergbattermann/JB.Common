// -----------------------------------------------------------------------
// <copyright file="AnalyticsProvider.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace JB.Reactive.Analytics
{
    /// <summary>
    /// Base class for <see cref="IAnalyticsProvider{TSource}"/>
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public abstract class AnalyticsProvider<TSource> : IAnalyticsProvider<TSource>
    {
        /// <summary>
        /// Gets the analyzers.
        /// </summary>
        /// <returns></returns>
        protected IReadOnlyCollection<IAnalyzer<TSource>> Analyzers
        {
            get
            {
                lock (_innerAnalyzersLock)
                {
                    return _analyzers.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the analyzers.
        /// </summary>
        /// <value>
        /// The analyzers.
        /// </value>
        private List<IAnalyzer<TSource>> _analyzers = new List<IAnalyzer<TSource>>();
        private readonly object _innerAnalyzersLock = new object();

        #region Implementation of IAnalyticsProvider<TSource>

        /// <summary>
        /// Registers an analyzer with this instance that will be used in all future instance of the <typeparam name="TSource"/> sequence.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        public virtual void RegisterAnalyzer(IAnalyzer<TSource> analyzer)
        {
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            lock (_innerAnalyzersLock)
            {
                _analyzers.Add(analyzer);
            }
        }

        /// <summary>
        /// De-registers the analyzer, does not affect currently ongoing analyses.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        public virtual void DeregisterAnalyzer(IAnalyzer<TSource> analyzer)
        {
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            lock (_innerAnalyzersLock)
            {
                _analyzers.Remove(analyzer);
            }
        }

        #endregion

        #region Implementation of IObservable<out IAnalysisResult<TSource>>

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public abstract IDisposable Subscribe(IObserver<IAnalysisResult<TSource>> observer);

        #endregion
    }
}