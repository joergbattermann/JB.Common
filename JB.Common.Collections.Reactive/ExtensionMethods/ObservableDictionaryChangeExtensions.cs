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
        public static IObservableCollectionChange<KeyValuePair<TKey, TValue>> ToObservableCollectionChange<TKey, TValue>(this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));

            switch (observableDictionaryChange.ChangeType)
            {
                case ObservableDictionaryChangeType.ItemAdded:
                    return new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemAdded,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value));
                case ObservableDictionaryChangeType.ItemChanged:
                    return new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemChanged,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value));
                case ObservableDictionaryChangeType.ItemRemoved:
                    return new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.ItemRemoved,
                        new KeyValuePair<TKey, TValue>(observableDictionaryChange.Key, observableDictionaryChange.Value));
                case ObservableDictionaryChangeType.Reset:
                    return new ObservableCollectionChange<KeyValuePair<TKey, TValue>>(
                        ObservableCollectionChangeType.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableDictionaryChange),
                        $"Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemChanged}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.");
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="observableDictionaryChange"/> is a replacement <see cref="ObservableCollectionChangeType.ItemChanged"/> type.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <returns>[true] if change is a replacement type, [false] otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">If <paramref name="observableDictionaryChange"/> is [null].</exception>
        public static bool IsReplacementItemChange<TKey, TValue>(this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));

            return string.IsNullOrWhiteSpace(observableDictionaryChange.ChangedPropertyName);
        }

        /// <summary>
        /// Determines whether the <paramref name="observableDictionaryChange"/> is a PropertyChanged <see cref="ObservableCollectionChangeType.ItemChanged"/> type.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <returns>[true] if change is a PropertyChanged type, [false] otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">If <paramref name="observableDictionaryChange"/> is [null].</exception>
        public static bool IsPropertyChangedItemChange<TKey, TValue>(this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange)
        {
            if (observableDictionaryChange == null)
                throw new ArgumentNullException(nameof(observableDictionaryChange));

            return string.IsNullOrWhiteSpace(observableDictionaryChange.ChangedPropertyName);
        }
    }
}