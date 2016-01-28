using System;
using System.Collections.Generic;
using System.Linq;
using JB.Collections.ExtensionMethods;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionaryChange{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryChangeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="observableDictionaryChange" /> to its <see cref="IObservableCollectionChange{T}" /> counterpart.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <param name="dictionary">The sender <see cref="IObservableDictionary{TKey,TValue}" />.</param>
        /// <param name="valueComparer">The <see cref="TValue"/> equality comparer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">observableDictionaryChange</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">$Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemValueChanged}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.</exception>
        public static IList<IObservableCollectionChange<KeyValuePair<TKey, TValue>>> ToObservableCollectionChanges<TKey, TValue>(
            this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange,
            IObservableDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TValue> valueComparer)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (valueComparer == null)
                throw new ArgumentNullException(nameof(valueComparer));

            var result = new List<IObservableCollectionChange<KeyValuePair<TKey, TValue>>>();

            switch (observableDictionaryChange.ChangeType)
            {
                case ObservableDictionaryChangeType.ItemAdded:
                    result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemAdded,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value)));
                    break;
                case ObservableDictionaryChangeType.ItemKeyChanged:
                {
                    TValue valueForKey;
                    if (((IDictionary<TKey, TValue>)dictionary).TryGetValue(observableDictionaryChange.Key, out valueForKey))
                    {
                        result.Add(new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                                ObservableCollectionChangeType.ItemChanged,
                                new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, valueForKey)));
                    }
                    break;
                }
                case ObservableDictionaryChangeType.ItemValueChanged:
                    {
                        result.AddRange(dictionary.GetKeysForValue(observableDictionaryChange.Value, valueComparer).Select(key => 
                            new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                                ObservableCollectionChangeType.ItemChanged,
                                new KeyValuePair<TKey, TValue>(key, observableDictionaryChange.Value))));
                    }
                    break;
                case ObservableDictionaryChangeType.ItemValueReplaced:
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
                        $"Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemValueChanged}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.");
            }

            return result;
        }
    }
}