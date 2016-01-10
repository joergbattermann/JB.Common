using System;
using JB.Collections.Reactive;

namespace JB.Reactive.Cache.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionaryChange{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryChangeExtensions
    {
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
                case ObservableDictionaryChangeType.ItemChanged:
                    return ObservableCacheChange<TKey, TValue>.ItemChanged(
                        observableDictionaryChange.Key,
                        observableDictionaryChange.Value.Value,
                        observableDictionaryChange.ChangedPropertyName,
                        observableDictionaryChange.Value.ExpiresAt(),
                        observableDictionaryChange.Value.ExpirationType);
                case ObservableDictionaryChangeType.ItemReplaced:
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