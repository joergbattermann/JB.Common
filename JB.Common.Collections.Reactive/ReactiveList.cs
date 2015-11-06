// -----------------------------------------------------------------------
// <copyright file="ReactiveList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using JB.ExtensionMethods;

namespace JB.Collections
{
	public class ReactiveList<T> : IReactiveBindingList<T>, IDisposable
	{
        /// <summary>
        /// Gets the inner list.
        /// </summary>
        /// <value>
        /// The inner list.
        /// </value>
        protected SchedulerSynchronizedBindingList<T> InnerList { get; }

		/// <summary>
		/// Gets the used scheduler.
		/// </summary>
		/// <value>
		/// The scheduler.
		/// </value>
		private IScheduler Scheduler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveList{T}" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="enableItemChangeTracking">if set to <c>true</c> and if <typeparam name="T"/> implements <see cref="INotifyPropertyChanged"/>, individual items' changes will handled and forwarded by this instance.</param>
        /// <param name="itemChangesToResetThreshold">The <see cref="ItemChangesToResetThreshold" /> value.</param>
        /// <param name="syncRoot">The synchronize root.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Must be between 0 and 1 (both inclusive)</exception>
        public ReactiveList(IList<T> list = null, bool enableItemChangeTracking = true, double itemChangesToResetThreshold = 0.3, object syncRoot = null, IScheduler scheduler = null)
		{
			if(itemChangesToResetThreshold < 0 || itemChangesToResetThreshold > 1) throw new ArgumentOutOfRangeException(nameof(itemChangesToResetThreshold), "Must be between 0 and 1 (both inclusive)");

			SyncRoot = syncRoot ?? new object();
			Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.Default;

			InnerList = new SchedulerSynchronizedBindingList<T>(list, SyncRoot, Scheduler);

			ItemChangesToResetThreshold = itemChangesToResetThreshold;
            MinimumItemsChangedToBeConsideredReset = 10;

            IsItemChangeTrackingEnabled = enableItemChangeTracking;

			SetupObservablesAndSubjects();
		}

