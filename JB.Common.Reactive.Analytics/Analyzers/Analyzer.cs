using System;
using System.Reactive.Subjects;
using System.Threading;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    /// <summary>
    /// Base class for <see cref="IAnalyzer{TSource}"/> implementations.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public abstract class Analyzer<TSource> : Analyzer<TSource, IAnalysisResult>
    {
    }

    /// <summary>
    /// Base class for <see cref="IAnalyzer{TSource}" /> implementations.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TAnalysisResult">The type of the analysis result.</typeparam>
    public abstract class Analyzer<TSource, TAnalysisResult> : IAnalyzer<TSource, TAnalysisResult>, IDisposable
        where TAnalysisResult : IAnalysisResult
    {
        private Subject<TAnalysisResult> _analysisResultSubject;

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyzer{TSource}" /> class.
        /// </summary>
        protected Analyzer()
        {
            _analysisResultSubject = new Subject<TAnalysisResult>();
        }
        
        /// <summary>
        /// Gets the analysis results subject.
        /// </summary>
        /// <value>
        /// The analysis results subject.
        /// </value>
        protected Subject<TAnalysisResult> AnalysisResultsSubject
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _analysisResultSubject;
            }
        }

        #region Implementation of IObserver<in TSource>

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public abstract void OnNext(TSource value);

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// By default this forwards the <paramref name="error"/> to the <see cref="AnalysisResultsSubject"/>.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public virtual void OnError(Exception error)
        {
            AnalysisResultsSubject.OnError(error);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// By default this forwards the completion to the <see cref="AnalysisResultsSubject"/>.
        /// </summary>
        public virtual void OnCompleted()
        {
            AnalysisResultsSubject.OnCompleted();
        }

        #endregion

        #region Implementation of IObservable<out TAnalysisResult>

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public virtual IDisposable Subscribe(IObserver<TAnalysisResult> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            return AnalysisResultsSubject.Subscribe(observer);
        }

        #endregion


        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();
        
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
                    if (_analysisResultSubject != null)
                    {
                        _analysisResultSubject.Dispose();
                        _analysisResultSubject = null;
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
    }
}