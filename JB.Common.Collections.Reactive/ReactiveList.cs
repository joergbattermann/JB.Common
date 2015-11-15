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
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using JB.Collections.Reactive.ExtensionMethods;
using JB.ExtensionMethods;

namespace JB.Collections.Reactive
{
    public class ReactiveList<T> : IReactiveList<T>, IDisposable
    {
        protected Subject<IReactiveCollectionChange<T>> ChangesSubject = null;
        private IDisposable _collectionChangesAndResetsPropertyChangeForwarder = null;
        protected Subject<int> CountChangesSubject = null;

        private IDisposable _countChangesPropertyChangeForwarder = null;

        private IDisposable _innerListChangedForwader = null;

        protected Subject<Exception> ThrownExceptionsSubject = null;

        /// <summary>
        ///     Gets the inner list.
        /// </summary>
        /// <value>
        ///     The inner list.
        /// </value>
        protected SynchronizedBindingList<T> InnerList { get; }

        /// <summary>
        ///     Gets the used scheduler.
        /// </summary>
        /// <value>
        ///     The scheduler.
        /// </value>
        protected IScheduler Scheduler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveList{T}" /> class.
        /// </summary>
        /// <param name="list">The initial list, if any.</param>
        /// <param name="syncRoot">The object used to synchronize access to the thread-safe collection.</param>
        /// <param name="scheduler">The scheduler to raise events on.</param>
        public ReactiveList(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
        {
            SyncRoot = syncRoot ?? new object();
            Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.Default;

            InnerList = new SynchronizedBindingList<T>(list, SyncRoot);

            ThresholdAmountWhenItemChangesAreNotifiedAsReset = 100;

            IsTrackingCollectionChanges = true;

            IsNotifyingAboutItemChanges = true;
            IsNotifyingAboutCountChanges = true;
            IsTrackingResets = true;

            SetupObservablesAndSubjects();
        }

        /// <summary>
        ///     Gets a value indicating whether the instance is read-only.
        /// </summary>
        /// <returns>
        ///     true if the instance is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsReadOnly;
            }
        }

        #region Implementation of IRaiseItemChangedEvents

        /// <summary>
        ///     Gets a value indicating whether the this instance forwards the inner Items'
        ///     <see cref="INotifyPropertyChanged.PropertyChanged" /> events as corresponding ItemChanged events.
        ///     Obviously only works if the
        ///     <typeparam name="T">type</typeparam>
        ///     does implement the <see cref="INotifyPropertyChanged" /> interface.
        /// </summary>
        /// <returns>
        ///     [true] if the items property changed events are forwarded as ItemChanged ones, [false] if not.
        /// </returns>
        public bool RaisesItemChangedEvents
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return ((IRaiseItemChangedEvents) InnerList).RaisesItemChangedEvents;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the number of elements contained in this instance.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in this instance.
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
        ///     Determines whether the amount of changed items is greater than the reset threshold and / or the minimum amount of
        ///     items to be considered as a reset.
        /// </summary>
        /// <param name="affectedItemsCount">The items changed / affected.</param>
        /// <param name="maximumAmountOfItemsChangedToBeConsideredResetThreshold">
        ///     The maximum amount of changed items count to
        ///     consider a change or a range of changes a reset.
        /// </param>
        /// <returns></returns>
        private bool IsItemsChangedAmountGreaterThanResetThreshold(int affectedItemsCount, int maximumAmountOfItemsChangedToBeConsideredResetThreshold)
        {
            if (affectedItemsCount <= 0) throw new ArgumentOutOfRangeException(nameof(affectedItemsCount));
            if (maximumAmountOfItemsChangedToBeConsideredResetThreshold < 0) throw new ArgumentOutOfRangeException(nameof(maximumAmountOfItemsChangedToBeConsideredResetThreshold));

            // check for '0' thresholds
            if (maximumAmountOfItemsChangedToBeConsideredResetThreshold == 0)
                return true;

            return affectedItemsCount >= maximumAmountOfItemsChangedToBeConsideredResetThreshold;
        }

