// -----------------------------------------------------------------------
// <copyright file="ObservableCollectionChange.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB.Collections.Reactive
{
    public class ObservableCollectionChange<T> : IObservableCollectionChange<T>
    {
        /// <summary>
        /// The type is a value type.. or not.. let's find out, lazily.
        /// </summary>
        private static readonly Lazy<bool> TypeIsValueType = new Lazy<bool>(() => typeof(T).IsValueType);

        #region Implementation of IObservableCollectionChange<out T>

        /// <summary>
        /// Gets the type of the change.
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        public ObservableCollectionChangeType ChangeType { get; }
        
        /// <summary>
        /// Gets the items that were changed or removed.
        /// </summary>
        /// <value>
        /// The affected items.
        /// </value>
        public T Item { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionChange{T}" /> class.
        /// </summary>
        /// <param name="changeType">Type of the change.</param>
        /// <param name="item">The item.</param>
        public ObservableCollectionChange(ObservableCollectionChangeType changeType, T item = default(T))
        {
            if (changeType == ObservableCollectionChangeType.Reset && (TypeIsValueType.Value == false && !Equals(item, default(T))))
                throw new ArgumentOutOfRangeException(nameof(item), $"Resets must not have an {nameof(item)}");

            ChangeType = changeType;
            Item = item;
        }

        /// <summary>
        /// Gets a <see cref="IObservableCollectionChange{T}"/> representing a <see cref="ObservableCollectionChangeType.Reset"/>.
        /// </summary>
        /// <value>
        /// The reset change type.
        /// </value>
        public static IObservableCollectionChange<T> Reset => new ObservableCollectionChange<T>(ObservableCollectionChangeType.Reset);
    }
}