		/// <summary>
		/// Gets the number of elements contained in this instance.
		/// </summary>
		/// <returns>
		/// The number of elements contained in this instance.
		/// </returns>
		public int Count
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return InnerList.Count;
		    }
		}

	    /// <summary>
		/// Gets a value indicating whether the instance is read-only.
		/// </summary>
		/// <returns>
		/// true if the instance is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsReadOnly;
		    }
		}

	    #region Implementation of INotifyCollectionChanged

        /// <summary>
        /// Occurs when the collection changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
		{
            CheckForAndThrowIfDisposed();

            return InnerList.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
            return GetEnumerator();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out T>

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
		public void Add(T item)
		{
            Insert(InnerList.Count, item);
        }

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <returns>
		/// The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.
		/// </returns>
		/// <param name="value">The object to add to the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
		public int Add(object value)
		{
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
		/// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
		/// </returns>
		/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>. </param>
		public bool Contains(object value)
		{
            CheckForAndThrowIfDisposed();
            return ((IList)InnerList).Contains(value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception>
		void IList.Clear()
		{
            CheckForAndThrowIfDisposed();
            throw new NotImplementedException();
		}

		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <returns>
		/// The index of <paramref name="value"/> if found in the list; otherwise, -1.
		/// </returns>
		/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>. </param>
		public int IndexOf(object value)
		{
            CheckForAndThrowIfDisposed();
            return ((IList)InnerList).IndexOf(value);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted. </param><param name="value">The object to insert into the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><exception cref="T:System.NullReferenceException"><paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception>
		public void Insert(int index, object value)
		{
            CheckForAndThrowIfDisposed();
            throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <param name="value">The object to remove from the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
		public void Remove(object value)
		{
            CheckForAndThrowIfDisposed();
            throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.IList"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
		void IList.RemoveAt(int index)
		{
            CheckForAndThrowIfDisposed();
            throw new NotImplementedException();
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		/// </returns>
		/// <param name="index">The zero-based index of the element to get or set. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.IList"/> is read-only. </exception>
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

                throw new NotImplementedException();
		    }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.
		/// </returns>
		bool IList.IsReadOnly
		{
		    get
            {
                CheckForAndThrowIfDisposed();
                return IsReadOnly;
            }
		}

	    /// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.
		/// </returns>
		public bool IsFixedSize
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsFixedSize;
		    }
		}

	    /// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
		void ICollection<T>.Clear()
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		public bool Contains(T item)
		{
            CheckForAndThrowIfDisposed();
            return InnerList.Contains(item);
		}

	    /// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
            CheckForAndThrowIfDisposed();

            InnerList.CopyTo(array, arrayIndex);
        }

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
		public bool Remove(T item)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.-or-The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
		public void CopyTo(Array array, int index)
		{
            CheckForAndThrowIfDisposed();

            ((IList)InnerList).CopyTo(array, index);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </returns>
		int ICollection.Count
		{
		    get
		    {
                CheckForAndThrowIfDisposed();

                return Count;
		    }
		}

	    /// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <returns>
		/// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
		/// </returns>
		public object SyncRoot { get; }

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
		/// </summary>
		/// <returns>
		/// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
		/// </returns>
		public bool IsSynchronized
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsSynchronized;
		    }
		}

	    /// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		int ICollection<T>.Count
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return Count;
		    }
		}

	    /// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		bool ICollection<T>.IsReadOnly
		{
		    get
            {
                CheckForAndThrowIfDisposed();
                return IsReadOnly;
            }
		}

	    /// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		int IReadOnlyCollection<T>.Count
		{
		    get
		    {
                CheckForAndThrowIfDisposed();
                return Count;
		    }
		}

	    #endregion

		#region Implementation of IReactiveCollection<T>

		/// <summary>
		/// Adds a range of items.
		/// </summary>
		/// <param name="items">The items.</param>
		public void AddRange(IEnumerable<T> items)
		{
		    if (items == null) throw new ArgumentNullException(nameof(items));

            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Moves the specified item to the new index position.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="newIndex">The new index.</param>
		/// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
		/// if required, depending on whether an index shift took place during the move due to the original position of the item.
		/// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than the <paramref name="item" /> ones
		/// will be shifted upwards (logically by -1).
		/// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex"/> position, correction might be useful.</param>
		public void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = false)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Moves the item(s) at the specified index to a new position in the list.
		/// </summary>
		/// <param name="itemIndex">The (starting) index of the item(s) to move.</param>
		/// <param name="newIndex">The new index.</param>
		/// <param name="count">The amount of items to move, starting at the itemIndex.</param>
		/// <param name="correctNewIndexOnIndexShift">if set to <c>true</c> the <paramref name="newIndex" /> will be adjusted,
		/// if required, depending on whether an index shift took place during the move due to the original position of the item.
		/// Basically if you move an item from a lower index position to a higher one, the index positions of all items with higher index positions than <paramref name="itemIndex" />
		/// will be shifted upwards (logically by -1).
		/// Depending on whether the caller intends to move the item strictly or logically to the <paramref name="newIndex" /> position, correction might be useful.</param>
		public void Move(int itemIndex, int newIndex, int count = 1, bool correctNewIndexOnIndexShift = false)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the specified items.
		/// </summary>
		/// <param name="items">The items.</param>
		public void RemoveRange(IEnumerable<T> items)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the amount of items starting at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		public void RemoveRange(int index, int count)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
            CheckForAndThrowIfDisposed();

            _resets.OnNext(Unit.Default);
		}

		#endregion

		#region Implementation of IList<T>

		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		public int IndexOf(T item)
        {
            CheckForAndThrowIfDisposed();

            return InnerList.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		public void Insert(int index, T item)
		{
            CheckForAndThrowIfDisposed();

		    InnerList.Insert(index, item);

		    if (IsItemChangeTrackingEnabled)
		    {
		        AddItemToPropertyChangedTracking(item);
		    }

		    if (IsSuppressingReactiveCollectionChangedNotifications == false)
		    {
		        if (IsItemsChangedAmountGreaterThanResetThreshold(1, Count, MinimumItemsChangedToBeConsideredReset, ItemChangesToResetThreshold))
		        {

		        }
		        else
		        {
		            
		        }
		    }

            throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		void IList<T>.RemoveAt(int index)
		{
            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		/// </returns>
		/// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		public T this[int index]
		{
		    get
		    {
                CheckForAndThrowIfDisposed();

                return InnerList[index];
		    }
		    set
		    {
                CheckForAndThrowIfDisposed();

                throw new NotImplementedException();
		    }
		}

        #endregion

        #region Implementation of INotifyReactiveCollectionChanged<out T>

        private readonly object _isSuppressingReactiveCollectionChangedNotificationsLocker = new object();
        long _isSuppressingReactiveCollectionChangedNotifications = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is suppressing reactive collection changed notifications.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is suppressing reactive collection changed notifications; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuppressingReactiveCollectionChangedNotifications
        {
            get
            {
                return Interlocked.Read(ref _isSuppressingReactiveCollectionChangedNotifications) == 1;
                
            }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isSuppressingReactiveCollectionChangedNotificationsLocker)
                {
                    if (value && IsSuppressingReactiveCollectionChangedNotifications)
                        throw new InvalidOperationException("A Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    Interlocked.Exchange(ref _isSuppressingReactiveCollectionChangedNotifications, value ? 1 : 0);
                }
            }
        }

        private long _isItemChangeTrackingEnabled = 0;

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to <typeparam name="T"/>'s <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if the interface is implemented.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsItemChangeTrackingEnabled
	    {
            get
            {
                return Interlocked.Read(ref _isItemChangeTrackingEnabled) == 1;

            }
            protected set
            {
                CheckForAndThrowIfDisposed();

                Interlocked.Exchange(ref _isItemChangeTrackingEnabled, value ? 1 : 0);
            }
        }

	    /// <summary>
        /// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IDisposable SuppressReactiveCollectionChangedNotifications(bool signalResetWhenFinished = true)
		{
            CheckForAndThrowIfDisposed();

            IsSuppressingReactiveCollectionChangedNotifications = true;

            return Disposable.Create(() =>
            {
                IsSuppressingReactiveCollectionChangedNotifications = false;
                if (signalResetWhenFinished)
                {
                    _resets.OnNext(Unit.Default);
                }
            });
		}

		/// <summary>
		/// Indicates at what percentage / fraction bulk changes are signaled as a Reset rather than individual change()s.
		/// [0] = Always, [1] = Only when ALL current items change (well except if list is entirely empty to begin with).
		/// </summary>
		/// <value>
		/// The changes to reset threshold.
		/// </value>
		public double ItemChangesToResetThreshold { get; }

	    /// <summary>
	    /// Gets the minimum amount of items that have been changed to be notified / considered a <see cref="ReactiveCollectionChangeType.Reset"/> rather than indivudal <see cref="ReactiveCollectionChangeType"/> notifications.
	    /// </summary>
	    /// <value>
	    /// The minimum items changed to be considered reset.
	    /// </value>
	    public int MinimumItemsChangedToBeConsideredReset
	    {
	        get
	        {
	            return _minimumItemsChangedToBeConsideredReset;
	        }
	        set
	        {
                _minimumItemsChangedToBeConsideredReset = value;
                RaisePropertyChanged();
	        }
	    }

	    /// <summary>
		/// Gets the collection change notifications as an observable stream.
		/// </summary>
		/// <value>
		/// The collection changes.
		/// </value>
		public IObservable<IReactiveCollectionChange<T>> CollectionChanges
		{
		    get
		    {
                CheckForAndThrowIfDisposed();

                return _collectionChanges.TakeWhile(_ => !IsDisposing && !IsDisposed).SkipWhile(_ => IsSuppressingReactiveCollectionChangedNotifications);
		    }
		}

	    /// <summary>
        /// Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        /// The count changes.
        /// </value>
        public IObservable<int> CountChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _countChanges.TakeWhile(_ => !IsDisposing && !IsDisposed).SkipWhile(_ => IsSuppressingReactiveCollectionChangedNotifications).DistinctUntilChanged();
            }
        }

        /// <summary>
        /// Gets the reset notifications as an observable stream.  Whenever signaled,
        /// observers should reset any knowledge / state etc about the list.
        /// </summary>
        /// <value>
        /// The resets.
        /// </value>
        public IObservable<Unit> Resets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _resets.TakeWhile(_ => !IsDisposing && !IsDisposed).SkipWhile(_ => IsSuppressingReactiveCollectionChangedNotifications);
            }
        }

	    #endregion

        Subject<IReactiveCollectionChange<T>> _collectionChanges;
        Subject<int> _countChanges;
        Subject<Unit> _resets;

	    IDisposable _countChangesPropertyChangeForwarder = null;
        IDisposable _collectionChangesAndResetsPropertyChangeForwarder = null;

	    IDisposable _innerListListChangedForwader = null;

        /// <summary>
		/// Setups the observables.
		/// </summary>
		private void SetupObservablesAndSubjects()
        {
            // prepare subjects for RX
            _collectionChanges = new Subject<IReactiveCollectionChange<T>>();
            _countChanges = new Subject<int>();
            _resets = new Subject<Unit>();

            // then connect to InnerList's ListChanged Event
            _innerListListChangedForwader = Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
                handler => InnerList.ListChanged += handler,
                handler => InnerList.ListChanged -= handler)
                .TakeWhile(_ => !IsDisposing && !IsDisposed)
                .SkipWhile(_ => IsSuppressingReactiveCollectionChangedNotifications)
                .ObserveOn(Scheduler)
                .Subscribe(ForwardInnerListChangedEventsToObservers);

            // 'Count' and 'Item[]' PropertyChanged events are used by WPF
            _countChangesPropertyChangeForwarder = CountChanges.Subscribe(_ => RaisePropertyChanged("Count"));
            _collectionChangesAndResetsPropertyChangeForwarder = CollectionChanges.Select(_ => Unit.Default).Merge(Resets).Subscribe(_ => RaisePropertyChanged("Item[]"));
        }

	    private void ForwardInnerListChangedEventsToObservers(EventPattern<ListChangedEventArgs> eventPattern)
	    {
	        switch (eventPattern.EventArgs.ListChangedType)
	        {
	            case ListChangedType.Reset:
	            {
                        _resets.OnNext(Unit.Default);
                        _collectionChanges.OnNext(new ReactiveCollectionChange<T>(ReactiveCollectionChangeType.Reset));
	                break;
	            }
                    case ListChangedType.ItemAdded:
	            {
	                _collectionChanges.On
	            }
	        }
        }

	    /// <summary>
        /// Determines whether the amount of changed items is greater than the reset threshold and / or the minimum amount of items to be considered as a reset.
        /// </summary>
        /// <param name="itemsChanged">The items changed.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="minimumItemsChangedCountToBeConsideredReset">The minimum changed items count to be considered a reset.</param>
        /// <param name="resetThreshold">The reset threshold.</param>
        /// <returns></returns>
        private bool IsItemsChangedAmountGreaterThanResetThreshold(int itemsChanged, int totalCount, int minimumItemsChangedCountToBeConsideredReset, double resetThreshold)
        {
            if (itemsChanged < 0) throw new ArgumentOutOfRangeException(nameof(itemsChanged));
            if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount));
            if (minimumItemsChangedCountToBeConsideredReset < 0) throw new ArgumentOutOfRangeException(nameof(minimumItemsChangedCountToBeConsideredReset));

            // if the list is entirely empty, it isn't really a reset
            if (itemsChanged < minimumItemsChangedCountToBeConsideredReset || totalCount == 0)
                return false;

            return (((double)itemsChanged) / totalCount) >= resetThreshold;
        }

        private void AddItemToPropertyChangedTracking(T item)
	    {
	        if (item == null) throw new ArgumentNullException(nameof(item));

            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
        }

        private void RemoveItemFromPropertyChangedTracking(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            CheckForAndThrowIfDisposed();

            throw new NotImplementedException();
        }

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
		    CheckForAndThrowIfDisposed();

            Scheduler.Schedule(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
		}

		#endregion

	    #region Implementation of IDisposable

	    private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();
	    private volatile int _minimumItemsChangedToBeConsideredReset;
	    private bool _allowNew;
	    private bool _allowEdit;
	    private bool _allowRemove;
	    private bool _supportsChangeNotification;
	    private bool _supportsSearching;
	    private bool _supportsSorting;
	    private bool _isSorted;
	    private PropertyDescriptor _sortProperty;
	    private ListSortDirection _sortDirection;
	    private bool _raisesItemChangedEvents;

	    /// <summary>
        /// Gets or sets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get
            {
                return Interlocked.Read(ref _isDisposed) == 1;

            }
            protected set
            {
                lock (_isDisposedLocker)
                {
                    if (value == false && IsDisposed)
                        throw new InvalidOperationException("Once Disposed has been set, it cannot be reset back to false.");

                    Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposing
        {
            get
            {
                return Interlocked.Read(ref _isDisposing) == 1;

            }
            protected set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
	    {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                if (disposeManagedResources)
                {
                    _collectionChangesAndResetsPropertyChangeForwarder.CheckAndDispose();
                    _countChangesPropertyChangeForwarder.CheckAndDispose();

                    _collectionChanges.CheckAndDispose();
                    _countChanges.CheckAndDispose();
                    _resets.CheckAndDispose();
                }
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Checks whether this instance is currently or already has been disposed.
        /// </summary>
        private void CheckForAndThrowIfDisposed()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException("This instance is currently being disposed.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException("This instance has been disposed.");
            }
        }

        #endregion

	    #region Implementation of IBindingList

	    /// <summary>
	    /// Adds a new item to the list.
	    /// </summary>
	    /// <returns>
	    /// The item added to the list.
	    /// </returns>
	    /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.AllowNew"/> is false. </exception>
	    public object AddNew()
	    {
            return (InnerList as IBindingList).AddNew();
        }

	    /// <summary>
	    /// Adds the <see cref="T:System.ComponentModel.PropertyDescriptor"/> to the indexes used for searching.
	    /// </summary>
	    /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to add to the indexes used for searching. </param>
	    public void AddIndex(PropertyDescriptor property)
	    {
            (InnerList as IBindingList).AddIndex(property);
        }

        /// <summary>
        /// Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor"/> and a <see cref="T:System.ComponentModel.ListSortDirection"/>.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to sort by. </param><param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values. </param><exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
	    {
            (InnerList as IBindingList).ApplySort(property, direction);
        }

	    /// <summary>
	    /// Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
	    /// </summary>
	    /// <returns>
	    /// The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
	    /// </returns>
	    /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to search on. </param><param name="key">The value of the <paramref name="property"/> parameter to search for. </param><exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSearching"/> is false. </exception>
	    public int Find(PropertyDescriptor property, object key)
	    {
            return (InnerList as IBindingList).Find(property, key);
        }

	    /// <summary>
	    /// Removes the <see cref="T:System.ComponentModel.PropertyDescriptor"/> from the indexes used for searching.
	    /// </summary>
	    /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to remove from the indexes used for searching. </param>
	    public void RemoveIndex(PropertyDescriptor property)
	    {
            (InnerList as IBindingList).RemoveIndex(property);
        }

        /// <summary>
        /// Removes any sort applied using <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public void RemoveSort()
	    {
            (InnerList as IBindingList).RemoveSort();
        }

	    /// <summary>
	    /// Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
	    /// </summary>
	    /// <returns>
	    /// true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>; otherwise, false.
	    /// </returns>
	    public bool AllowNew
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowNew;
            }
        }

        /// <summary>
        /// Gets whether you can update items in the list.
        /// </summary>
        /// <returns>
        /// true if you can update the items in the list; otherwise, false.
        /// </returns>
        public bool AllowEdit
	    {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowEdit;
            }
        }

	    /// <summary>
	    /// Gets whether you can remove items from the list, using <see cref="M:System.Collections.IList.Remove(System.Object)"/> or <see cref="M:System.Collections.IList.RemoveAt(System.Int32)"/>.
	    /// </summary>
	    /// <returns>
	    /// true if you can remove items from the list; otherwise, false.
	    /// </returns>
	    public bool AllowRemove
	    {
	        get
	        {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowRemove;
            }
	    }

	    /// <summary>
	    /// Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or an item in the list changes.
	    /// </summary>
	    /// <returns>
	    /// true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or when an item changes; otherwise, false.
	    /// </returns>
	    public bool SupportsChangeNotification
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).SupportsChangeNotification;
            }
        }

        /// <summary>
        /// Gets whether the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method.
        /// </summary>
        /// <returns>
        /// true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method; otherwise, false.
        /// </returns>
        public bool SupportsSearching
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).SupportsSearching;
            }
        }

        /// <summary>
        /// Gets whether the list supports sorting.
        /// </summary>
        /// <returns>
        /// true if the list supports sorting; otherwise, false.
        /// </returns>
        public bool SupportsSorting
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).SupportsSorting;
            }
        }

        /// <summary>
        /// Gets whether the items in the list are sorted.
        /// </summary>
        /// <returns>
        /// true if <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort"/> has not been called; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public bool IsSorted
	    {
	        get
	        {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).IsSorted;
	        }
	    }

	    /// <summary>
	    /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
	    /// </summary>
	    /// <returns>
	    /// The <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
	    /// </returns>
	    /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
	    public PropertyDescriptor SortProperty
	    {
	        get
	        {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).SortProperty;
	        }
	    }

	    /// <summary>
	    /// Gets the direction of the sort.
	    /// </summary>
	    /// <returns>
	    /// One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.
	    /// </returns>
	    /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
	    public ListSortDirection SortDirection
	    {
	        get
	        {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList).SortDirection;
	        }
	    }

	    /// <summary>
        /// Occurs when the list changes or an item in the list changes.
        /// </summary>
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                InnerList.ListChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                InnerList.ListChanged -= value;
            }
        }

        #endregion

        #region Implementation of ICancelAddNew

        /// <summary>
        /// Discards a pending new item from the collection.
        /// </summary>
        /// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
        public void CancelNew(int itemIndex)
	    {
            CheckForAndThrowIfDisposed();

            InnerList.CancelNew(itemIndex);
	    }

	    /// <summary>
	    /// Commits a pending new item to the collection.
	    /// </summary>
	    /// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
	    public void EndNew(int itemIndex)
	    {
            CheckForAndThrowIfDisposed();

            InnerList.EndNew(itemIndex);
        }

	    #endregion

	    #region Implementation of IRaiseItemChangedEvents

	    /// <summary>
	    /// Gets a value indicating whether the <see cref="T:System.ComponentModel.IRaiseItemChangedEvents"/> object raises <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> events.
	    /// </summary>
	    /// <returns>
	    /// true if the <see cref="T:System.ComponentModel.IRaiseItemChangedEvents"/> object raises <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> events when one of its property values changes; otherwise, false.
	    /// </returns>
	    public bool RaisesItemChangedEvents
	    {
	        get
	        {
                CheckForAndThrowIfDisposed();

	            return InnerList.RaiseListChangedEvents;
	        }
	    }

	    #endregion
	}
}