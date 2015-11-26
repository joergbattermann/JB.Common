using System;

namespace JB.Reactive.Analytics
{
    public interface IAnalyzer<TSource> : IObserver<TSource>, IObservable<IAnalysisResult<TSource>>
    {
    }
}