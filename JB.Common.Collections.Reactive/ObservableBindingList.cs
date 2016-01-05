using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using JB.Collections.Reactive.ExtensionMethods;
using JB.Reactive;

namespace JB.Collections.Reactive
{
    public class ObservableBindingList<T> : ObservableList<T>, IObservableBindingList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableBindingList{T}" /> class.
        /// </summary>
        /// <param name="list">The initial list, if any.</param>
        /// <param name="syncRoot">The object used to synchronize access to the thread-safe collection.</param>
        /// <param name="scheduler">The scheduler to raise events on.</param>
        public ObservableBindingList(IList<T> list = null, object syncRoot = null, IScheduler scheduler = null)
            : base(list, syncRoot, scheduler)
        {
            ObservableListChanged += ObservableListChangedAsListChangedForwarder;
        }

        /// <summary>
        /// Handles the ObservableCollectionChanged event of the underlying <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="observableListChangedEventArgs">The <see cref="ObservableListChangedEventArgs{T}"/> instance containing the event data.</param>
        private void ObservableListChangedAsListChangedForwarder(object sender, ObservableListChangedEventArgs<T> observableListChangedEventArgs)
        {
            try
            {
                RaiseListChanged(observableListChangedEventArgs.Change.ToListChangedEventArgs());
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException(
                    $"An error occured notifying {nameof(ListChanged)} subscribers of this {this.GetType().Name}.",
                    exception);

                UnhandledObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
        }

        #region Overrides of ObservableList<T>

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposeManagedResources)
        {
            ObservableListChanged -= ObservableListChangedAsListChangedForwarder;

            base.Dispose(disposeManagedResources);
        }

        #endregion

        #region Implementation of IBindingList

        /// <summary>
        ///     Adds a new item to the list.
        /// </summary>
        /// <returns>
        ///     The item added to the list.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.AllowNew" /> is false. </exception>
        public virtual object AddNew()
        {
            return (InnerList as IBindingList)?.AddNew();
        }

        /// <summary>
        ///     Adds the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to the indexes used for searching.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to add to the indexes used for
        ///     searching.
        /// </param>
        public virtual void AddIndex(PropertyDescriptor property)
        {
            (InnerList as IBindingList)?.AddIndex(property);
        }

