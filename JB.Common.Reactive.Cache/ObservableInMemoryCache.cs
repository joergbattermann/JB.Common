// -----------------------------------------------------------------------
// <copyright file="ObservableInMemoryCache.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JB.Collections;
using JB.Collections.Reactive;
using JB.Reactive.Cache.ExtensionMethods;
using JB.Reactive.Linq;

namespace JB.Reactive.Cache
{
    [DebuggerDisplay("Count={Count}")]
    public class ObservableInMemoryCache<TKey, TValue> : IObservableCache<TKey, TValue>, IDisposable
    {
        private Subject<IObservableCacheChange<TKey, TValue>> _cacheChangesSubject = new Subject<IObservableCacheChange<TKey, TValue>>();
        private Subject<ObserverException> _unhandledObserverExceptionsSubject = new Subject<ObserverException>();
        private Subject<ObservableCachedElement<TKey, TValue>> _expiredElementsSubject = new Subject<ObservableCachedElement<TKey, TValue>>();

        private IDisposable _expiredElementsSubscription = null;

        /// <summary>
        /// Gets the cache changes observer.
        /// </summary>
        /// <value>
        /// The cache changes observer.
        /// </value>
        protected IObserver<IObservableCacheChange<TKey, TValue>> CacheChangesObserver { get; private set; }

        /// <summary>
        /// Gets the observer thrown exceptions observer.
        /// </summary>
        /// <value>
        /// The observer thrown exceptions observer.
        /// </value>
        protected IObserver<ObserverException> ObserverExceptionsObserver { get; private set; }

        /// <summary>
        /// Gets the expired elements observer.
        /// </summary>
        /// <value>
        /// The expired elements observer.
        /// </value>
        protected IObserver<ObservableCachedElement<TKey, TValue>> ExpiredElementsObserver => _expiredElementsSubject;

        /// <summary>
        /// Gets or sets the inner dictionary.
        /// </summary>
        /// <value>
        /// The inner dictionary.
        /// </value>
        protected ObservableDictionary<TKey, ObservableCachedElement<TKey, TValue>> InnerDictionary { get; private set; }

