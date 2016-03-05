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
        /// Forwards the <paramref name="sourceObservable" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="sourceObservable">The source observable.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}" />.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="addRangePredicateForResets">This filter predicate tests which elements of the source <see cref="IObservableDictionary{TKey,TValue}" /> to add
        /// whenever a <see cref="ObservableDictionaryChangeType.Reset" /> is received. A reset is forwarded by clearing the <paramref name="target" /> completely and re-filling it with
        /// the source's values, and this predicate determines which ones are added. If no filter predicate is provided, all source values will be re-added to the <paramref name="target" />.</param>
        /// <param name="addDistinctValuesOnResetOnly">if set to <c>true</c> only distinct values will be re-added on <see cref="ObservableDictionaryChangeType.Reset" /> changes.</param>
        /// <param name="valueComparerForResets">The value equality comparer to use for reset changes and if <paramref name="valueComparerForResets"/> is set to [true]. If none is provided, the default one for the value type will be used</param>
        /// <returns>
        /// An <see cref="IDisposable" /> which will forward the changes to the <paramref name="target" /> as long as <see cref="IDisposable.Dispose" /> hasn't been called.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            this IObservable<IObservableDictionaryChange<TKey, TValue>> sourceObservable,
            IEnhancedBindingList<TValue> target,
            bool includeItemChanges = false,
            Func<KeyValuePair<TKey, TValue>, bool> addRangePredicateForResets = null,
            bool addDistinctValuesOnResetOnly = true,
            IEqualityComparer<TValue> valueComparerForResets = null)
        {
            if (sourceObservable == null)
                throw new ArgumentNullException(nameof(sourceObservable));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (addRangePredicateForResets == null)
            {
                addRangePredicateForResets = _ => true;
            }

            if (addDistinctValuesOnResetOnly == true && valueComparerForResets == null)
            {
                valueComparerForResets = EqualityComparer<TValue>.Default;
            }

            return sourceObservable.Subscribe(dictionaryChange =>
            {
                switch (dictionaryChange.ChangeType)
                {
                    case ObservableDictionaryChangeType.ItemAdded:
                        {
                            target.Add(dictionaryChange.Value);
                            break;
                        }
                    case ObservableDictionaryChangeType.ItemKeyChanged:
                        {
                            // nothing to do here
                            break;
                        }
                    case ObservableDictionaryChangeType.ItemValueChanged:
                        {
                            if (includeItemChanges)
                            {
                                // check whether target list contains the moved element at its expected index position
                                var targetIndex = target.IndexOf(dictionaryChange.Value);
                                if (targetIndex == -1)
                                    return;

                                target.ResetItem(targetIndex);
                            }
                            break;
                        }
                    case ObservableDictionaryChangeType.ItemValueReplaced:
                        {
                            if (includeItemChanges)
                            {
                                if (target.Contains(dictionaryChange.OldValue))
                                    target.Remove(dictionaryChange.OldValue);

                                var newValueTargetIndex = target.IndexOf(dictionaryChange.Value);
                                if (newValueTargetIndex != -1)
                                    target.ResetItem(newValueTargetIndex);
                                else
                                {
                                    target.Add(dictionaryChange.Value);
                                }
                            }
                            break;
                        }
                    case ObservableDictionaryChangeType.ItemRemoved:
                        {
                            // check whether target list contains the removed item, and delete if so
                            if (target.Contains(dictionaryChange.Value))
                            {
                                target.Remove(dictionaryChange.Value);
                            }
                            break;
                        }
                    case ObservableDictionaryChangeType.Reset:
                        {
                            var originalBindingRaiseListChangedEvents = target.RaiseListChangedEvents;
                            try
                            {
                                target.RaiseListChangedEvents = false;

                                target.Clear();

                                var rangeofValuesToAdd = dictionaryChange.Dictionary
                                    .Where(keyValuePair => addRangePredicateForResets(keyValuePair))
                                    .Select(kvp => kvp.Value);

                                if (addDistinctValuesOnResetOnly == true)
                                {
                                    rangeofValuesToAdd = rangeofValuesToAdd.Distinct(valueComparerForResets);
                                }

                                target.AddRange(rangeofValuesToAdd);
                            }
                            finally
                            {
                                target.RaiseListChangedEvents = originalBindingRaiseListChangedEvents;

                                if (originalBindingRaiseListChangedEvents)
                                    target.ResetBindings();
                            }

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dictionaryChange),
                            $"Only {ObservableDictionaryChangeType.ItemAdded}, {ObservableDictionaryChangeType.ItemKeyChanged}, {ObservableDictionaryChangeType.ItemValueChanged}, {ObservableDictionaryChangeType.ItemValueReplaced}, {ObservableDictionaryChangeType.ItemRemoved} and {ObservableDictionaryChangeType.Reset} are supported.");
                }
            });
        }

        /// <summary>
        /// Converts the given <paramref name="observableDictionaryChange" /> to its <see cref="IObservableCollectionChange{T}" /> counterpart.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="observableDictionaryChange">The observable dictionary change.</param>
        /// <param name="dictionary">The sender <see cref="IObservableDictionary{TKey,TValue}" />.</param>
        /// <param name="valueComparer">The <typeparamref name="TValue"/> equality comparer.</param>
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