using System;
using System.Collections.Generic;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionaryChange{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="observableDictionaryChange"/> to its <see cref="IObservableCollectionChange{T}"/> counterpart.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException">$Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemChanged}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.</exception>
        public static IList<IObservableCollectionChange<KeyValuePair<TKey, TValue>>> ToObservableCollectionChanges<TKey, TValue>(this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));

            var result = new List<IObservableCollectionChange<KeyValuePair<TKey, TValue>>>();

            switch (observableDictionaryChange.ChangeType)
            {
                case ObservableDictionaryChangeType.ItemAdded:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemAdded,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value)));
                    break;
                case ObservableDictionaryChangeType.ItemChanged:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemChanged,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value)));
                    break;
                case ObservableDictionaryChangeType.ItemReplaced:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemRemoved,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.OldValue)));
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemAdded,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value)));
                    break;
                case ObservableDictionaryChangeType.ItemRemoved:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemRemoved,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.OldValue)));
                    break;
                case ObservableDictionaryChangeType.Reset:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.Reset));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableDictionaryChange),
                        $"Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemChanged}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.");
            }

            return result;
        }
    }
}