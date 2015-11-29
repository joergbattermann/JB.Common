using System.Reactive.Subjects;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    /// <summary>
    /// An analyzer acts as an observer for an input sequence and provides its
    /// corresponding analysis results as an observable sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface IAnalyzer<in TSource> : ISubject<TSource, IAnalysisResult>
    {
    }
}