// -----------------------------------------------------------------------
// <copyright file="SynchronizationContextExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>
// Extension methods for SynchronizationContext instances.
// </summary>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using JB.Threading.Tasks;

namespace JB.Threading
{
	/// <summary>
	/// Extension methods for <see cref="SynchronizationContext"/> instances.
	/// </summary>
	public static class SynchronizationContextExtensions
	{
		/// <summary>
		/// Sends the specified action to the <paramref name="synchronizationContext"/>.
		/// </summary>
		/// <param name="synchronizationContext">The synchronization context.</param>
		/// <param name="action">The action.</param>
		/// <exception cref="System.ArgumentNullException">
		/// </exception>
		public static void Send(this SynchronizationContext synchronizationContext, Action action)
		{
			if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
			if (action == null) throw new ArgumentNullException(nameof(action));

		    synchronizationContext.OperationStarted();
		    synchronizationContext.Send(_ =>
		    {
		        try
		        {
		            action();
		        }
		        finally
		        {
		            synchronizationContext.OperationCompleted();
		        }
		    }, null);
		}

        /// <summary>
        /// Sends the specified action to the <paramref name="synchronizationContext" />.
        /// </summary>
        /// <typeparam name="TState">The type of the <paramref name="state"/> parameter.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="action">The action.</param>
        /// <param name="state">The state to pass along.</param>
        /// <exception cref="ArgumentNullException">
        /// synchronizationContext
        /// or
        /// action
        /// </exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Send<TState>(this SynchronizationContext synchronizationContext, Action<TState> action, TState state)
	    {
	        if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
	        if (action == null) throw new ArgumentNullException(nameof(action));

	        synchronizationContext.OperationStarted();
	        synchronizationContext.Send(o =>
	        {
	            try
	            {
	                action((TState)o);
	            }
	            finally
	            {
	                synchronizationContext.OperationCompleted();
	            }
	        }, state);
	    }

        /// <summary>
        /// Sends the specified function to the <paramref name="synchronizationContext"/> and returns its result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static TResult Send<TResult>(this SynchronizationContext synchronizationContext, Func<TResult> func)
		{
			if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
			if (func == null) throw new ArgumentNullException(nameof(func));

			var result = default(TResult);
		    synchronizationContext.OperationStarted();
		    synchronizationContext.Send(_ =>
		    {
		        try
		        {
		            result = func();
                }
		        finally
		        {
		            synchronizationContext.OperationCompleted();
		        }
		    }, null);

			return result;
		}

        /// <summary>
        /// Sends the specified function to the <paramref name="synchronizationContext" /> and returns its result.
        /// </summary>
        /// <typeparam name="TState">The type of the <paramref name="state"/> parameter.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="func">The function.</param>
        /// <param name="state">The state to pass along.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// synchronizationContext
        /// or
        /// func
        /// </exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static TResult Send<TState, TResult>(this SynchronizationContext synchronizationContext, Func<TState, TResult> func, TState state)
	    {
	        if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
	        if (func == null) throw new ArgumentNullException(nameof(func));

	        var result = default(TResult);
	        synchronizationContext.OperationStarted();
	        synchronizationContext.Send(o =>
	        {
	            try
	            {
	                result = func((TState)o);
	            }
	            finally
	            {
	                synchronizationContext.OperationCompleted();
	            }
	        }, state);

	        return result;
	    }

        /// <summary>
        /// Posts the specified action to the <paramref name="synchronizationContext"/>.
        /// </summary>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static void Post(this SynchronizationContext synchronizationContext, Action action)
		{
			if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
			if (action == null) throw new ArgumentNullException(nameof(action));

			synchronizationContext.OperationStarted();
		    synchronizationContext.Post(_ =>
		    {
		        try
		        {
		            action();
		        }
		        finally
		        {
		            synchronizationContext.OperationCompleted();
		        }
		    }, null);
        }

        /// <summary>
        /// Posts the specified action to the <paramref name="synchronizationContext" />.
        /// </summary>
        /// <typeparam name="TState">The type of the <paramref name="state"/> parameter.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="action">The action.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="ArgumentNullException">
        /// synchronizationContext
        /// or
        /// action
        /// </exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Post<TState>(this SynchronizationContext synchronizationContext, Action<TState> action, TState state)
	    {
	        if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
	        if (action == null) throw new ArgumentNullException(nameof(action));

	        synchronizationContext.OperationStarted();
	        synchronizationContext.Post(o =>
	        {
	            try
	            {
	                action((TState)o);
	            }
	            finally
	            {
	                synchronizationContext.OperationCompleted();
	            }
	        }, state);
	    }

