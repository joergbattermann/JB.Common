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
using System.Runtime.CompilerServices;

namespace JB.Collections
{
	public class ReactiveList<T> : IReactiveList<T>
	{
		/// <summary>
		/// Gets the inner list.
		/// </summary>
		/// <value>
		/// The inner list.
		/// </value>
		protected SchedulerSynchronizedBindingList<T> InnerList { get; private set; }

		/// <summary>
		/// Gets the used scheduler.
		/// </summary>
		/// <value>
		/// The scheduler.
		/// </value>
		private IScheduler Scheduler { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ReactiveList{T}"/> class.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="changesToResetThreshold">The <see cref="ChangesToResetThreshold"/> value.</param>
		/// <param name="syncRoot">The synchronize root.</param>
		/// <param name="scheduler">The scheduler.</param>
		public ReactiveList(IList<T> list = null, double changesToResetThreshold = 0.3, object syncRoot = null, IScheduler scheduler = null)
		{
			if(changesToResetThreshold < 0 || changesToResetThreshold > 1) throw new ArgumentOutOfRangeException(nameof(changesToResetThreshold), "Must be between 0 and 1 (both inclusive)");

			SyncRoot = syncRoot ?? new object();
			Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.Default;

			InnerList = new SchedulerSynchronizedBindingList<T>(list, SyncRoot, Scheduler);

			ChangesToResetThreshold = changesToResetThreshold;

			SetupObservables();
		}

		/// <summary>
		/// Setups the observables.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		private void SetupObservables()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determines whether the provided object is one of the given type.
		/// </summary>
		/// <typeparam name="TObject">The type of the object.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private bool IsObjectOf<TObject>(object value)
		{
			return ((value is TObject) || ((value == null) && (default(TObject) == null)));
		}

		/// <summary>
		/// Gets the number of elements contained in this instance.
		/// </summary>
		/// <returns>
		/// The number of elements contained in this instance.
		/// </returns>
		public int Count => InnerList.Count;

		/// <summary>
		/// Gets a value indicating whether the instance is read-only.
		/// </summary>
		/// <returns>
		/// true if the instance is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly => ((IList)InnerList).IsReadOnly;

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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			return ((IList)InnerList).Contains(value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception>
		void IList.Clear()
		{
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
			return ((IList)InnerList).IndexOf(value);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted. </param><param name="value">The object to insert into the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><exception cref="T:System.NullReferenceException"><paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception>
		public void Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
		/// </summary>
		/// <param name="value">The object to remove from the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
		public void Remove(object value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.IList"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
		void IList.RemoveAt(int index)
		{
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
			get { return InnerList[index]; }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.
		/// </returns>
		bool IList.IsReadOnly => IsReadOnly;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.
		/// </returns>
		public bool IsFixedSize => ((IList)InnerList).IsFixedSize;

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
		void ICollection<T>.Clear()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		public bool Contains(T item) => InnerList.Contains(item);

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.-or-The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
		public void CopyTo(Array array, int index)
		{
			((IList)InnerList).CopyTo(array, index);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </returns>
		int ICollection.Count => Count;

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
		public bool IsSynchronized => ((IList)InnerList).IsSynchronized;

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		int ICollection<T>.Count => Count;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		bool ICollection<T>.IsReadOnly => IsReadOnly;

		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		int IReadOnlyCollection<T>.Count => Count;

		#endregion

		#region Implementation of IReactiveCollection<T>

		/// <summary>
		/// Adds a range of items.
		/// </summary>
		/// <param name="items">The items.</param>
		public void Add(IEnumerable<T> items)
		{
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the specified items.
		/// </summary>
		/// <param name="items">The items.</param>
		public void Remove(IEnumerable<T> items)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the amount of items starting at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		public void RemoveRange(int index, int count)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
			throw new NotImplementedException();
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
			return InnerList.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		void IList<T>.RemoveAt(int index)
		{
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
			get { return InnerList[index]; }
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region Implementation of INotifyReactiveCollectionChanged<out T>

		/// <summary>
		/// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
		/// has been Disposed and a Reset will be signaled.
		/// </summary>
		/// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IDisposable SuppressReactiveCollectionChangedNotifications(bool signalResetWhenFinished = true)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Indicates at what percentage / fraction bulk changes are signaled as a Reset rather than individual change()s.
		/// [0] = Always, [1] = Never.
		/// </summary>
		/// <value>
		/// The changes to reset threshold.
		/// </value>
		public double ChangesToResetThreshold { get; }

		/// <summary>
		/// Gets the collection change notifications as an observable stream.
		/// </summary>
		/// <value>
		/// The collection changes.
		/// </value>
		public IObservable<IReactiveCollectionChange<T>> CollectionChanges { get; }

		/// <summary>
		/// Gets the count change notifications as an observable stream.
		/// </summary>
		/// <value>
		/// The count changes.
		/// </value>
		public IObservable<int> CountChanges { get; }

		/// <summary>
		/// Gets the reset notifications as an observable stream.  Whenever signaled,
		/// observers should reset any knowledge / state etc about the list.
		/// </summary>
		/// <value>
		/// The resets.
		/// </value>
		public IObservable<Unit> Resets { get; }

		#endregion

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
			Scheduler.Schedule(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
		}

		#endregion
	}
}