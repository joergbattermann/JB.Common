// -----------------------------------------------------------------------
// <copyright file="AnalyticsProvider.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
        private readonly object _analyzersLocker = new object();

        /// <summary>
        /// Gets the analyzers.
        /// </summary>
        /// <returns></returns>
        protected IReadOnlyCollection<IAnalyzer<TSource>> Analyzers
        {
            get
            {
                CheckForAndThrowIfDisposed();

                lock (_analyzersLocker)
                {
                    return _analyzers.ToArray();
                }
            }
        }

        protected virtual IObserver<TSource> SourceValuesForwardingNotifier
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _sourceValuesForwarderSubject;
            }
        }

        protected virtual IObservable<TSource> SourceValuesObservable
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _sourceValuesForwarderSubject;
            }
        }

        /// <summary>
        /// Gets the source sequence subject.
        /// </summary>
        /// <value>
        /// The source sequence subject.
        /// </value>
        protected virtual Subject<TSource> SourceValuesForwarderSubject
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _sourceValuesForwarderSubject;
            }
            private set
            {
                CheckForAndThrowIfDisposed();

                _sourceValuesForwarderSubject = value;
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
        /// <param name="analyzers">The analyzers to use.</param>
        public AnalyticsProvider(IEnumerable<IAnalyzer<TSource>> analyzers)
        {
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));

            _analyzers = new List<IAnalyzer<TSource>>(analyzers);

            SourceValuesForwarderSubject = new Subject<TSource>();
            AnalysisResultsSubject = new Subject<IAnalysisResult>();

            SetupAnalyzerSubscriptions();
        }

        /// <summary>
        /// Makes sure the <see cref="Analyzers"/> result sequence is forwarded.
        /// </summary>
        private void SetupAnalyzerSubscriptions()
        {
            _analyzersSubscription = new CompositeDisposable(Analyzers.Select(SubscribeToAnalyzer).ToList());
        }

        /// <summary>
        /// Subscribes to the given analyzer.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private IDisposable SubscribeToAnalyzer(IAnalyzer<TSource> analyzer)
        {
            if (analyzer == null) throw new ArgumentNullException(nameof(analyzer));

            return analyzer.Subscribe(analysisResult =>
            {
                AnalysisResultsSubject.OnNext(analysisResult);
            },
            exception =>
            {
                AnalysisResultsSubject.OnError(exception);
            },
            () =>
            {
                // remove completed analyzers from underlying list
                lock (_analyzersLocker)
                {
                    _analyzers.Remove(analyzer);
                }
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsProvider{TSource}"/> class.
        /// </summary>
        /// <param name="analyzers">The analyzers to use.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public AnalyticsProvider(params IAnalyzer<TSource>[] analyzers)
            : this(analyzers.ToList())
        {
            if (analyzers == null) throw new ArgumentNullException(nameof(analyzers));
        }

        #region Implementation of IObserver<in TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public virtual void OnNext(TSource value)
        {
            foreach (var analyzer in Analyzers)
            {
                ToDo - hand over / in Scheduler and make sure all subjects are logically split between observers and observables
                analyzer.NotifyOadgdagn() .OnNext(value);
            }

            SourceValuesForwardingNotifier.OnNext(value);
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public virtual void OnError(Exception error)
        {
            SourceValuesForwardingNotifier.OnError(error);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public virtual void OnCompleted()
        {
            SourceValuesForwardingNotifier.OnCompleted();
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

        #endregion

        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();
        private Subject<TSource> _sourceValuesForwarderSubject;
        private Subject<IAnalysisResult> _analysisResultsSubject;
        private IDisposable _analyzersSubscription;

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
                    if (_analyzersSubscription != null)
                    {
                        _analyzersSubscription.Dispose();
                        _analyzersSubscription = null;
                    }

                    if (_sourceValuesForwarderSubject != null)
                    {
                        _sourceValuesForwarderSubject.Dispose();
                        _sourceValuesForwarderSubject = null;
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

            return SourceValuesForwarderSubject.Subscribe(observer);
        }

        #endregion
    }
}