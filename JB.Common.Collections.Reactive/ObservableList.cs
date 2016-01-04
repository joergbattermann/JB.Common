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
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JB.Collections.Reactive.ExtensionMethods;
using JB.ExtensionMethods;
using JB.Reactive.Linq;

namespace JB.Collections.Reactive
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableList<T> : ObservableCollection<T>, IObservableList<T>, IDisposable
    {
        private IDisposable _innerListChangedRelevantListChangedEventsForwader;

        private Subject<IObservableListChange<T>> _listChangesSubject = new Subject<IObservableListChange<T>>();

        /// <summary>
        /// Gets the list changes observer.
        /// </summary>
        /// <value>
        /// The list changes observer.
        /// </value>
        protected IObserver<IObservableListChange<T>> ListChangesObserver { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableList{T}" /> class.
        /// </summary>
        /// <param name="list">The initial list, if any.</param>
        /// <param name="syncRoot">The object used to synchronize access to the thread-safe collection.</param>
        /// <param name="scheduler">The scheduler to to send out observer messages & raise events on. If none is provided <see cref="System.Reactive.Concurrency.Scheduler.CurrentThread"/> will be used.</param>
        public ObservableList(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
            : base(list, syncRoot, scheduler)
        {
            SetupObservablesAndObserversAndSubjects();
        }

        #region Helpers

        /// <summary>
        /// Prepares and sets up the observables and subjects used, particularly
        /// <see cref="ListChanges"/>, <see cref="INotifyObservableCountChanged.CountChanges"/> and <see cref="INotifyUnhandledObserverExceptions.UnhandledObserverExceptions"/>.
        /// </summary>
        private void SetupObservablesAndObserversAndSubjects()
        {
            ListChangesObserver = _listChangesSubject.NotifyOn(Scheduler);

            // then connect to InnerList's ListChanged Event
            _innerListChangedRelevantListChangedEventsForwader = Observable.FromEventPattern<ListChangedEventHandler, ListChangedEventArgs>(
                handler => InnerList.ListChanged += handler,
                handler => InnerList.ListChanged -= handler)
                .TakeWhile(_ => !IsDisposing && !IsDisposed)
                .SkipContinuouslyWhile(_ => !IsTrackingChanges)
                .Where(eventPattern => eventPattern?.EventArgs != null)
                .Select(eventPattern => eventPattern.EventArgs.ToObservableListChange(InnerList))
                .ObserveOn(Scheduler)
                .Subscribe(
                    NotifySubscribersAboutListChanges,
                    exception =>
                    {
                        UnhandledObserverExceptionsObserver.OnNext(exception);
                        // ToDo: at this point this instance is practically doomed / no longer forwarding any events & therefore further usage of the instance itself should be prevented, or the observable stream should re-connect/signal-and-swallow exceptions. Either way.. not ideal.
                    });
        }

        /// <summary>
        ///     Notifies all <see cref="ListChanges" /> and <see cref="INotifyObservableResets.Resets" /> subscribers and
        ///     raises the (observable)collection changed events.
        /// </summary>
        /// <param name="observableListChange">The observable list change.</param>
        protected virtual void NotifySubscribersAboutListChanges(IObservableListChange<T> observableListChange)
        {
            // This is similar to what ObservableCollection implements via its NotifyObserversAboutCollectionChanges method,
            // however:
            // - no need to handle count-relevant changes because the underlying ObservableCollection takes care of this
            // - no (extra) (Raise)CollectionChanged call here, again.. already done by the ObservableCollection
            // - however as 'Move's are only possible for / with ObservableLists, we also raise a PropertyChangedEvent for 'Item[]' (for wpf) in case of a item move(s)

            if (observableListChange == null)
                throw new ArgumentNullException(nameof(observableListChange));

            CheckForAndThrowIfDisposed();

            // go ahead and check whether a Reset or item add, -change, -move or -remove shall be signaled
            // .. based on the ThresholdAmountWhenItemChangesAreNotifiedAsReset value
            var actualObservableListChange =
                (observableListChange.ChangeType == ObservableListChangeType.Reset
                 || IsItemsChangedAmountGreaterThanResetThreshold(1, ThresholdAmountWhenItemChangesAreNotifiedAsReset))
                    ? ObservableListChange<T>.Reset
                    : observableListChange;

            // raise events and notify about list changes
            try
            {
                ListChangesObserver.OnNext(actualObservableListChange);
            }
            catch (Exception exception)
            {
                UnhandledObserverExceptionsObserver.OnNext(exception);

                if (IsThrowingUnhandledObserverExceptions)
                    throw;
            }

            try
            {
                RaiseObservableListChanged(new ObservableListChangedEventArgs<T>(actualObservableListChange));
            }
            catch (Exception exception)
            {
                UnhandledObserverExceptionsObserver.OnNext(exception);

                if (IsThrowingUnhandledObserverExceptions)
                    throw;
            }

            if (actualObservableListChange.ChangeType == ObservableListChangeType.ItemMoved)
            {
                try
                {
                    RaisePropertyChanged(ItemIndexerName);
                }
                catch (Exception exception)
                {
                    UnhandledObserverExceptionsObserver.OnNext(exception);

                    if (IsThrowingUnhandledObserverExceptions)
                        throw;
                }
            }
        }
        #endregion

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

        #region Implementation of IDisposable

        #region Overrides of ObservableCollection<T>

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;
            try
            {
                IsDisposing = true;

                Dispose(true);
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;

                GC.SuppressFinalize(this);
            }
        }

        #endregion

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                if (_innerListChangedRelevantListChangedEventsForwader != null)
                {
                    _innerListChangedRelevantListChangedEventsForwader.Dispose();
                    _innerListChangedRelevantListChangedEventsForwader = null;
                }

                var listChangesObserverAsDisposable = ListChangesObserver as IDisposable;
                listChangesObserverAsDisposable?.Dispose();
                ListChangesObserver = null;

                if (_listChangesSubject != null)
                {
                    _listChangesSubject.Dispose();
                    _listChangesSubject = null;
                }
            }

            base.Dispose(disposeManagedResources);
        }

        #endregion

        #region Implementation of INotifyObservableListChanged<T>

        /// <summary>
        /// Gets the list changes as an observable stream.
        /// This, contrary to <see cref="INotifyObservableCollectionChanged{T}.CollectionChanges"/>
        /// also notifies about move operations inside the underlying list of items and provides index positions
        /// per change event.
        /// </summary>
        /// <value>
        /// The list changes.
        /// </value>
        public IObservable<IObservableListChange<T>> ListChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();
                
                return _listChangesSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .SkipContinuouslyWhile(change => !IsTrackingChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableListChangeType.ItemChanged&& !IsTrackingItemChanges)
                    .SkipContinuouslyWhile(change => change.ChangeType == ObservableListChangeType.Reset && !IsTrackingResets);
            }
        }

        /// <summary>
        ///     The actual <see cref="ObservableListChanged" /> event.
        /// </summary>
        private EventHandler<ObservableListChangedEventArgs<T>> _observableListChanged;

        /// <summary>
        /// Occurs when the corresponding <see cref="T:JB.Collections.Reactive.IObservableList`1" /> changed.
        /// This, contrary to <see cref="INotifyObservableCollectionChanged{T}.ObservableCollectionChanged"/>
        /// also notifies about move operations inside the underlying list of items and provides index positions
        /// per change event.
        /// </summary>
        public event EventHandler<ObservableListChangedEventArgs<T>> ObservableListChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();
                _observableListChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();
                _observableListChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:ObservableListChanged" /> event.
        /// </summary>
        /// <param name="observableListChangedEventArgs">
        ///     The <see cref="ObservableListChangedEventArgs{T}" /> instance
        ///     containing the event data.
        /// </param>
        protected virtual void RaiseObservableListChanged(ObservableListChangedEventArgs<T> observableListChangedEventArgs)
        {
            if (observableListChangedEventArgs == null) throw new ArgumentNullException(nameof(observableListChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            // only raise event if it's currently allowed
            if (!IsTrackingChanges
                || (observableListChangedEventArgs.ChangeType == ObservableListChangeType.ItemChanged && !IsTrackingItemChanges)
                || (observableListChangedEventArgs.ChangeType == ObservableListChangeType.Reset && !IsTrackingResets))
            {
                return;
            }

            var eventHandler = _observableListChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, observableListChangedEventArgs));
            }
        }

        #endregion
    }
}