using System.Collections.Generic;

namespace JB.Reactive.Cache
{
    /// <summary>
    /// This is basically an internally used <see cref="IEqualityComparer{T}" /> wrapper used
    /// (at least) by <see cref="ObservableInMemoryCache{TKey,TValue}" /> implementation(s).
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ObservableCachedElementValueEqualityComparer<TKey, TValue> : IEqualityComparer<ObservableCachedElement<TKey, TValue>>
    {
        /// <summary>
        /// Gets the actually used <typeparamref name="TValue" /> equality comparer.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TValue" /> equality comparer.
        /// </value>
        protected IEqualityComparer<TValue> ValueEqualityComparer { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCachedElementValueEqualityComparer{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="valueEqualityComparer">The <typeparamref name="TValue"/> equality comparer.</param>
        public ObservableCachedElementValueEqualityComparer(
            IEqualityComparer<TValue> valueEqualityComparer = null)
        {
            ValueEqualityComparer = valueEqualityComparer ?? EqualityComparer<TValue>.Default;
        }

        #region Implementation of IEqualityComparer<in ObservableCachedElement<TKey,TValue>>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <see cref="ObservableInMemoryCache{TKey,TValue}"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="ObservableInMemoryCache{TKey,TValue}"/> to compare.</param>
        public bool Equals(ObservableCachedElement<TKey, TValue> x, ObservableCachedElement<TKey, TValue> y)
        {
            return ValueEqualityComparer.Equals(x.Value, y.Value);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(ObservableCachedElement<TKey, TValue> obj)
        {
            return ValueEqualityComparer.GetHashCode(obj.Value);
        }

        #endregion
    }
}