using System;

namespace JB.Reactive.Analytics
{
    /// <summary>
    /// An input-only receiving <see cref="IAnalyticsProvider{TSource}"/> for a
    /// <typeparamref name="TSource">source sequence</typeparamref> of values allowing
    /// the sequence to be analysed via its registered  <see cref="IAnalyzer{TSource}">analyzers</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    public interface IReceivingAnalyticsProvider<TSource> : IAnalyticsProvider<TSource>, IObserver<TSource>
    {
    }
}