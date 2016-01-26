using System;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionaryChange{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="observableDictionaryChange"/> to its <see cref="IObservableCacheChange{TKey,TValue}"/> representation.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">$The {nameof(ObservableDictionaryChangeType)}.{observableDictionaryChange.ChangeType} is not supported.</exception>
        public static IObservableCacheChange<TKey, TValue> ToObservableCacheChange<TKey, TValue>(
            this IObservableDictionaryChange<TKey, ObservableCachedElement<TKey, TValue>> observableDictionaryChange)
        {
            if (observableDictionaryChange == null) throw new ArgumentNullException(nameof(observableDictionaryChange));

            switch (observableDictionaryChange.ChangeType)
            {
                case ObservableDictionaryChangeType.Reset:
                    return ObservableCacheChange<TKey, TValue>.Reset();
                case ObservableDictionaryChangeType.ItemAdded:
                    return ObservableCacheChange<TKey, TValue>.ItemAdded(
                        observableDictionaryChange.Key,
                        observableDictionaryChange.Value.Value,
                        observableDictionaryChange.Value.ExpiresAt(),
                        observableDictionaryChange.Value.ExpirationType);
                // key changes are not supported
                //case ObservableDictionaryChangeType.KeyChanged:
                // break;
                // value changes are not supported
                //case ObservableDictionaryChangeType.ValueChanged:
                // break;
                case ObservableDictionaryChangeType.ValueReplaced:
                    return ObservableCacheChange<TKey, TValue>.ItemReplaced(
                        observableDictionaryChange.Key,
                        observableDictionaryChange.Value.Value,
                        observableDictionaryChange.OldValue.Value,
                        observableDictionaryChange.Value.ExpiresAt(),
                        observableDictionaryChange.Value.ExpirationType);
                case ObservableDictionaryChangeType.ItemRemoved:
                    return ObservableCacheChange<TKey, TValue>.ItemRemoved(
                        observableDictionaryChange.Key,
                        observableDictionaryChange.Value.Value,
                        observableDictionaryChange.Value.ExpiresAt(),
                        observableDictionaryChange.Value.ExpirationType);
                default:
                    throw new InvalidOperationException($"The {nameof(ObservableDictionaryChangeType)}.{observableDictionaryChange.ChangeType} is not supported.");
            }
        }
    }
}