        /// <summary>
        /// Gets the default expired elements buffer time span.
        /// </summary>
        /// <value>
        /// The default expired elements buffer time span.
        /// </value>
        protected TimeSpan DefaultExpiredElementsHandlingChillPeriod { get; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Internally expired elements are handled in bulk rather than one by one
        /// and this time span defines how long / large these windows each are. 
        /// </summary>
        /// <value>
        /// The buffer window time span.
        /// </value>
        protected TimeSpan ExpiredElementsHandlingChillPeriod { get; }

        /// <summary>
        /// Gets the scheduler for observer / event notifications.
        /// </summary>
        /// <value>
        /// The scheduler for observer / event notifications.
        /// </value>
        protected IScheduler NotificationScheduler { get; }

        /// <summary>
        /// Gets the expiration scheduler to schedule and handle expirations on.
        /// </summary>
        /// <value>
        /// The expiration scheduler.
        /// </value>
        protected IScheduler ExpirationScheduler { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TKey"/> comparer.
        /// </value>
        protected IEqualityComparer<TKey> KeyComparer { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> implementation to use when comparing values.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TValue"/> comparer.
        /// </value>
        protected IEqualityComparer<TValue> ValueComparer { get; }

        /// <summary>
        /// Gets the single key updater.
        /// </summary>
        /// <value>
        /// The single key updater.
        /// </value>
        protected Func<TKey, TValue> SingleKeyRetrievalAction { get; }

        /// <summary>
        /// Gets the multiple keys updater.
        /// </summary>
        /// <value>
        /// The multiple keys updater.
        /// </value>
        protected Func<IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>> MultipleKeysRetrievalAction { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ObservableInMemoryCache" />.
        /// </summary>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing values.</param>
        /// <param name="singleKeyRetrievalAction">
        ///     The action that will be invoked whenever a single key has expired and has his expiration type set to <see cref="ObservableCacheExpirationType.Update"/>.
        /// </param>
        /// <param name="multipleKeysRetrievalAction">
        ///     The action that will be invoked whenever multiple keys have expired and had their expiration type set to <see cref="ObservableCacheExpirationType.Update"/>.
        ///     This is internally preferred over <paramref name="singleKeyRetrievalAction"/> if more than one element has expired within a given <paramref name="expiredElementsHandlingChillPeriod"/>.
        /// </param>
        /// <param name="expiredElementsHandlingChillPeriod">Expired elements are internally handled every <paramref name="expiredElementsHandlingChillPeriod"/>
        ///     and thereby in bulk rather than the very moment they expire.
        ///     This value allows to specify the time window inbetween each expiration handling process.</param>
        /// <param name="expirationScheduler">
        ///     The <see cref="IScheduler"/> to to schedule and run elements' expiration handling on.
        ///     If none is provided <see>
        ///         <cref>System.Reactive.Concurrency.Scheduler.Default</cref>
        ///     </see>
        ///     will be used.
        /// </param>
        /// <param name="notificationScheduler">
        ///     The <see cref="IScheduler"/> to to send out observer messages and raise events on.
        ///     If none is provided <see>
        ///         <cref>System.Reactive.Concurrency.Scheduler.CurrentThread</cref>
        ///     </see>
        ///     will be used.
        /// </param>
        public ObservableInMemoryCache(
            IEqualityComparer<TKey> keyComparer = null,
            IEqualityComparer<TValue> valueComparer = null,
            Func<TKey, TValue> singleKeyRetrievalAction = null,
            Func<IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>> multipleKeysRetrievalAction = null,
            TimeSpan? expiredElementsHandlingChillPeriod = null,
            IScheduler expirationScheduler = null,
            IScheduler notificationScheduler = null)
        {
            if(expiredElementsHandlingChillPeriod.HasValue && expiredElementsHandlingChillPeriod.Value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiredElementsHandlingChillPeriod), "Must be 0 Ticks or more");

            NotificationScheduler = notificationScheduler ?? Scheduler.CurrentThread;
            ExpirationScheduler = expirationScheduler ?? Scheduler.Default;

            KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            SingleKeyRetrievalAction = singleKeyRetrievalAction;
            MultipleKeysRetrievalAction = multipleKeysRetrievalAction;

            InnerDictionary = new ObservableDictionary<TKey, ObservableCachedElement<TKey, TValue>>(
                keyComparer: KeyComparer,
                valueComparer: new ObservableCachedElementValueEqualityComparer<TKey, TValue>(ValueComparer),
                scheduler: notificationScheduler);

            ThresholdAmountWhenChangesAreNotifiedAsReset = Int32.MaxValue;
            ExpiredElementsHandlingChillPeriod = expiredElementsHandlingChillPeriod ?? DefaultExpiredElementsHandlingChillPeriod;

            SetupObservablesAndObserversAndSubjects();
        }

        #region Helper Methods

        /// <summary>
        /// Prepares and sets up the observables and subjects used, particularly
        /// <see cref="_cacheChangesSubject"/> and <see cref="_unhandledObserverExceptionsSubject"/>.
        /// </summary>
        private void SetupObservablesAndObserversAndSubjects()
        {
            // prepare subjects for RX
            ObserverExceptionsObserver = _unhandledObserverExceptionsSubject.NotifyOn(NotificationScheduler);
            CacheChangesObserver = _cacheChangesSubject.NotifyOn(NotificationScheduler);

            _expiredElementsSubscription = ExpiredElements
                .ObserveOn(ExpirationScheduler)
                .Buffer(ExpiredElementsHandlingChillPeriod, ExpirationScheduler)
                .Where(bufferedElements => bufferedElements != null && bufferedElements.Count > 0)
                .SelectMany(ExpiredElementsToObservable) // this is somewhat of a hack to migrate from RX over to TPL / async handling
                .Subscribe(
                    _ => { }, // nothing to do here per element - it's all done in the .SelectMany() call
                    exception =>
                    {
                        // ToDo: at this point this instance is practically doomed / no longer forwarding any events & therefore further usage of the instance itself should be prevented, or the observable stream should re-connect/signal-and-swallow exceptions. Either way.. not ideal.
                        var observerException = new ObserverException(
                            $"An error occured notifying observers of this {this.GetType().Name} - consistency and future notifications are no longer guaranteed.",
                            exception);
                        ObserverExceptionsObserver.OnNext(observerException);
                    });
        }

        /// <summary>
        /// Converts the <paramref name="expiredElements"/> to an observable stream.
        /// </summary>
        /// <param name="expiredElements">The expired elements.</param>
        /// <returns></returns>
        protected virtual IObservable<Unit> ExpiredElementsToObservable(IList<ObservableCachedElement<TKey, TValue>> expiredElements)
        {
            // ToDo: this needs to be decomposed into smaller functional units.. quite a lot

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    CheckForAndThrowIfDisposed(false);

                    HandleAndNotifyObserversAboutExpiredElements(expiredElements);

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException(
                        $"An error occured handling expired elements of this {this.GetType().Name}.",
                        exception);

                    ObserverExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                    {
                        observer.OnError(exception);
                        throw;
                    }
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Handles and notifies observers about expired elements.
        /// </summary>
        /// <param name="expiredElements">The expired elements.</param>
        /// <returns></returns>
        protected virtual void HandleAndNotifyObserversAboutExpiredElements(IList<ObservableCachedElement<TKey, TValue>> expiredElements)
        {
            CheckForAndThrowIfDisposed(false);

            // return early if the current batch is null/empty
            if (expiredElements == null || expiredElements.Count == 0)
                return;

            // then check which ones are 'still' expired - as these expiration handlings are done in bulk after a timewindow has lapsed, they may have been updated already elsewhere
            var actuallyExpiredElements = expiredElements.Where(element => element.HasExpired).ToList();
            if (actuallyExpiredElements.Count == 0)
                return;

            // then check which of the ones marked as expired are actually still in the cache
            var keysStillInCache = actuallyExpiredElements.Select(element => element.Key).Where(key => InnerDictionary.ContainsKey(key)).ToList();
            var expiredElementsStillInCache = actuallyExpiredElements.Where(element => keysStillInCache.Contains(element.Key, KeyComparer)).ToList();
            if (expiredElementsStillInCache.Count == 0)
                return;
            
            // then go ahead and signal expiration for those filtered down elements to observers
            foreach (var expiredElement in expiredElementsStillInCache)
            {
                try
                {
                    CacheChangesObserver.OnNext(ObservableCacheChange<TKey, TValue>.ItemExpired(
                    expiredElement.Key,
                    expiredElement.Value,
                    expiredElement.ExpiresAt(),
                    expiredElement.ExpirationType));
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException(
                        $"An error occured notifying {nameof(ItemExpirations)} Observers of this {this.GetType().Name} about an item's expiration.",
                        exception);

                    ObserverExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                        throw;
                }
            }
            
            // then split them up by expiry type
            var elementsGroupedByExpirationType = expiredElementsStillInCache.GroupBy(element => element.ExpirationType);
            foreach (var grouping in elementsGroupedByExpirationType)
            {
                var elementsForExpirationType = grouping.ToDictionary(element => element.Key, element => element);
                if (elementsForExpirationType.Count == 0)
                    continue;

                switch (grouping.Key)
                {
                    case ObservableCacheExpirationType.DoNothing:
                    {
                        // keep as is & do nothing in particular
                        break;
                    }
                    case ObservableCacheExpirationType.Remove:
                        {
                            // Using .TryRemove on innerdictionary to remove only those with the same / original value as expired
                            // (to prevent deletion of elements that had changed in the meantime)
                            IDictionary<TKey, ObservableCachedElement<TKey, TValue>> itemsThatCouldNotBeRemoved;
                            InnerDictionary.TryRemoveRange(elementsForExpirationType, out itemsThatCouldNotBeRemoved);

                            foreach (var removedElement in elementsForExpirationType.Except(itemsThatCouldNotBeRemoved))
                            {
                                RemoveFromEventAndNotificationsHandlingAndStopExpiration(removedElement.Value);
                            }

                            break;
                        }
                    case ObservableCacheExpirationType.Update:
                        {
                            if (SingleKeyRetrievalAction == null && MultipleKeysRetrievalAction == null)
                            {
                                throw new InvalidOperationException($"Neither a {nameof(SingleKeyRetrievalAction)} nor {nameof(MultipleKeysRetrievalAction)} has been specified at construction of this instance and therefore {typeof(ObservableCacheExpirationType)} of type {grouping.Key} cannot be handled.");
                            }
                            
                            var elementsStillInCache = elementsForExpirationType
                                .Where(keyValuePair => ((ICollection<KeyValuePair<TKey, ObservableCachedElement<TKey, TValue>>>)InnerDictionary).Contains(keyValuePair))
                                .Select(keyValuePair => keyValuePair.Value)
                                .ToList();

                            if (elementsStillInCache.Count == 0)
                                break;

                            if (elementsStillInCache.Count == 1 && SingleKeyRetrievalAction != null)
                            {
                                var element = elementsStillInCache.FirstOrDefault();
                                if (element == null || !InnerDictionary.ContainsKey(element.Key))
                                    break;

                                UpdateValueForCachedElement(
                                    element,
                                    RetrieveUpdatedValueForSingleElement(element, SingleKeyRetrievalAction),
                                    false);
                            }
                            else
                            {
                                if (MultipleKeysRetrievalAction != null)
                                {
                                    UpdateValuesForCachedElements(
                                        elementsStillInCache,
                                        RetrieveUpdatedValuesForMultipleElements(elementsStillInCache, MultipleKeysRetrievalAction),
                                        false);
                                }
                                else
                                {
                                    foreach (var elementStillInCache in elementsStillInCache)
                                    {
                                        UpdateValueForCachedElement(
                                            elementStillInCache,
                                            RetrieveUpdatedValueForSingleElement(elementStillInCache, SingleKeyRetrievalAction),
                                            false);
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        throw new InvalidOperationException($"The expired elements contain at least one unknown / unhandled {typeof(ObservableCacheExpirationType)}: '{grouping.Key}'");
                }
            }
        }

        /// <summary>
        /// Retrieves the updated value for multiple elements and updates the inner dictionary with it.
        /// </summary>
        /// <param name="existingObservableCachedElement">The existing observable cached element.</param>
        /// <param name="multipleKeysUpdater">The single key updater.</param>
        /// <returns></returns>
        protected virtual IDictionary<TKey, TValue> RetrieveUpdatedValuesForMultipleElements(
            IList<ObservableCachedElement<TKey, TValue>> existingObservableCachedElement,
            Func<IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>> multipleKeysUpdater)
        {
            if (multipleKeysUpdater == null) throw new ArgumentNullException(nameof(multipleKeysUpdater));
            if (existingObservableCachedElement == null) throw new ArgumentNullException(nameof(existingObservableCachedElement));

            var result = new Dictionary<TKey, TValue>();
            if (existingObservableCachedElement.Count == 0)
                return result;

            // else
            var keysForElementsToUpdate = existingObservableCachedElement.Select(element => element.Key).ToList();
            try
            {
                var originalValues = existingObservableCachedElement
                    .ToDictionary(kvp => kvp.Key, kvp => kvp, KeyComparer);

                result = multipleKeysUpdater
                    .Invoke(keysForElementsToUpdate)
                    .Where(kvp => originalValues.ContainsKey(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, KeyComparer);
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException($"An error occured trying to retrieve updated value(s) for {keysForElementsToUpdate.Count} keys.", exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }

            // finally
            return result;
        }

        /// <summary>
        /// Retrieves the updated value for a single element.
        /// </summary>
        /// <param name="existingObservableCachedElement">The existing observable cached element.</param>
        /// <param name="singleKeyUpdater">The single key updater.</param>
        protected virtual TValue RetrieveUpdatedValueForSingleElement(ObservableCachedElement<TKey, TValue> existingObservableCachedElement, Func<TKey, TValue> singleKeyUpdater)
        {
            if (singleKeyUpdater == null) throw new ArgumentNullException(nameof(singleKeyUpdater));
            if (existingObservableCachedElement == null) throw new ArgumentNullException(nameof(existingObservableCachedElement));

            var newValue = default(TValue);
            try
            {
                newValue = singleKeyUpdater.Invoke(existingObservableCachedElement.Key);
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException($"An error occured trying to retrieve an updated value for {existingObservableCachedElement.Key?.ToString() ?? "n.a."}.", exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }

            return newValue;
        }

        /// <summary>
        /// Compares and updates the value if different from the existing one OR only updates the expiration if the value is actually the same.
        /// </summary>
        /// <param name="existingObservableCachedElement">The existing observable cached element.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="keepExistingExpiration">
        ///     If [true] the <paramref name="existingObservableCachedElement"/>'s (remaining) Expiration time will be taken over as it is right now
        ///     for the <paramref name="newValue"/>, if [false], it will be reset to its original value and expiration timing will re-start.
        /// </param>
        private void UpdateValueForCachedElement(ObservableCachedElement<TKey, TValue> existingObservableCachedElement, TValue newValue, bool keepExistingExpiration = true)
        {
            if (existingObservableCachedElement == null)
                throw new ArgumentNullException(nameof(existingObservableCachedElement));

            if (keepExistingExpiration && existingObservableCachedElement.HasExpired)
                throw new KeyHasExpiredException<TKey>(existingObservableCachedElement.Key, existingObservableCachedElement.ExpiresAt());

            try
            {
                if (InnerDictionary.ContainsKey(existingObservableCachedElement.Key) == false)
                {
                    // make sure old / existing element is removed from cache's event handling
                    RemoveFromEventAndNotificationsHandlingAndStopExpiration(existingObservableCachedElement);
                    return;
                }

                // if TimeSpan.MaxValue was originally chosen, make sure it is re-used again rather than the .ExpiresIn() value
                var newOrUpdatedExpiry = (keepExistingExpiration && existingObservableCachedElement.OriginalExpiry < TimeSpan.MaxValue)
                    ? existingObservableCachedElement.ExpiresIn()
                    : existingObservableCachedElement.OriginalExpiry;

                // check whether the actual value has changed
                if (!ValueComparer.Equals(newValue, existingObservableCachedElement.Value))
                {
                    try
                    {
                        // ..  and if so, update the key with the new cached element and thereby value
                        var newObservableCachedElement = new ObservableCachedElement<TKey, TValue>(
                            existingObservableCachedElement.Key,
                            newValue,
                            existingObservableCachedElement.ExpirationType);

                        // and attempt to update it in the inner dictionary
                        var updateResult = InnerDictionary.TryUpdate(
                            existingObservableCachedElement.Key,
                            newObservableCachedElement);

                        if (updateResult == false)
                        {
                            // if the update failed it means the key has 'vanished' in the meantime
                            throw new KeyNotFoundException();
                        }
                        else
                        {
                            // otherwise all good - hooking up the new value to event handling etc
                            AddToEventAndNotificationsHandlingAndStartExpiration(
                                newObservableCachedElement,
                                newOrUpdatedExpiry);
                        }
                    }
                    finally
                    {
                        // make sure old / existing element is removed from cache's event handling
                        RemoveFromEventAndNotificationsHandlingAndStopExpiration(existingObservableCachedElement);
                    }
                }
                else
                {
                    existingObservableCachedElement.StartOrUpdateExpiration(newOrUpdatedExpiry, ExpiredElementsObserver, ObserverExceptionsObserver, ExpirationScheduler);
                }
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException($"An error occured trying to handle {nameof(ObservableCacheExpirationType.Update)} expiration for {existingObservableCachedElement.Key?.ToString() ?? "n.a."}.", exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
        }

        /// <summary>
        /// Compares and updates the values (if different) from the existing cached elements OR
        /// only updates their corresponding expiration values.
        /// </summary>
        /// <param name="existingCachedElements">The existing cached elements.</param>
        /// <param name="updatedCachedElements">The updated elements.</param>
        /// <param name="keepExistingExpiration">
        ///     If [true] the <paramref name="existingCachedElements"/>' (remaining) Expiration time will be taken over as it they are right now
        ///     for the <paramref name="updatedCachedElements"/>, if [false], they will be reset to their original values and expiration timing
        ///     will re-start.
        /// </param>
        private void UpdateValuesForCachedElements(
            IList<ObservableCachedElement<TKey, TValue>> existingCachedElements,
            IDictionary<TKey, TValue> updatedCachedElements,
            bool keepExistingExpiration = true)
        {
            if (existingCachedElements == null) throw new ArgumentNullException(nameof(existingCachedElements));
            if (updatedCachedElements == null) throw new ArgumentNullException(nameof(updatedCachedElements));

            if (keepExistingExpiration)
            {
                var expiredElementsExceptions = existingCachedElements
                    .Where(element => element.HasExpired)
                    .Select(expiredElement => new KeyHasExpiredException<TKey>(expiredElement.Key, expiredElement.ExpiresAt()))
                    .ToList<Exception>();
                if (expiredElementsExceptions.Count == 1)
                    throw expiredElementsExceptions.First();
                if (expiredElementsExceptions.Count > 1)
                    throw new AggregateException($"{expiredElementsExceptions.Count} elements of the provided {existingCachedElements.Count} {nameof(existingCachedElements)} have expired and their Expiration value(s) cannot be kept (Parameter '{nameof(keepExistingExpiration)}' is set to '{keepExistingExpiration}')", expiredElementsExceptions);
            }
           
            var existingCachedElementsWithoutUpdatedValue = new List<ObservableCachedElement<TKey, TValue>>();
            foreach (var existingCachedElement in existingCachedElements)
            {
                try
                {
                    TValue updatedValue;
                    if (updatedCachedElements.TryGetValue(existingCachedElement.Key, out updatedValue))
                    {
                        UpdateValueForCachedElement(existingCachedElement, updatedValue, keepExistingExpiration);
                    }
                    else
                    {
                        existingCachedElementsWithoutUpdatedValue.Add(existingCachedElement);
                    }
                }
                catch (Exception exception)
                {
                    var observerException = new ObserverException($"An error occured trying to handle {nameof(ObservableCacheExpirationType.Update)} expiration for {existingCachedElement.Key?.ToString() ?? "n.a."}.", exception);

                    ObserverExceptionsObserver.OnNext(observerException);

                    if (observerException.Handled == false)
                        throw;
                }
            }

            var existingCachedElementsWithoutUpdatedValuesExceptions = existingCachedElementsWithoutUpdatedValue
                .Select(expiredElement => new KeyNotFoundException<TKey>(expiredElement.Key))
                .ToList<Exception>();

            if (existingCachedElementsWithoutUpdatedValuesExceptions.Count == 1)
                throw existingCachedElementsWithoutUpdatedValuesExceptions.First();
            if (existingCachedElementsWithoutUpdatedValuesExceptions.Count > 1)
                throw new AggregateException($"{existingCachedElementsWithoutUpdatedValuesExceptions.Count} key(s) of the given {existingCachedElements.Count} '{nameof(existingCachedElements)}' had no counterpart in the provided '{nameof(updatedCachedElements)}'", existingCachedElementsWithoutUpdatedValuesExceptions);
        }

        /// <summary>
        /// Adds <see cref="OnCachedElementValuePropertyChanged" /> as event handlers for <paramref name="cachedElement" />'s
        /// <see cref="ObservableCachedElement{TKey,TValue}.ValuePropertyChanged" />.
        /// </summary>
        /// <param name="cachedElement">The value.</param>
        /// <param name="expiry">The expiry of the <paramref name="cachedElement"/>.</param>
        protected virtual void AddToEventAndNotificationsHandlingAndStartExpiration(ObservableCachedElement<TKey, TValue> cachedElement, TimeSpan expiry)
        {
            CheckForAndThrowIfDisposed();

            if (cachedElement != null)
            {
                cachedElement.ValuePropertyChanged += OnCachedElementValuePropertyChanged;
                cachedElement.StartOrUpdateExpiration(
                    expiry,
                    ExpiredElementsObserver,
                    ObserverExceptionsObserver,
                    ExpirationScheduler);
            }
        }

        /// <summary>
        /// Called when a cached element's value property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="forwardedEventArgs">The <see cref="ForwardedEventArgs{PropertyChangedEventArgs}"/> instance containing the event data.</param>
        protected virtual void OnCachedElementValuePropertyChanged(object sender, ForwardedEventArgs<PropertyChangedEventArgs> forwardedEventArgs)
        {
            CheckForAndThrowIfDisposed(false);

            try
            {
                if (forwardedEventArgs == null)
                    throw new ArgumentNullException(nameof(forwardedEventArgs));

                var senderAsObservableCachedElement = sender as ObservableCachedElement<TKey, TValue>;
                if (senderAsObservableCachedElement == null)
                    throw new ArgumentOutOfRangeException(nameof(sender), $"{nameof(sender)} must be a {typeof (ObservableCachedElement<TKey, TValue>).Name} instance");

                CacheChangesObserver.OnNext(ObservableCacheChange<TKey, TValue>.ItemChanged(senderAsObservableCachedElement.Key, senderAsObservableCachedElement.Value, forwardedEventArgs.OriginalEventArgs.PropertyName, senderAsObservableCachedElement.ExpiresAt(), senderAsObservableCachedElement.ExpirationType));
            }
            catch (Exception exception)
            {
                var observerException = new ObserverException($"An error occured notifying {nameof(Changes)} Observers of this {this.GetType().Name} about an element's Value Property Change.", exception);

                ObserverExceptionsObserver.OnNext(observerException);

                if (observerException.Handled == false)
                    throw;
            }
        }

        /// <summary>
        /// Removes <see cref="OnCachedElementValuePropertyChanged"/> as event handlers for <paramref name="cachedElement"/>'s
        /// <see cref="ObservableCachedElement{TKey,TValue}.ValuePropertyChanged"/>.
        /// </summary>
        /// <param name="cachedElement">The value.</param>
        protected virtual void RemoveFromEventAndNotificationsHandlingAndStopExpiration(ObservableCachedElement<TKey, TValue> cachedElement)
        {
            CheckForAndThrowIfDisposed(false);

            if (cachedElement != null)
            {
                cachedElement.ValuePropertyChanged -= OnCachedElementValuePropertyChanged;
                cachedElement.StopExpiration();
            }
        }

        #endregion

        #region Implementation of IDisposable

        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly object _isDisposedLocker = new object();

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisposed
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
        ///     Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisposing
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
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
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
                    var cacheChangesObserverAsDisposable = CacheChangesObserver as IDisposable;
                    cacheChangesObserverAsDisposable?.Dispose();
                    CacheChangesObserver = null;

                    _cacheChangesSubject?.Dispose();
                    _cacheChangesSubject = null;

                    _expiredElementsSubscription?.Dispose();
                    _expiredElementsSubscription = null;

                    _expiredElementsSubject?.Dispose();
                    _expiredElementsSubject = null;

                    var thrownExceptionsObserverAsDisposable = ObserverExceptionsObserver as IDisposable;
                    thrownExceptionsObserverAsDisposable?.Dispose();
                    ObserverExceptionsObserver = null;

                    _unhandledObserverExceptionsSubject?.Dispose();
                    _unhandledObserverExceptionsSubject = null;

                    // make sure values are removed from event handlers
                    var currentValues = InnerDictionary?.Values ?? new List<ObservableCachedElement<TKey, TValue>>();
                    foreach (var value in currentValues)
                    {
                        RemoveFromEventAndNotificationsHandlingAndStopExpiration(value);
                    }

                    var innerDictionaryAsDisposable = InnerDictionary as IDisposable;
                    innerDictionaryAsDisposable?.Dispose();
                    InnerDictionary = null;
                }
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Checks whether this instance has been disposed, optionally whether it is currently being disposed.
        /// </summary>
        /// <param name="checkIsDisposing">if set to <c>true</c> checks whether disposal is currently ongoing, indicated via <see cref="IsDisposing"/>.</param>
        protected virtual void CheckForAndThrowIfDisposed(bool checkIsDisposing = true)
        {
            if (checkIsDisposing && IsDisposing)
            {
                throw new ObjectDisposedException(GetType().Name, "This instance is currently being disposed.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
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

            var eventHandler = _propertyChanged;
            if (eventHandler != null)
            {
                NotificationScheduler.Schedule(() => eventHandler.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        #endregion

        #region Implementation of INotifyObserverExceptions

        /// <summary>
        /// Provides an observable sequence of <see cref="ObserverException">exceptions</see> thrown by observers.
        /// An <see cref="ObserverException" /> provides a <see cref="ObserverException.Handled" /> property, if set to [true] by
        /// any of the observers of <see cref="ObserverExceptions" /> observable, it is assumed to be safe to continue
        /// without re-throwing the exception.
        /// </summary>
        /// <value>
        /// An observable stream of unhandled exceptions.
        /// </value>
        public virtual IObservable<ObserverException> ObserverExceptions
        {
            get
            {
                CheckForAndThrowIfDisposed();

                // not caring about IsDisposing / IsDisposed on purpose once subscribed, so corresponding Exceptions are forwarded 'til the "end" to already existing subscribers
                return _unhandledObserverExceptionsSubject.Merge(InnerDictionary.ObserverExceptions);
            }
        }

        #endregion

        #region Implementation of INotifyObservableChanges

        /// <summary>
        /// (Temporarily) suppresses change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if wanted and applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressChangeNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.SuppressChangeNotifications(signalResetWhenFinished);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently suppressing observable change notifications of any kind.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suppressing observable change notifications; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">A Change Notification Suppression is currently already ongoing, multiple concurrent suppressions are not supported.</exception>
        public virtual bool IsTrackingChanges
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.IsTrackingChanges;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of the minimum amount of changes to switch individual notifications to a reset one.
        /// </summary>
        /// <value>
        /// The minimum items changed to be considered as a reset.
        /// </value>
        public int ThresholdAmountWhenChangesAreNotifiedAsReset
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset;
            }
            set
            {
                CheckForAndThrowIfDisposed();

                InnerDictionary.ThresholdAmountWhenChangesAreNotifiedAsReset = value;
            }
        }

        #endregion

        #region Implementation of INotifyObservableCacheChanges<out TKey,out TValue>

        /// <summary>
        ///     Gets an observable stream of changes to the <see cref="IObservableCache{TKey,TValue}" />.
        /// </summary>
        /// <value>
        ///     The changes.
        /// </value>
        public virtual IObservable<IObservableCacheChange<TKey, TValue>> Changes
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.DictionaryChanges.SkipContinuouslyWhile(change => change.ChangeType != ObservableDictionaryChangeType.ItemChanged) // this must be handled here separately from the underlying dictionary
                    .Select(dictionaryChange => dictionaryChange.ToObservableCacheChange()).Merge(_cacheChangesSubject).TakeWhile(_ => !IsDisposing && !IsDisposed).SkipContinuouslyWhile(change => !IsTrackingChanges).SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.ItemChanged && !IsTrackingItemChanges).SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.ItemReplaced && !IsTrackingItemChanges).SkipContinuouslyWhile(change => change.ChangeType == ObservableCacheChangeType.Reset && !IsTrackingResets);
            }
        }

        #endregion

        #region Implementation of IObservableCache<TKey,TValue>

        /// <summary>
        /// Gets the count of keys in this instance.
        /// </summary>
        /// <value>
        /// The count of keys in this instance.
        /// </value>
        public virtual int Count
        {
            get
            {
                CheckForAndThrowIfDisposed(false);

                return InnerDictionary.Count;
            }
        }

        /// <summary>
        /// Adds the specified <paramref name="key" /> with the given <paramref name="value" /> to the <see cref="IObservableCache{TKey,TValue}" />
        /// with its expiry set to <see cref="TimeSpan.MaxValue"/> and <see cref="ObservableCacheExpirationType"/> to <see cref="ObservableCacheExpirationType.DoNothing"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> Add(TKey key, TValue value)
        {
            return Add(key, value, TimeSpan.MaxValue, ObservableCacheExpirationType.DoNothing);
        }

        /// <summary>
        /// Adds the specified <paramref name="key"/> with the given <paramref name="value"/> to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>.</param>
        /// <param name="expirationType">Defines how the <paramref name="key" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> Add(TKey key, TValue value, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (expirationType == ObservableCacheExpirationType.Update && (SingleKeyRetrievalAction == null && MultipleKeysRetrievalAction == null))
                throw new ArgumentOutOfRangeException(nameof(expirationType), $"{nameof(expirationType)} cannot be set to {nameof(ObservableCacheExpirationType.Update)} if no {nameof(SingleKeyRetrievalAction)} or {nameof(MultipleKeysRetrievalAction)} had been specified at construction of this instance.");

            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    var observableCachedElement = new ObservableCachedElement<TKey, TValue>(key, value, expirationType);

                    InnerDictionary.Add(key, observableCachedElement);

                    AddToEventAndNotificationsHandlingAndStartExpiration(observableCachedElement, expiry);

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Adds the specified <paramref name="keyValuePairs"/> to the <see cref="IObservableCache{TKey,TValue}"/>
        /// with their expiry set to <see cref="TimeSpan.MaxValue"/> and <see cref="ObservableCacheExpirationType"/> to <see cref="ObservableCacheExpirationType.DoNothing"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs to add.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            if (keyValuePairs == null)
                throw new ArgumentNullException(nameof(keyValuePairs));

            return AddRange(keyValuePairs, TimeSpan.MaxValue, ObservableCacheExpirationType.DoNothing);
        }

        /// <summary>
        /// Adds the specified <paramref name="keyValuePairs"/> to the <see cref="IObservableCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs to add.</param>
        /// <param name="expiry">The expiry of the <paramref name="keyValuePairs"/>.</param>
        /// <param name="expirationType">Defines how the <paramref name="keyValuePairs" /> shall expire.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, TimeSpan expiry, ObservableCacheExpirationType expirationType = ObservableCacheExpirationType.Remove)
        {
            if (keyValuePairs == null)
                throw new ArgumentNullException(nameof(keyValuePairs));
            if (expirationType == ObservableCacheExpirationType.Update && (SingleKeyRetrievalAction == null && MultipleKeysRetrievalAction == null))
                throw new ArgumentOutOfRangeException(nameof(expirationType), $"{nameof(expirationType)} cannot be set to {nameof(ObservableCacheExpirationType.Update)} if no {nameof(SingleKeyRetrievalAction)} or {nameof(MultipleKeysRetrievalAction)} had been specified at construction of this instance.");
            
            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    // first check which keys / elements ARE in the innerdictionary
                    var cachedElementsForKeyValuePairs = keyValuePairs
                        .ToDictionary(kvp => kvp.Key, kvp => new ObservableCachedElement<TKey, TValue>(kvp.Key, kvp.Value, expirationType));

                    if (cachedElementsForKeyValuePairs.Count > 0)
                    {
                        IDictionary<TKey, ObservableCachedElement<TKey, TValue>> elementsThatCouldNotBeAdded;
                        InnerDictionary.TryAddRange(cachedElementsForKeyValuePairs, out elementsThatCouldNotBeAdded);

                        // and finally add to expiration / value changed notification etc
                        var keysForNonAddedElements = elementsThatCouldNotBeAdded.Keys;
                        foreach (var observableCachedElement in cachedElementsForKeyValuePairs.Where(element => !keysForNonAddedElements.Contains(element.Key, KeyComparer)))
                        {
                            AddToEventAndNotificationsHandlingAndStartExpiration(observableCachedElement.Value, expiry);
                        }

                        if (elementsThatCouldNotBeAdded.Count > 0)
                        {
                            var keyAlreadyExistsExceptions =
                                elementsThatCouldNotBeAdded
                                .Select(keyValuePair => new KeyAlreadyExistsException<TKey>(keyValuePair.Key))
                                .ToList();

                            if (keyAlreadyExistsExceptions.Count == 1)
                                throw keyAlreadyExistsExceptions.First();
                            if (keyAlreadyExistsExceptions.Count > 1)
                                throw new AggregateException($"{keyAlreadyExistsExceptions.Count} elements of the provided '{nameof(keyValuePairs)}' could not be added because a corresponding key already existed in this {this.GetType().Name}", keyAlreadyExistsExceptions);

                        }
                    }
                    
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> Clear()
        {
            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    var valuesBeforeClearing = InnerDictionary.Values;

                    InnerDictionary.Clear();

                    if (valuesBeforeClearing.Count > 0)
                    {
                        foreach (var value in valuesBeforeClearing)
                        {
                            RemoveFromEventAndNotificationsHandlingAndStopExpiration(value);
                        }
                    }
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Determines whether this instance contains the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns [true] if the <paramref name="key"/> is is contained in this instance, [false] if not.
        /// </returns>
        public virtual IObservable<bool> Contains(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<bool>(observer =>
            {
                try
                {
                    observer.OnNext(InnerDictionary.ContainsKey(key));
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Determines whether this instance contains the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent <see cref="Contains"/> checks.</param>
        /// <param name="scheduler">Scheduler to run the concurrent <see cref="Contains"/> checks on.</param>
        /// <returns>
        /// An observable stream that returns [true] if all <paramref name="keys"/> are contained in this instance, [false] if not.
        /// </returns>
        public virtual IObservable<bool> ContainsAll(ICollection<TKey> keys, int maxConcurrent = 1, IScheduler scheduler = null)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (maxConcurrent <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrent), "Must be 1 or higher");

            CheckForAndThrowIfDisposed();

            return scheduler != null
                ? keys.Select(Contains).Merge(maxConcurrent, scheduler).All(result => result)
                : keys.Select(Contains).Merge(maxConcurrent).All(result => result);
        }

        /// <summary>
        /// Determines whether which ones of the specified <paramref name="keys"/> are contained in this instance.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <returns>
        /// An observable stream that returns the subset of keys of the provided <paramref name="keys"/> that are contained in this instance.
        /// </returns>
        public virtual IObservable<TKey> ContainsWhich(IEnumerable<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            CheckForAndThrowIfDisposed();

            return Observable.Create<TKey>(observer =>
            {
                try
                {
                    foreach (var key in keys.Where(key => InnerDictionary.ContainsKey(key)))
                    {
                        observer.OnNext(key);
                    }
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Determines the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="DateTime"/> (UTC) the <paramref name="key"/> expires.
        /// </returns>
        public virtual IObservable<DateTime> ExpiresAt(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<DateTime>(observer =>
            {
                try
                {
                    observer.OnNext(InnerDictionary[key].ExpiresAt());

                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Determines the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TimeSpan"/> in which the <paramref name="key"/> expires.
        /// </returns>
        public virtual IObservable<TimeSpan> ExpiresIn(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<TimeSpan>(observer =>
            {
                try
                {
                    observer.OnNext(InnerDictionary[key].ExpiresIn());

                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to retrieve the <typeparamref name="TValue"/> for.</param>
        /// <returns>
        /// An observable stream that returns the <see cref="TValue"/> for the provided <paramref name="key"/>.
        /// </returns>
        public virtual IObservable<TValue> Get(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<TValue>(observer =>
            {
                try
                {
                    var cachedElement = InnerDictionary[key];

                    if (cachedElement.HasExpired)
                        throw new KeyHasExpiredException<TKey>(key, cachedElement.ExpiresAt());

                    // else
                    observer.OnNext(cachedElement.Value);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Gets the values for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to retrieve the values for.</param>
        /// <param name="maxConcurrent">Maximum number of concurrent retrievals.</param>
        /// <param name="scheduler">Scheduler to run the concurrent retrievals on.</param>
        /// <returns>
        /// An observable stream that returns the values for the provided <paramref name="keys"/>.
        /// </returns>
        public virtual IObservable<TValue> Get(IEnumerable<TKey> keys, int maxConcurrent = 1, IScheduler scheduler = null)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (maxConcurrent <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrent), "Must be 1 or higher");

            CheckForAndThrowIfDisposed();

            return scheduler != null
                ? keys.Select(Get).Merge(maxConcurrent, scheduler)
                : keys.Select(Get).Merge(maxConcurrent);
        }

        /// <summary>
        /// Removes the specified <paramref name="key"/> from this instance.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    ObservableCachedElement<TKey, TValue> observableCachedElement;
                    if (InnerDictionary.TryRemove(key, out observableCachedElement) == false)
                        throw new KeyNotFoundException();

                    RemoveFromEventAndNotificationsHandlingAndStopExpiration(observableCachedElement);

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Removes the specified <paramref name="keys"/> from this instance.
        /// </summary>
        /// <param name="keys">The keys to remove.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> RemoveRange(IEnumerable<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            CheckForAndThrowIfDisposed();
            
            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    // first check which keys / elements ARE in the innerdictionary
                    var cachedElementsInInnerDictionaryForKeys = new Dictionary<TKey, ObservableCachedElement<TKey, TValue>>();
                    foreach (var key in keys)
                    {
                        ObservableCachedElement<TKey, TValue> observableCachedElementForCurrentKey;
                        if (InnerDictionary.TryGetValue(key, out observableCachedElementForCurrentKey) == true)
                        {
                            cachedElementsInInnerDictionaryForKeys.Add(key, observableCachedElementForCurrentKey);
                        }
                    }

                    // then go ahead and remove them
                    if (cachedElementsInInnerDictionaryForKeys.Count > 0)
                    {
                        IDictionary<TKey, ObservableCachedElement<TKey, TValue>> elementsThatCouldNotBeRemoved;
                        InnerDictionary.TryRemoveRange(cachedElementsInInnerDictionaryForKeys, out elementsThatCouldNotBeRemoved);

                        // and finally remove expiration / value changed notification etc
                        var keysForNonRemovedElements = elementsThatCouldNotBeRemoved.Keys;
                        foreach (var observableCachedElement in cachedElementsInInnerDictionaryForKeys.Where(element => !keysForNonRemovedElements.Contains(element.Key, KeyComparer)))
                        {
                            RemoveFromEventAndNotificationsHandlingAndStopExpiration(observableCachedElement.Value);
                        }
                    }

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Updates the specified <paramref name="key"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The value to update the <paramref name="key"/> with.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> Update(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    ObservableCachedElement<TKey, TValue> existingCachedElement;
                    if (InnerDictionary.TryGetValue(key, out existingCachedElement) == false)
                    {
                        throw new KeyNotFoundException<TKey>(key);
                    }

                    // else
                    UpdateValueForCachedElement(existingCachedElement, value, true);
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Updates a range of <paramref name="keyValuePairs"/>.
        /// </summary>
        /// <param name="keyValuePairs">The key/value pairs that each contain the key to update and the value to update it with.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> UpdateRange(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="key"/>.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> UpdateExpiration(TKey key, TimeSpan expiry)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CheckForAndThrowIfDisposed();

            return Observable.Create<Unit>(observer =>
            {
                try
                {
                    ObservableCachedElement<TKey, TValue> existingCachedElement;
                    if (InnerDictionary.TryGetValue(key, out existingCachedElement) == false)
                    {
                        throw new KeyNotFoundException<TKey>(key);
                    }

                    // else
                    existingCachedElement.StartOrUpdateExpiration(
                        expiry,
                        ExpiredElementsObserver,
                        ObserverExceptionsObserver,
                        ExpirationScheduler);

                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Updates the expiration behavior for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">The keys to update.</param>
        /// <param name="expiry">The expiry of the <paramref name="keys"/>.</param>
        /// <returns>
        /// An observable stream that, when done, returns an <see cref="Unit" />.
        /// </returns>
        public virtual IObservable<Unit> UpdateExpiration(IEnumerable<TKey> keys, TimeSpan expiry)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of INotifyObservableResets

        /// <summary>
        /// Gets a value indicating whether this instance is tracking and notifying about
        /// list / collection resets, typically for data binding.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tracking resets; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsTrackingResets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.IsTrackingResets;
            }
        }

        /// <summary>
        /// Gets the reset notifications as an observable stream.  Whenever signaled,
        /// observers should reset any knowledge / state etc about the list.
        /// </summary>
        /// <value>
        /// The resets.
        /// </value>
        public virtual IObservable<Unit> Resets
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.Resets.TakeWhile(_ => !IsDisposing && !IsDisposed).SkipContinuouslyWhile(_ => !IsTrackingResets);
            }
        }

        /// <summary>
        /// (Temporarily) suppresses change notifications for resets until the <see cref="IDisposable" /> handed over to the caller
        /// has been Disposed and then a Reset will be signaled, if wanted and applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressResetNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.SuppressResetNotifications(signalResetWhenFinished);
        }

        #endregion

        #region Implementation of INotifyObservableCacheItemChanges<out TKey,out TValue>

        /// <summary>
        /// Gets the observable streams of collection item changes.
        /// </summary>
        /// <value>
        /// The item changes.
        /// </value>
        public virtual IObservable<IObservableCacheChange<TKey, TValue>> ItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return Changes.TakeWhile(_ => !IsDisposing && !IsDisposed).Where(change => change.ChangeType == ObservableCacheChangeType.ItemChanged || change.ChangeType == ObservableCacheChangeType.ItemReplaced).SkipContinuouslyWhile(change => !IsTrackingItemChanges);
            }
        }

        /// <summary>
        /// Gets the observable streams of item expirations.
        /// </summary>
        /// <value>
        /// The item item expirations.
        /// </value>
        public virtual IObservable<IObservableCacheChange<TKey, TValue>> ItemExpirations
        {
            get
            {
                return _cacheChangesSubject.
                    TakeWhile(_ => !IsDisposing && !IsDisposed)
                    .Where(change => change.ChangeType == ObservableCacheChangeType.ItemExpired)
                    .SkipContinuouslyWhile(change => !IsTrackingItemChanges);
            }
        }

        /// <summary>
        /// Gets the expired elements as an observable stream.
        /// </summary>
        /// <value>
        /// The expired elements.
        /// </value>
        protected virtual IObservable<ObservableCachedElement<TKey, TValue>> ExpiredElements
        {
            get
            {
                return _expiredElementsSubject
                    .TakeWhile(_ => !IsDisposing && !IsDisposed);
            }
        }

        #endregion

        #region Implementation of INotifyObservableItemChanges

        /// <summary>
        /// (Temporarily) suppresses item change notifications until the returned <see cref="IDisposable" />
        /// has been Disposed and a Reset will be signaled, if applicable.
        /// </summary>
        /// <param name="signalResetWhenFinished">if set to <c>true</c> signals a reset when finished.</param>
        /// <returns></returns>
        public virtual IDisposable SuppressItemChangedNotifications(bool signalResetWhenFinished = true)
        {
            CheckForAndThrowIfDisposed();

            return InnerDictionary.SuppressItemChangedNotifications(signalResetWhenFinished);
        }

        /// <summary>
        /// Gets a value indicating whether this instance has per item change tracking enabled and therefore listens to
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events, if that interface is implemented, too.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has item change tracking enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsTrackingItemChanges
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return InnerDictionary.IsTrackingItemChanges;
            }
        }

        #endregion
    }
}