        /// <summary>
        /// Posts the specified action to the <paramref name="synchronizationContext"/> asynchronously.
        /// </summary>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="action">The action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static Task PostAsync(this SynchronizationContext synchronizationContext, Action action, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
			if (action == null) throw new ArgumentNullException(nameof(action));

			var taskCompletionSource = new TaskCompletionSource();

		    synchronizationContext.OperationStarted();
            synchronizationContext.Post(state =>
			{
			    try
			    {
			        cancellationToken.ThrowIfCancellationRequested();
			        action();

			        cancellationToken.ThrowIfCancellationRequested();
			        taskCompletionSource.SetResult();
			    }
			    catch (OperationCanceledException)
			    {
			        taskCompletionSource.SetCanceled();
			    }
			    catch (Exception exception)
			    {
			        taskCompletionSource.SetException(exception);
			    }
			    finally
			    {
			        synchronizationContext.OperationCompleted();
                }
			}, null);

			return taskCompletionSource.Task;
		}

        /// <summary>
        /// Posts the specified action to the <paramref name="synchronizationContext" /> asynchronously.
        /// </summary>
        /// <typeparam name="TState">The type of the <paramref name="state"/> parameter.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="action">The action.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// synchronizationContext
        /// or
        /// action
        /// </exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Task PostAsync<TState>(this SynchronizationContext synchronizationContext, Action<TState> action, TState state, CancellationToken cancellationToken = default(CancellationToken))
	    {
	        if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
	        if (action == null) throw new ArgumentNullException(nameof(action));

	        var taskCompletionSource = new TaskCompletionSource();

	        synchronizationContext.OperationStarted();
            synchronizationContext.Post(o =>
	        {
	            try
	            {
	                cancellationToken.ThrowIfCancellationRequested();
	                action((TState)o);

	                cancellationToken.ThrowIfCancellationRequested();
	                taskCompletionSource.SetResult();
	            }
	            catch (OperationCanceledException)
	            {
	                taskCompletionSource.SetCanceled();
	            }
	            catch (Exception exception)
	            {
	                taskCompletionSource.SetException(exception);
	            }
	            finally
	            {
	                synchronizationContext.OperationCompleted();
	            }
            }, state);

	        return taskCompletionSource.Task;
	    }

        /// <summary>
        /// Posts the specified function to the <paramref name="synchronizationContext"/> asynchronously and returns its result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="func">The function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static Task<TResult> PostAsync<TResult>(this SynchronizationContext synchronizationContext, Func<TResult> func, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
			if (func == null) throw new ArgumentNullException(nameof(func));

			var taskCompletionSource = new TaskCompletionSource<TResult>();

		    synchronizationContext.OperationStarted();
            synchronizationContext.Post(state =>
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					var result = func();

					cancellationToken.ThrowIfCancellationRequested();
					taskCompletionSource.SetResult(result);
				}
				catch (OperationCanceledException)
				{
					taskCompletionSource.SetCanceled();
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
				}
				finally
				{
				    synchronizationContext.OperationCompleted();
				}
            }, null);

			return taskCompletionSource.Task;
		}

        /// <summary>
        /// Posts the specified function to the <paramref name="synchronizationContext" /> asynchronously and returns its result.
        /// </summary>
        /// <typeparam name="TState">The type of the <paramref name="state"/> parameter.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <param name="func">The function.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// synchronizationContext
        /// or
        /// func
        /// </exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Task<TResult> PostAsync<TState, TResult>(this SynchronizationContext synchronizationContext, Func<TState, TResult> func, TState state, CancellationToken cancellationToken = default(CancellationToken))
	    {
	        if (synchronizationContext == null) throw new ArgumentNullException(nameof(synchronizationContext));
	        if (func == null) throw new ArgumentNullException(nameof(func));

	        var taskCompletionSource = new TaskCompletionSource<TResult>();

	        synchronizationContext.OperationStarted();
	        synchronizationContext.Post(o =>
	        {
	            try
	            {
	                cancellationToken.ThrowIfCancellationRequested();
	                var result = func((TState)o);

	                cancellationToken.ThrowIfCancellationRequested();
	                taskCompletionSource.SetResult(result);
	            }
	            catch (OperationCanceledException)
	            {
	                taskCompletionSource.SetCanceled();
	            }
	            catch (Exception exception)
	            {
	                taskCompletionSource.SetException(exception);
	            }
	            finally
	            {
	                synchronizationContext.OperationCompleted();
	            }
	        }, state);

	        return taskCompletionSource.Task;
	    }
    }
}