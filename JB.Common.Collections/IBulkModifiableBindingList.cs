using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace JB.Collections
{
    public interface IBulkModifiableBindingList<T> :
        IBulkModifiableCollection<T>,
        IReadOnlyCollection<T>,
        ICollection<T>,
        ICollection,
        IBindingList,
        IReadOnlyList<T>,
        IList<T>,
        IList,
        IEnumerable<T>,
        IEnumerable,
        ICancelAddNew,
        IRaiseItemChangedEvents
    {
        /// <summary>
        /// Adds the range of items. Use <see cref="BindingList{T}.RaiseListChangedEvents" /> to control whether the range addition will
        /// be communicated via an implicit and per-item <see cref="ListChangedType.ItemAdded"/> event.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> a <see cref="ListChangedType.Reset"/> will be signaled when finished.
        /// This and <see cref="BindingList{T}.RaiseListChangedEvents"/> control if and what <see cref="IBindingList.ListChanged" />
        /// event will be raised while / after adding the <paramref name="items"/>.</param>
        void AddRange(IEnumerable<T> items, bool signalResetWhenFinished);

        /// <summary>
        /// Removes the range of items. Use <see cref="BindingList{T}.RaiseListChangedEvents" /> to control whether the range addition will
        /// be communicated via an implicit and per-item <see cref="ListChangedType.ItemDeleted"/> event.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> a <see cref="ListChangedType.Reset"/> will be signaled when finished.
        /// This and <see cref="BindingList{T}.RaiseListChangedEvents"/> control if and what <see cref="IBindingList.ListChanged" />
        /// event will be raised while / after adding the <paramref name="items"/>.</param>
        void RemoveRange(IEnumerable<T> items, bool signalResetWhenFinished);
    }
}