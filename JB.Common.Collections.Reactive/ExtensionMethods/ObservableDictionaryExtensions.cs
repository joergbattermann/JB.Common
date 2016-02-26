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
        /// Forwards the <paramref name="sourceObservable" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="sourceObservable">The source observable.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        private static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            IObservable<IObservableDictionaryChange<TKey, TValue>> sourceObservable,
            IEnhancedBindingList<TValue> target,
            bool includeItemChanges = false)
        {
            if (sourceObservable == null)
                throw new ArgumentNullException(nameof(sourceObservable));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
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

                                ((ICollection<TValue>)target).Clear();
                                target.AddRange(((IDictionary<TKey, TValue>)dictionaryChange.Dictionary).Values);
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
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable dictionary.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            this IObservableDictionary<TKey,TValue> source,
            IEnhancedBindingList<TValue> target,
            bool includeItemChanges = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sourceObservable = scheduler != null
                ? source.DictionaryChanges.ObserveOn(scheduler)
                : source.DictionaryChanges;

            return ForwardDictionaryChangesTo(sourceObservable, target, includeItemChanges);
        }

        /// <summary>
        /// Forwards the <paramref name="source" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source observable dictionary.</param>
        /// <param name="target">The target <see cref="IEnhancedBindingList{TValue}"/>.</param>
        /// <param name="filterPredicate">A filter function to test each <paramref name="source" /> change for whether or not to forward it to the <paramref name="target" />.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the <paramref name="target" />.</param>
        /// <param name="scheduler">The scheduler to schedule notifications and changes on.</param>
        /// <returns>An <see cref="IDisposable"/> which will forward the changes to the <paramref name="target"/> as long as <see cref="IDisposable.Dispose"/> hasn't been called.</returns>
        public static IDisposable ForwardDictionaryChangesTo<TKey, TValue>(
            this IObservableDictionary<TKey, TValue> source,
            IEnhancedBindingList<TValue> target,
            Func<IObservableDictionaryChange<TKey, TValue>, bool> filterPredicate,
            bool includeItemChanges = false,
            IScheduler scheduler = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sourceObservable = scheduler != null
                ? source.DictionaryChanges.ObserveOn(scheduler)
                : source.DictionaryChanges;

            if (filterPredicate != null)
            {
                sourceObservable = sourceObservable.Where(filterPredicate);
            }

            return ForwardDictionaryChangesTo(sourceObservable, target, includeItemChanges);
        }
    }
}
