// -----------------------------------------------------------------------
// <copyright file="ObservableDictionaryExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableDictionary{TKey,TValue}"/> instances.
    /// </summary>
    public static class ObservableDictionaryExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable list.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(this IObservableDictionary<TKey,TValue> source,
                                                          IEnhancedBindingList<TValue> target,
                                                          bool includeItemChanges = false,
                                                          IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            IObservable<IObservableDictionaryChange<TKey, TValue>> sourceObservable = scheduler != null
                ? source.DictionaryChanges.ObserveOn(scheduler)
                : source.DictionaryChanges;

            return sourceObservable.Subscribe(dictionaryChange =>
            {
                switch (dictionaryChange.ChangeType)
                {
                    case ObservableDictionaryChangeType.ItemAdded:
                        {
                            target.Add(dictionaryChange.Value);
                            break;
                        }
                    case ObservableDictionaryChangeType.ValueChanged:
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
                    case ObservableDictionaryChangeType.ValueReplaced:
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

                                ((ICollection<TValue>)target).Clear();
                                target.AddRange(((IDictionary<TKey, TValue>)source).Values);
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
                        break;
                }
            });
        }
    }
}