        /// <summary>
        ///     Notifies all <see cref="CollectionChanges" /> and <see cref="Resets" /> subscribes and
        ///     raises the list- and (reactive)collection changed events.
        /// </summary>
        /// <param name="reactiveCollectionChange">The reactive collection change.</param>
        private void NotifySubscribersAndRaiseListAndCollectionChangedEvents(IReactiveCollectionChange<T> reactiveCollectionChange)
        {
            if (reactiveCollectionChange == null) throw new ArgumentNullException(nameof(reactiveCollectionChange));

            CheckForAndThrowIfDisposed();

            // go ahead and check whether a Reset or item add, -change, -move or -remove shall be signaled
            // .. based on the ThresholdAmountWhenItemChangesAreNotifiedAsReset value
            var actualReactiveCollectionChange = (reactiveCollectionChange.ChangeType == ReactiveCollectionChangeType.Reset || IsItemsChangedAmountGreaterThanResetThreshold(1, ThresholdAmountWhenItemChangesAreNotifiedAsReset))
                ? ReactiveCollectionChange<T>.Reset
                : reactiveCollectionChange;

            // raise events and notifiy about collection/list changes
            try
            {
                ChangesSubject.OnNext(actualReactiveCollectionChange);
            }
            catch (Exception exception)
            {
                ThrownExceptionsSubject.OnNext(exception);
            }

            if (actualReactiveCollectionChange.ChangeType == ReactiveCollectionChangeType.ItemAdded
                || actualReactiveCollectionChange.ChangeType == ReactiveCollectionChangeType.ItemRemoved
                || actualReactiveCollectionChange.ChangeType == ReactiveCollectionChangeType.Reset)
            {
                try
                {
                    CountChangesSubject.OnNext(Count);
                }
                catch (Exception exception)
                {
                    ThrownExceptionsSubject.OnNext(exception);
                }
            }

            try
            {
                RaiseCollectionChanged(actualReactiveCollectionChange.ToNotifyCollectionChangedEventArgs());
            }
            catch (Exception exception)
            {
                ThrownExceptionsSubject.OnNext(exception);
            }

            try
            {
                RaiseReactiveCollectionChanged(new ReactiveCollectionChangedEventArgs<T>(actualReactiveCollectionChange));
            }
            catch (Exception exception)
            {
                ThrownExceptionsSubject.OnNext(exception);
            }
        }

        /// <summary>
        ///     Setups the observables.
        /// </summary>
        private void SetupObservablesAndSubjects()
        {
            // prepare subjects for RX
            ThrownExceptionsSubject = new Subject<Exception>();
            ChangesSubject = new Subject<IReactiveCollectionChange<T>>();
            CountChangesSubject = new Subject<int>();

            // then connect to InnerList's ListChanged Event
            _innerListChangedForwader = Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
                handler => InnerList.ListChanged += handler,
                handler => InnerList.ListChanged -= handler)
                .TakeWhile(_ => !IsDisposing && !IsDisposed)
                .SkipWhile(_ => !IsTrackingCollectionChanges)
                .Where(eventPattern => eventPattern != null && eventPattern.EventArgs != null)
                .Select(eventPattern => eventPattern.EventArgs.ToReactiveCollectionChange(this))
                .Subscribe(NotifySubscribersAndRaiseListAndCollectionChangedEvents);


