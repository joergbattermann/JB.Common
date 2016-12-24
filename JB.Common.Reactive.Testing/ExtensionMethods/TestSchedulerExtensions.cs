using System;
using Microsoft.Reactive.Testing;

namespace JB.Reactive.Testing.ExtensionMethods
{
    /// <summary>
    /// Extension Methods for <see cref="TestScheduler"/> instances
    /// </summary>
    public static class TestSchedulerExtensions
    {
        /// <summary>
        /// Advances the scheduler's clock by the specified relative <paramref name="timeSpan" />, running all work scheduled for that timespan.
        /// </summary>
        /// <param name="testScheduler">The test scheduler.</param>
        /// <param name="timeSpan">Relative time to advance the scheduler's clock by.</param>
        /// <exception cref="System.ArgumentNullException">testScheduler</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeSpan" /> is negative.</exception>
        public static void AdvanceBy(this TestScheduler testScheduler, TimeSpan timeSpan)
        {
            if (testScheduler == null) throw new ArgumentNullException(nameof(testScheduler));
            
            testScheduler.AdvanceBy(timeSpan.Ticks);
        }

        /// <summary>
        /// Advances the scheduler's clock to the specified <paramref name="absoluteTimeSpan" />, running all work till that point.
        /// </summary>
        /// <param name="testScheduler">The test scheduler.</param>
        /// <param name="absoluteTimeSpan">Absolute time to advance the scheduler's clock to.</param>
        /// <exception cref="System.ArgumentNullException">testScheduler</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="absoluteTimeSpan" />, compared to the <paramref name="testScheduler"/>'s Clock, is in the past.</exception>
        public static void AdvanceTo(this TestScheduler testScheduler, TimeSpan absoluteTimeSpan)
        {
            if (testScheduler == null) throw new ArgumentNullException(nameof(testScheduler));

            testScheduler.AdvanceTo(absoluteTimeSpan.Ticks);
        }
    }
}