using System.Collections.Generic;

namespace JB.Linq
{
    /// <summary>
    /// Helper methods for and around <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class Enumerable
    {

        /// <summary>
        /// Returns the specified value as an <see cref="IEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the <paramref name="value"/>.</typeparam>
        /// <param name="value">The value to return as an enumerable.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> From<TSource>(TSource value)
        {
            yield return value;
        }
    }
}