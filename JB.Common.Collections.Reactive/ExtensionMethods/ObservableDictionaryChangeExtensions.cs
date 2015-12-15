using System;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionaryChange{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryChangeExtensions
    {
        public static IObservableCollectionChange<TValue> ToObservableCollectionChange<TKey, TValue>(this IObservableDictionaryChange<TKey, TValue> observableDictionaryChange, IObservableDictionary<TKey, TValue> sender)
        {
            if (observableDictionaryChange == null) throw new ArgumentNullException(nameof(observableDictionaryChange));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            
            switch (observableDictionaryChange.ChangeType)
            {
                case ObservableDictionaryChangeType.ItemAdded:
                    return new ObservableCollectionChange<TValue>(
                        ObservableCollectionChangeType.ItemAdded,
                        observableDictionaryChange.Value);
            }
        }
    }
}