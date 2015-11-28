// -----------------------------------------------------------------------
// <copyright file="AnalyticsProvider.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;
using JB.Reactive.Analytics.Analyzers;

namespace JB.Reactive.Analytics.Providers
{
    /// <summary>
    /// Generic <see cref="IAnalyticsProvider{TSource}"/> implementation
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public class AnalyticsProvider<TSource> : IAnalyticsProvider<TSource>, IDisposable
    {
        /// <summary>
        /// Backing field for the <see cref="Analyzers"/> property.
        /// </summary>
        private readonly List<IAnalyzer<TSource>> _analyzers;

        /// <summary>
        /// The locker object for accessing and modifying the <see cref="_analyzers"/> field.
        /// </summary>
        private readonly object _innerAnalyzersLock = new object();

        /// <summary>
        /// Gets the analyzers.
        /// </summary>
        /// <returns></returns>
        protected IReadOnlyCollection<IAnalyzer<TSource>> Analyzers
        {
            get
            {
                CheckForAndThrowIfDisposed();

                lock (_innerAnalyzersLock)
                {
                    return _analyzers.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the source sequence subject.
        /// </summary>
        /// <value>
        /// The source sequence subject.
        /// </value>
        protected virtual Subject<TSource> SourceSequenceSubject
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _sourceSequenceSubject;
            }
            private set
            {
                CheckForAndThrowIfDisposed();

                _sourceSequenceSubject = value;
            }
        }

        /// <summary>
        /// Gets the analysis results subject.
        /// </summary>
        /// <value>
        /// The analysis results subject.
        /// </value>
        protected virtual Subject<IAnalysisResult> AnalysisResultsSubject
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _analysisResultsSubject;
            }
            private set
            {
                CheckForAndThrowIfDisposed();

                _analysisResultsSubject = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsProvider{TSource}"/> class.
        /// </summary>
        /// <param name="analyzers">The initial set of analyzers, if any.</param>
        protected AnalyticsProvider(IEnumerable<IAnalyzer<TSource>> analyzers = null)
        {
            _analyzers = analyzers != null
                ? new List<IAnalyzer<TSource>>(analyzers)
                : new List<IAnalyzer<TSource>>();

            AnalysisResultsSubject = new Subject<IAnalysisResult>();
            SourceSequenceSubject = new Subject<TSource>();
        }

        #region Implementation of IObserver<in TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public virtual void OnNext(TSource value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public virtual void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public virtual void OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IAnalyticsProvider<TSource>

        /// <summary>
        /// Gets the analysis results observable.
        /// </summary>
        /// <value>
        /// The analysis results.
        /// </value>
        public IObservable<IAnalysisResult> AnalysisResults
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return AnalysisResultsSubject;
            }
        }

        /// <summary>
        /// Registers an analyzer with this instance that will be used in all future instance of the <typeparam name="TSource"/> sequence.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        public virtual void RegisterAnalyzer(IAnalyzer<TSource> analyzer)
        {
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            CheckForAndThrowIfDisposed();

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

            CheckForAndThrowIfDisposed();

            lock (_innerAnalyzersLock)
            {
                _analyzers.Remove(analyzer);
            }
        }

        #endregion
        
        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();
        private Subject<IAnalysisResult> _analysisResultsSubject;
        private Subject<TSource> _sourceSequenceSubject;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref _isDisposed) == 1; }
            protected set
            {
                lock (_isDisposedLocker)
                {
                    if (value == false && IsDisposed)
                        throw new InvalidOperationException("Once Disposed has been set, it cannot be reset back to false.");

                    Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposing
        {
            get { return Interlocked.Read(ref _isDisposing) == 1; }
            protected set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
            }
        }
        
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                if (disposeManagedResources)
                {
                    if (_sourceSequenceSubject != null)
                    {
                        _sourceSequenceSubject.Dispose();
                        _sourceSequenceSubject = null;
                    }

                    if (_analysisResultsSubject != null)
                    {
                        _analysisResultsSubject.Dispose();
                        _analysisResultsSubject = null;
                    }
                }
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        ///     Checks whether this instance is currently or already has been disposed.
        /// </summary>
        protected virtual void CheckForAndThrowIfDisposed()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException(this.GetType().Name, "This instance is currently being disposed.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion

        #region Implementation of IObservable<out TSource>

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            CheckForAndThrowIfDisposed();

            return _sourceSequenceSubject.Subscribe(observer);
        }

        #endregion
    }
}