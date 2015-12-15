using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JB.Collections.Reactive.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IObservableListChange{T}"/> instances.
    /// </summary>
    public static class ObservableListChangeExtensions
    {
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