        /// <summary>
        ///     Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor" /> and a
        ///     <see cref="T:System.ComponentModel.ListSortDirection" />.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to sort by. </param>
        /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values. </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is
        ///     false.
        /// </exception>
        public virtual void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            (InnerList as IBindingList)?.ApplySort(property, direction);
        }

        /// <summary>
        ///     Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.
        /// </summary>
        /// <returns>
        ///     The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.
        /// </returns>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to search on. </param>
        /// <param name="key">The value of the <paramref name="property" /> parameter to search for. </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSearching" />
        ///     is false.
        /// </exception>
        public virtual int Find(PropertyDescriptor property, object key)
        {
            return (InnerList as IBindingList)?.Find(property, key) ?? -1;
        }

        /// <summary>
        ///     Removes the <see cref="T:System.ComponentModel.PropertyDescriptor" /> from the indexes used for searching.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to remove from the indexes used
        ///     for searching.
        /// </param>
        public virtual void RemoveIndex(PropertyDescriptor property)
        {
            (InnerList as IBindingList)?.RemoveIndex(property);
        }

        /// <summary>
        ///     Removes any sort applied using
        ///     <see
        ///         cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" />
        ///     .
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is
        ///     false.
        /// </exception>
        public virtual void RemoveSort()
        {
            (InnerList as IBindingList)?.RemoveSort();
        }

        /// <summary>
        ///     Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew" />.
        /// </summary>
        /// <returns>
        ///     true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew" />; otherwise,
        ///     false.
        /// </returns>
        public virtual bool AllowNew
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowNew;
            }
        }

        /// <summary>
        ///     Gets whether you can update items in the list.
        /// </summary>
        /// <returns>
        ///     true if you can update the items in the list; otherwise, false.
        /// </returns>
        public virtual bool AllowEdit
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowEdit;
            }
        }

        /// <summary>
        ///     Gets whether you can remove items from the list, using
        ///     <see cref="M:System.Collections.IList.Remove(System.Object)" /> or
        ///     <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />.
        /// </summary>
        /// <returns>
        ///     true if you can remove items from the list; otherwise, false.
        /// </returns>
        public virtual bool AllowRemove
        {
            get
            {
                CheckForAndThrowIfDisposed();
                return InnerList.AllowRemove;
            }
        }

        /// <summary>
        ///     Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event is raised when the list
        ///     changes or an item in the list changes.
        /// </summary>
        /// <returns>
        ///     true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event is raised when the list changes or
        ///     when an item changes; otherwise, false.
        /// </returns>
        public virtual bool SupportsChangeNotification
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.SupportsChangeNotification ?? false;
            }
        }

        /// <summary>
        ///     Gets whether the list supports searching using the
        ///     <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" />
        ///     method.
        /// </summary>
        /// <returns>
        ///     true if the list supports searching using the
        ///     <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" />
        ///     method; otherwise, false.
        /// </returns>
        public virtual bool SupportsSearching
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.SupportsSearching ?? false;
            }
        }

        /// <summary>
        ///     Gets whether the list supports sorting.
        /// </summary>
        /// <returns>
        ///     true if the list supports sorting; otherwise, false.
        /// </returns>
        public virtual bool SupportsSorting
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.SupportsSorting ?? false;
            }
        }

        /// <summary>
        ///     Gets whether the items in the list are sorted.
        /// </summary>
        /// <returns>
        ///     true if
        ///     <see
        ///         cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" />
        ///     has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort" /> has not been called; otherwise,
        ///     false.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is
        ///     false.
        /// </exception>
        public virtual bool IsSorted
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.IsSorted ?? false;
            }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> that is being used for sorting.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.ComponentModel.PropertyDescriptor" /> that is being used for sorting.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is
        ///     false.
        /// </exception>
        public virtual PropertyDescriptor SortProperty
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.SortProperty;
            }
        }

        /// <summary>
        ///     Gets the direction of the sort.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is
        ///     false.
        /// </exception>
        public virtual ListSortDirection SortDirection
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return (InnerList as IBindingList)?.SortDirection ?? default(ListSortDirection);
            }
        }

        /// <summary>
        ///     The actual <see cref="ListChanged" /> event.
        /// </summary>
        private ListChangedEventHandler _listChanged;

        /// <summary>
        ///     Occurs when the list changes or an item in the list changes.
        /// </summary>
        public virtual event ListChangedEventHandler ListChanged
        {
            add
            {
                CheckForAndThrowIfDisposed();

                _listChanged += value;
            }
            remove
            {
                CheckForAndThrowIfDisposed();

                _listChanged -= value;
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:ListChanged" /> event.
        /// </summary>
        /// <param name="listChangedEventArgs">
        ///     The <see cref="System.ComponentModel.ListChangedEventArgs" /> instance containing
        ///     the event data.
        /// </param>
        protected virtual void RaiseListChanged(ListChangedEventArgs listChangedEventArgs)
        {
            if (listChangedEventArgs == null)
                throw new ArgumentNullException(nameof(listChangedEventArgs));

            if (IsDisposed || IsDisposing)
                return;

            if (!RaisesListChangedEvents)
                return;

            var eventHandler = _listChanged;
            if (eventHandler != null)
            {
                Scheduler.Schedule(() => eventHandler.Invoke(this, listChangedEventArgs));
            }
        }

        #endregion

        #region Implementation of ICancelAddNew

        /// <summary>
        ///     Discards a pending new item from the collection.
        /// </summary>
        /// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
        public virtual void CancelNew(int itemIndex)
        {
            CheckForAndThrowIfDisposed();

            InnerList.CancelNew(itemIndex);
        }

        /// <summary>
        ///     Commits a pending new item to the collection.
        /// </summary>
        /// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
        public virtual void EndNew(int itemIndex)
        {
            CheckForAndThrowIfDisposed();

            InnerList.EndNew(itemIndex);
        }

        #endregion
        
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
        public virtual bool RaisesItemChangedEvents
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return ((IRaiseItemChangedEvents)InnerList).RaisesItemChangedEvents;
            }
        }

        #endregion

        #region Implementation of IObservableBindingList<T>

        /// <summary>
        ///     Gets a value indicating whether this instance is currently notifying event and observable subscribers about
        ///     collection changed events.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is notifying observable and event subscribers; otherwise, <c>false</c>.
        /// </value>
        public virtual bool RaisesListChangedEvents
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return IsTrackingChanges;
            }
        }

        /// <summary>
        ///     Raises <see cref="INotifyObservableCollectionChanged{T}.ObservableCollectionChanged" />,
        ///     <see cref="INotifyCollectionChanged.CollectionChanged" />
        ///     and <see cref="IBindingList.ListChanged" /> event(s) as well as notifies the
        ///     <see cref="INotifyObservableCollectionChanged{T}.CollectionChanges" />
        ///     and <see cref="INotifyObservableResets.Resets" /> subscribers signalling an entire List / Collection
        ///     Reset.
        /// </summary>
        public virtual void ResetBindings()
        {
            CheckForAndThrowIfDisposed();

            InnerList.ResetBindings();
        }

        /// <summary>
        ///     Raises <see cref="INotifyObservableCollectionChanged{T}.ObservableCollectionChanged" />,
        ///     <see cref="INotifyCollectionChanged.CollectionChanged" />
        ///     and <see cref="IBindingList.ListChanged" /> event(s) as well as notifies the
        ///     <see cref="INotifyObservableCollectionChanged{T}.CollectionChanges" />
        ///     subscribers signalling a single item change event.
        /// </summary>
        /// <param name="index">A zero-based index position of the item to be reset.</param>
        public virtual void ResetItem(int index)
        {
            CheckForAndThrowIfDisposed();

            InnerList.ResetItem(index);
        }

        #endregion
    }
}