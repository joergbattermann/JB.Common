// -----------------------------------------------------------------------
// <copyright file="ObservableList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using JB.ExtensionMethods;

namespace JB.Collections.Reactive
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableList<T> : ObservableCollection<T>, IObservableList<T>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableList{T}" /> class.
        /// </summary>
        /// <param name="list">The initial list, if any.</param>
        /// <param name="syncRoot">The object used to synchronize access to the thread-safe collection.</param>
        /// <param name="scheduler">The scheduler to raise events on.</param>
        public ObservableList(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
            : base(list, syncRoot, scheduler)
        {
            afgdagf >> this needs to override / provide what the collection cannot provide
        }

        #region Implementation of IList
        
        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <returns>
        ///     The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the
        ///     collection.
        /// </returns>
        /// <param name="value">The object to add to the <see cref="T:System.Collections.IList" />. </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList" /> is read-only.-or- The
        ///     <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </exception>
        int IList.Add(object value)
        {
            CheckForAndThrowIfDisposed();

            if (value != null && value.IsObjectOfType<T>())
            {
                Add((T) value);
                return Count - 1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise,
        ///     false.
        /// </returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
        bool IList.Contains(object value)
        {
            CheckForAndThrowIfDisposed();
            return ((IList) InnerList).Contains(value);
        }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <returns>
        ///     The index of <paramref name="value" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
        int IList.IndexOf(object value)
        {
            CheckForAndThrowIfDisposed();

            return ((IList) InnerList).IndexOf(value);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
        /// <param name="value">The object to insert into the <see cref="T:System.Collections.IList" />. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.IList" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList" /> is read-only.-or- The
        ///     <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </exception>
        /// <exception cref="T:System.NullReferenceException">
        ///     <paramref name="value" /> is null reference in the
        ///     <see cref="T:System.Collections.IList" />.
        /// </exception>
        void IList.Insert(int index, object value)
        {
            CheckForAndThrowIfDisposed();

            ((IList) InnerList).Insert(index, value);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="T:System.Collections.IList" />. </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList" /> is read-only.-or- The
        ///     <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </exception>
        void IList.Remove(object value)
        {
            CheckForAndThrowIfDisposed();

            ((IList) InnerList).Remove(value);
        }
        
        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        ///     The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.IList" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The property is set and the <see cref="T:System.Collections.IList" />
        ///     is read-only.
        /// </exception>
        object IList.this[int index]
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList[index];
            }
            set
            {
                CheckForAndThrowIfDisposed();

                if (value.IsObjectOfType<T>() == false)
                    throw new ArgumentOutOfRangeException(nameof(value));

                InnerList[index] = (T) value;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.
        /// </returns>
        public virtual bool IsFixedSize
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsFixedSize;
            }
        }
        
        #endregion
        
        #region Implementation of IList<T>

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <returns>
        ///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public virtual int IndexOf(T item)
        {
            CheckForAndThrowIfDisposed();

            return InnerList.IndexOf(item);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.Generic.IList`1" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public virtual void Insert(int index, T item)
        {
            CheckForAndThrowIfDisposed();

            InnerList.Insert(index, item);
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.Generic.IList`1" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public virtual void RemoveAt(int index)
        {
            CheckForAndThrowIfDisposed();

            InnerList.RemoveAt(index);
        }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        ///     The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.Generic.IList`1" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The property is set and the
        ///     <see cref="T:System.Collections.Generic.IList`1" /> is read-only.
        /// </exception>
        public virtual T this[int index]
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerList[index];
            }
            set
            {
                CheckForAndThrowIfDisposed();

                InnerList[index] = value;
            }
        }

        #endregion

        #region Implementation of IItemMovableList<T>


        /// <summary>
        ///     Moves the specified item to the new index position.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="correctNewIndexOnIndexShift">
        ///     if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
        ///     if required, depending on whether an index shift took place during the move due to the original position of the
        ///     item.
        ///     Basically if you move an item from a lower index position to a higher one, the index positions of all items with
        ///     higher index positions than the <paramref name="item" /> ones
        ///     will be shifted upwards (logically by -1).
        ///     Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex" />
        ///     position, correction might be useful.
        /// </param>
        public virtual void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = true)
        {
            if (Equals(item, default(T))) throw new ArgumentOutOfRangeException(nameof(item));
            if (newIndex < 0 || newIndex >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));

            CheckForAndThrowIfDisposed();

            InnerList.Move(item, newIndex, correctNewIndexOnIndexShift);
        }

        /// <summary>
        ///     Moves the item(s) at the specified index to a new position in the list.
        /// </summary>
        /// <param name="itemIndex">The (starting) index of the item(s) to move.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="correctNewIndexOnIndexShift">
        ///     if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
        ///     if required, depending on whether an index shift took place during the move due to the original position of the
        ///     item.
        ///     Basically if you move an item from a lower index position to a higher one, the index positions of all items with
        ///     higher index positions than <paramref name="itemIndex" />
        ///     will be shifted upwards (logically by -1).
        ///     Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex" />
        ///     position, correction might be useful.
        /// </param>
        public virtual void Move(int itemIndex, int newIndex, bool correctNewIndexOnIndexShift = true)
        {
            if (itemIndex < 0 || itemIndex >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
            if (newIndex < 0 || newIndex >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));

            CheckForAndThrowIfDisposed();

            InnerList.Move(itemIndex, newIndex, correctNewIndexOnIndexShift);
        }
        #endregion
    }
}