            // 'Count' and 'Item[]' PropertyChanged events are used by WPF typically via / for ObservableCollections, see
            // http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs,421

#pragma warning disable S3236 // These two explicit RaisePropertyChanged parameters are used as intended
            _countChangesPropertyChangeForwarder = CountChanges.Subscribe(_ => RaisePropertyChanged("Count"));
            _collectionChangesAndResetsPropertyChangeForwarder = CollectionChanges.Subscribe(_ => RaisePropertyChanged("Item[]"));
#pragma warning restore S3236 // These two explicit RaisePropertyChanged parameters are used as intended
        }

        #region Implementation of INotifyCollectionChanged

        /// <summary>
        ///     The actual <see cref="CollectionChanged" /> event.
        /// </summary>
        private NotifyCollectionChangedEventHandler _collectionChanged;


        /// <summary>
        ///     Occurs when the collection changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _collectionChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _collectionChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="notifyCollectionChangedEventArgs">
        ///     The
        ///     <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null) throw new ArgumentNullException(nameof(notifyCollectionChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            Scheduler.Schedule(() => _collectionChanged?.Invoke(this, notifyCollectionChangedEventArgs));
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            CheckForAndThrowIfDisposed();

            return InnerList.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out T>

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public void Add(T item)
        {
            CheckForAndThrowIfDisposed();

            InnerList.Add(item);
        }

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
        public int Add(object value)
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
        public bool Contains(object value)
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
        public int IndexOf(object value)
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
        public void Insert(int index, object value)
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
        public void Remove(object value)
        {
            CheckForAndThrowIfDisposed();

            ((IList) InnerList).Remove(value);
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.IList" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the
        ///     <see cref="T:System.Collections.IList" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList" /> is read-only.-or- The
        ///     <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </exception>
        void IList.RemoveAt(int index)
        {
            CheckForAndThrowIfDisposed();

            ((IList) InnerList).RemoveAt(index);
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
        public bool IsFixedSize
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsFixedSize;
            }
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public void Clear()
        {
            CheckForAndThrowIfDisposed();

            InnerList.Clear();
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item)
        {
            CheckForAndThrowIfDisposed();
            return InnerList.Contains(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The number of elements in the source
        ///     <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from
        ///     <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CheckForAndThrowIfDisposed();

            InnerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
        ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public bool Remove(T item)
        {
            CheckForAndThrowIfDisposed();

            return InnerList.Remove(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based
        ///     indexing.
        /// </param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="array" /> is multidimensional.-or- The number of elements
        ///     in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from
        ///     <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source
        ///     <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination
        ///     <paramref name="array" />.
        /// </exception>
        public void CopyTo(Array array, int index)
        {
            CheckForAndThrowIfDisposed();

            ((IList) InnerList).CopyTo(array, index);
        }

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>
        ///     An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </returns>
        public object SyncRoot { get; }

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
        ///     (thread safe).
        /// </summary>
        /// <returns>
        ///     true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise,
        ///     false.
        /// </returns>
        public bool IsSynchronized
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return ((IList) InnerList).IsSynchronized;
            }
        }

        #endregion

        #region Implementation of IReactiveCollection<T>

        /// <summary>
        ///     Adds a range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            CheckForAndThrowIfDisposed();

            var itemsAsList = items.ToList();

            if (itemsAsList.Count == 0)
                return;

            // only use the Suppress & Reset mechanism if possible
            var suppressItemChangesWhileAdding =
                IsItemsChangedAmountGreaterThanResetThreshold(itemsAsList.Count, ThresholdAmountWhenItemChangesAreNotifiedAsReset)
                && IsTrackingCollectionChanges;

            // we use an IDisposable either way, but in case of not sending a reset, an empty Disposable will be used to simplify the logic here
            using (suppressItemChangesWhileAdding ? SuppressCollectionChangedNotifications(true) : Disposable.Empty)
            {
                var originalRaiseListChangedEvents = InnerList.RaiseListChangedEvents;
                try
                {
                    InnerList.RaiseListChangedEvents = !suppressItemChangesWhileAdding;
                    InnerList.AddRange(itemsAsList);
                }
                finally
                {
                    InnerList.RaiseListChangedEvents = originalRaiseListChangedEvents;
                }
            }
        }

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
        public void Move(T item, int newIndex, bool correctNewIndexOnIndexShift = true)
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
        public void Move(int itemIndex, int newIndex, bool correctNewIndexOnIndexShift = true)
        {
            if (itemIndex < 0 || itemIndex >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
            if (newIndex < 0 || newIndex >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));

            CheckForAndThrowIfDisposed();

            InnerList.Move(itemIndex, newIndex, correctNewIndexOnIndexShift);
        }

        /// <summary>
        ///     Removes the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            CheckForAndThrowIfDisposed();

            var itemsAsList = items.ToList();

            if (itemsAsList.Count == 0)
                return;

            // only use the Suppress & Reset mechanism if possible
            var suppressionItemChanges =
                IsItemsChangedAmountGreaterThanResetThreshold(itemsAsList.Count, ThresholdAmountWhenItemChangesAreNotifiedAsReset)
                && IsTrackingCollectionChanges;

            // we use an IDisposable either way, but in case of not sending a reset, an empty Disposable will be used to simplify the logic here
            using (suppressionItemChanges ? SuppressCollectionChangedNotifications(true) : Disposable.Empty)
            {
                var originalRaiseListChangedEvents = InnerList.RaiseListChangedEvents;
                try
                {
                    InnerList.RaiseListChangedEvents = !suppressionItemChanges;
                    InnerList.RemoveRange(itemsAsList);
                }
                finally
                {
                    InnerList.RaiseListChangedEvents = originalRaiseListChangedEvents;
                }
            }
        }

        /// <summary>
        ///     Resets this instance.
        /// </summary>
        public void Reset()
        {
            CheckForAndThrowIfDisposed();

            InnerList.ResetBindings();
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
        public int IndexOf(T item)
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
        public void Insert(int index, T item)
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
        void IList<T>.RemoveAt(int index)
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

                InnerList[index] = value;
            }
        }

        #endregion

        #region Implementation of INotifyReactiveCollectionChanged<out T>

        private readonly object _isTrackingCollectionChangesLocker = new object();
        private long _isTrackingCollectionChanges = 0;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is tracking and notifying about all collection changes.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking and notifying about collection changes; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingCollectionChanges
        {
            get { return Interlocked.Read(ref _isTrackingCollectionChanges) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingCollectionChangesLocker)
                {
                    if (value == false && IsTrackingCollectionChanges == false)
                        throw new InvalidOperationException("A Collection Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingCollectionChanges, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     (Temporarily) suppresses change notifications for (single) item changes until the returned
        ///     <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsNotifyingAboutItemChanges = false;

            return Disposable.Create(() =>
            {
                IsNotifyingAboutItemChanges = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        private readonly object _isItemChangeTrackingEnabledLocker = new object();
        private long _isItemChangeTrackingEnabled = 0;

        /// <summary>
        ///     Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        ///     <typeparam name="T" />
        ///     's <see cref="INotifyPropertyChanged.PropertyChanged" /> events, if the interface is implemented.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotifyingAboutItemChanges
        {
            get { return Interlocked.Read(ref _isItemChangeTrackingEnabled) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isItemChangeTrackingEnabledLocker)
                {
                    if (value == false && IsNotifyingAboutItemChanges == false)
                        throw new InvalidOperationException("An Item Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isItemChangeTrackingEnabled, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public IDisposable SuppressCollectionChangedNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingCollectionChanges = false;

            return Disposable.Create(() =>
            {
                IsTrackingCollectionChanges = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        private readonly object _isTrackingResetsLocker = new object();
        private long _isTrackingResets = 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is tracking resets.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackingResets
        {
            get { return Interlocked.Read(ref _isTrackingResets) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingResetsLocker)
                {
                    if (value == false && IsTrackingResets == false)
                        throw new InvalidOperationException("A Reset(s) Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingResets, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     (Temporarily) suppresses change notifications for <see cref="ReactiveCollectionChangeType.Reset" /> events until
        ///     the returned <see cref="IDisposable" />
        ///     has been Disposed and a Reset will be signaled.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsTrackingResets = false;

            return Disposable.Create(() =>
            {
                IsTrackingResets = true;

                if (signalResetWhenFinished)
                {
                    InnerList.ResetBindings();
                }
            });
        }

        private readonly object _isTrackingCountChangesLocker = new object();
        private long _isTrackingCountChanges = 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is tracking <see cref="IReadOnlyCollection{T}.Count" /> changes.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotifyingAboutCountChanges
        {
            get { return Interlocked.Read(ref _isTrackingCountChanges) == 1; }
            protected set
            {
                CheckForAndThrowIfDisposed();

                lock (_isTrackingCountChangesLocker)
                {
                    if (value == false && IsNotifyingAboutCountChanges == false)
                        throw new InvalidOperationException("A Count Change(s) Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.");

                    // First set marker here to prevent re-entry
                    Interlocked.Exchange(ref _isTrackingCountChanges, value ? 1 : 0);

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     (Temporarily) suppresses item count change notification until the returned <see cref="IDisposable" />
        ///     has been Disposed.
        /// </summary>
        /// <param name="signalCurrentCountWhenFinished">
        ///     if set to <c>true</c> signals a the <see cref="IReadOnlyCollection{T}.Count" />
        ///     when finished.
        /// </param>
        /// <returns></returns>
        public IDisposable SuppressCountChangedNotifications(bool signalCurrentCountWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            IsNotifyingAboutCountChanges = false;

            return Disposable.Create(() =>
            {
                IsNotifyingAboutCountChanges = true;

                if (signalCurrentCountWhenFinished)
                {
                    CountChangesSubject.OnNext(Count);
                }
            });
        }

        /// <summary>
        ///     Gets the threshold amount (inclusive) of item changes to be signaled as a
        ///     <see cref="ReactiveCollectionChangeType.Reset" />
        ///     rather than individual <see cref="ReactiveCollectionChangeType" /> notifications.
        /// </summary>
        /// <value>
        ///     The threshold amount of item changes to be signal a reset rather than item change(s).
        /// </value>
        public int ThresholdAmountWhenItemChangesAreNotifiedAsReset
        {
            get { return _thresholdAmountWhenItemChangesAreNotifiedAsReset; }
            set
            {
                _thresholdAmountWhenItemChangesAreNotifiedAsReset = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the collection change notifications as an observable stream.
        /// </summary>
        /// <value>
        ///     The collection changes.
        /// </value>
        public IObservable<IReactiveCollectionChange<T>> CollectionChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return ChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhile(change => IsTrackingCollectionChanges == false)
                    .SkipWhile(change => change.ChangeType == ReactiveCollectionChangeType.ItemChanged && IsNotifyingAboutItemChanges == false)
                    .SkipWhile(change => change.ChangeType == ReactiveCollectionChangeType.Reset && IsTrackingResets == false);
            }
        }

        /// <summary>
        ///     Gets the observable streams of (<see cref="INotifyPropertyChanged" /> implementing) items inside this collection
        ///     that have changed.
        /// </summary>
        /// <value>
        ///     The item changes.
        /// </value>
        public IObservable<IReactiveCollectionChange<T>> CollectionItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CollectionChanges
                    .Where(change => change.ChangeType == ReactiveCollectionChangeType.ItemChanged)
                    .SkipWhile(_ => IsNotifyingAboutItemChanges == false);
            }
        }

        /// <summary>
        ///     Gets the count change notifications as an observable stream.
        /// </summary>
        /// <value>
        ///     The count changes.
        /// </value>
        public IObservable<int> CountChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CountChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipWhile(_ => IsNotifyingAboutCountChanges == false)
                    .DistinctUntilChanged();
            }
        }

        /// <summary>
        ///     Gets the reset notifications as an observable stream.  Whenever signaled,
        ///     observers should reset any knowledge / state etc about the list.
        /// </summary>
        /// <value>
        ///     The resets.
        /// </value>
        public IObservable<Unit> Resets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return CollectionChanges
                    .Where(change => change.ChangeType == ReactiveCollectionChangeType.Reset)
                    .SkipWhile(_ => IsTrackingResets == false)
                    .Select(_ => Unit.Default);
            }
        }

        /// <summary>
        ///     Gets the thrown exceptions for the <see cref="INotifyReactiveCollectionChanged{T}.CollectionChanges" /> stream.
        ///     Ugly, but oh well.
        /// </summary>
        /// <value>
        ///     The thrown exceptions.
        /// </value>
        public IObservable<Exception> ThrownExceptions
        {
            get
            {
                CheckForAndThrowIfDisposed();

                // not caring about IsDisposing / IsDisposed on purpose once subscribed, so corresponding Exceptions are forwarded 'til the "end" to already existing subscribers
                return ThrownExceptionsSubject;
            }
        }

        /// <summary>
        ///     The actual <see cref="ReactiveCollectionChanged" /> event.
        /// </summary>
        private EventHandler<ReactiveCollectionChangedEventArgs<T>> _reactiveCollectionChanged;

        /// <summary>
        ///     Occurs when the corresponding <see cref="IReactiveCollection{T}" /> changed.
        /// </summary>
        public event EventHandler<ReactiveCollectionChangedEventArgs<T>> ReactiveCollectionChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _reactiveCollectionChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _reactiveCollectionChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:ReactiveCollectionChanged" /> event.
        /// </summary>
        /// <param name="reactiveCollectionChangedEventArgs">
        ///     The <see cref="ReactiveCollectionChangedEventArgs{T}" /> instance
        ///     containing the event data.
        /// </param>
        protected virtual void RaiseReactiveCollectionChanged(ReactiveCollectionChangedEventArgs<T> reactiveCollectionChangedEventArgs)
        {
            if (reactiveCollectionChangedEventArgs == null) throw new ArgumentNullException(nameof(reactiveCollectionChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            Scheduler.Schedule(() => _reactiveCollectionChanged?.Invoke(this, reactiveCollectionChangedEventArgs));
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        ///     The actual <see cref="PropertyChanged" /> event.
        /// </summary>
        private PropertyChangedEventHandler _propertyChanged;

        /// <summary>
        ///     Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _propertyChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _propertyChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (IsDisposed || IsDisposing)
                return;

            Scheduler.Schedule(() => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion

        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();
        private volatile int _thresholdAmountWhenItemChangesAreNotifiedAsReset;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref _isDisposed) == 1; }
            protected set
            {
                lock (_isDisposedLocker)
                {
                    if (value == false && IsDisposed)
                        throw new InvalidOperationException("Once Disposed has been set, it cannot be reset back to false.");

                    Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
                }

                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposing
        {
            get { return Interlocked.Read(ref _isDisposing) == 1; }
            protected set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                if (disposeManagedResources)
                {
                    if (_collectionChangesAndResetsPropertyChangeForwarder != null)
                    {
                        _collectionChangesAndResetsPropertyChangeForwarder.Dispose();
                        _collectionChangesAndResetsPropertyChangeForwarder = null;
                    }

                    if (_countChangesPropertyChangeForwarder != null)
                    {
                        _countChangesPropertyChangeForwarder.Dispose();
                        _countChangesPropertyChangeForwarder = null;
                    }

                    if (CountChangesSubject != null)
                    {
                        CountChangesSubject.Dispose();
                        CountChangesSubject = null;
                    }

                    if (ChangesSubject != null)
                    {
                        ChangesSubject.Dispose();
                        ChangesSubject = null;
                    }


                    if (_innerListChangedForwader != null)
                    {
                        _innerListChangedForwader.Dispose();
                        _innerListChangedForwader = null;
                    }

                    if (ThrownExceptionsSubject != null)
                    {
                        ThrownExceptionsSubject.Dispose();
                        ThrownExceptionsSubject = null;
                    }
                }
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        ///     Checks whether this instance is currently or already has been disposed.
        /// </summary>
        protected virtual void CheckForAndThrowIfDisposed()
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

    }
}