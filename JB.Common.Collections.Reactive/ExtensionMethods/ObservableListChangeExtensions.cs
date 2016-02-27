using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableListChange{T}"/> instances.
    /// </summary>
    public static class ObservableListChangeExtensions
    {
        /// <summary>
        /// Forwards the <paramref name="sourceObservable" /> changes to the <paramref name="target" />.
        /// </summary>
        /// <typeparam name="T">The type of the list item(s)</typeparam>
        /// <param name="sourceObservable">The source observable.</param>
        /// <param name="target">The target binding list.</param>
        /// <param name="includeItemChanges">if set to <c>true</c> individual items' changes will be propagated to the
        /// <paramref name="target" /> via replacing the item completely.</param>
        /// <param name="includeMoves">if set to <c>true</c> move operations will be replicated to the <paramref name="target" />.</param>
        /// <param name="addRangePredicateForResets">This filter predicate tests which elements of the source <see cref="IObservableListChange{T}"/> to add
        /// whenever a <see cref="ObservableListChangeType.Reset"/> is received. A reset is forwarded by clearing the <paramref name="target"/> completely and re-filling it with
        /// the source's values, and this predicate determines which ones are added. If no filter predicate is provided, all source values will be re-added to the <paramref name="target"/>.</param>
        /// <returns></returns>
        public static IDisposable ForwardListChangesTo<T>(
            this IObservable<IObservableListChange<T>> sourceObservable,
            IEnhancedBindingList<T> target,
            bool includeItemChanges = true,
            bool includeMoves = false,
            Func<T, bool> addRangePredicateForResets = null)
        {
            if (sourceObservable == null)
                throw new ArgumentNullException(nameof(sourceObservable));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (addRangePredicateForResets == null)
            {
                addRangePredicateForResets = _ => true;
            }

            return sourceObservable.Subscribe(observableListChange =>
            {
                switch (observableListChange.ChangeType)
                {
                    case ObservableListChangeType.ItemAdded:
                        {
                            if (includeMoves)
                                target.Insert(observableListChange.Index, observableListChange.Item);
                            else
                                target.Add(observableListChange.Item);
                            break;
                        }
                    case ObservableListChangeType.ItemChanged:
                        {
                            if (includeItemChanges)
                            {
                                // check whether target list contains the moved element at its expected index position
                                var targetIndex = target.IndexOf(observableListChange.Item);
                                if (targetIndex == -1)
                                    return;

                                target.ResetItem(targetIndex);
                            }
                            break;
                        }
                    case ObservableListChangeType.ItemMoved:
                        {
                            if (includeMoves)
                            {
                                // check whether target list contains the moved element at its expected index position
                                if (target.IndexOf(observableListChange.Item) != observableListChange.OldIndex)
                                {
                                    throw new InvalidOperationException($"The source and and target lists are no longer in sync: target has a diffent item at index position {observableListChange.OldIndex} than expected.");
                                }

                                target.Move(observableListChange.Item, observableListChange.Index);
                            }
                            break;
                        }
                    case ObservableListChangeType.ItemRemoved:
                        {
                            // check whether target list contains the removed item, and delete if so
                            if (target.Contains(observableListChange.Item))
                            {
                                target.Remove(observableListChange.Item);
                            }
                            break;
                        }
                    case ObservableListChangeType.Reset:
                        {
                            var originalBindingRaiseListChangedEvents = target.RaiseListChangedEvents;
                            try
                            {
                                target.RaiseListChangedEvents = false;
                                target.Clear();
                                target.AddRange(observableListChange.List.Where(addRangePredicateForResets));
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

        /// <summary>
        /// Converts the given <paramref name="observableListChange" /> to its <see cref="ListChangedEventArgs" /> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observableListChange">The <see cref="IObservableListChange{T}" /> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static ListChangedEventArgs ToListChangedEventArgs<T>(this IObservableListChange<T> observableListChange)
        {
            if (observableListChange == null)
                throw new ArgumentNullException(nameof(observableListChange));

            switch (observableListChange.ChangeType)
            {
                case ObservableListChangeType.ItemAdded:
                    return new ListChangedEventArgs(ListChangedType.ItemAdded, observableListChange.Index);
                case ObservableListChangeType.ItemChanged:
                    return new ListChangedEventArgs(ListChangedType.ItemChanged, observableListChange.Index, observableListChange.OldIndex);
                case ObservableListChangeType.ItemMoved:
                    return new ListChangedEventArgs(ListChangedType.ItemMoved, observableListChange.Index, observableListChange.OldIndex);
                case ObservableListChangeType.ItemRemoved:
                    return new ListChangedEventArgs(ListChangedType.ItemDeleted, observableListChange.Index, observableListChange.OldIndex);
                case ObservableListChangeType.Reset:
                    return new ListChangedEventArgs(ListChangedType.Reset, observableListChange.Index);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableListChange),
                        $"Only {ObservableListChangeType.ItemAdded}, {ObservableListChangeType.ItemChanged}, {ObservableListChangeType.ItemMoved}, {ObservableListChangeType.ItemRemoved} and {ObservableListChangeType.Reset} are supported.");
            }
        }

        /// <summary>
        /// Converts the given <paramref name="observableListChange" /> to its <see cref="NotifyCollectionChangedEventArgs" /> counter part.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observableListChange">The <see cref="IObservableListChange{T}" /> instance containing the event data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static NotifyCollectionChangedEventArgs ToNotifyCollectionChangedEventArgs<T>(this IObservableListChange<T> observableListChange)
        {
            if (observableListChange == null) throw new ArgumentNullException(nameof(observableListChange));

            switch (observableListChange.ChangeType)
            {
                case ObservableListChangeType.ItemAdded:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, observableListChange.Item, observableListChange.Index);
                case ObservableListChangeType.ItemChanged:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, observableListChange.Item, observableListChange.Index);
                case ObservableListChangeType.ItemMoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, observableListChange.Item, observableListChange.Index, observableListChange.OldIndex);
                case ObservableListChangeType.ItemRemoved:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, observableListChange.Item, observableListChange.Index, observableListChange.OldIndex);
                case ObservableListChangeType.Reset:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observableListChange),
                        $"Only {ObservableListChangeType.ItemAdded}, {ObservableListChangeType.ItemChanged}, {ObservableListChangeType.ItemMoved}, {ObservableListChangeType.ItemRemoved} and {ObservableListChangeType.Reset} are supported.");
            }
        }
    }
}