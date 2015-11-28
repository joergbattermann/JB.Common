using System;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public abstract class Analyzer<TSource, TAnalysisResult> : Analyzer<TSource>, IAnalyzer<TSource, TAnalysisResult>, IDisposable
        where TAnalysisResult : IAnalysisResult
    {
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
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
        /// Abstract base class for <see cref="IAnalyzer{TSource}"/> implementations
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        public abstract class Analyzer<TSource> : IAnalyzer<TSource>, IDisposable
    {
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

        #region Implementation of IObservable<out IAnalysisResult>

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public virtual IDisposable Subscribe(IObserver<IAnalysisResult> observer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}