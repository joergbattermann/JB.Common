using System;

namespace JB.Reactive.Analytics
{
    /// <summary>
    /// A forwarding <see cref="IAnalyticsProvider{TSource}"/> that taps into the source stream
    /// and, besides performing analysis over the stream's elements, forwards the stream as is to
    /// its own subscribers allowing a placement inside and a logical pipeline.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public interface IForwardingAnalyticsProvider<TSource> : IReceivingAnalyticsProvider<TSource>, IObservable<TSource>
    {
        
    }
}