// -----------------------------------------------------------------------
// <copyright file="WeakEventHandler.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary>Idea is based on Paul Stovell's blog post (http://paulstovell.com/blog/weakevents) but uses cached expression bodies/constructs for better performance.</summary>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;

namespace JB
{
	/// <summary>
	/// A Weak Event Handler that handles garbage collected handlers gracefully.
	/// </summary>
	/// <example>
	/// Instead of registering the <see cref="EventHandler{TEventArgs}"/> to your event directly, wrap it in a <see cref="WeakEventHandler{TEventArgs,THandler}"/>. That's all.
	/// </example>
	/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
	/// <typeparam name="TEventTarget">The type of the handler.</typeparam>
	public sealed class WeakEventHandler<TEventArgs, TEventTarget>
		where TEventArgs : EventArgs
		where TEventTarget : class
	{
		private WeakReference<TEventTarget> _targetReference;
		private Lazy<Action<TEventTarget, object, TEventArgs>> _cachedCallback;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakEventHandler{TEventArgs, THandler}"/> class.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		public WeakEventHandler(EventHandler<TEventArgs> callback)
		{
			if (typeof(TEventTarget) != callback.Target.GetType()) throw new ArgumentOutOfRangeException(nameof(callback));

			SetupReferenceAndInvoker(callback);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakEventHandler{TEventArgs, TEventTarget}"/> class.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		public WeakEventHandler(Delegate callback)
		{
			if (typeof(TEventTarget) != callback.Target.GetType()) throw new ArgumentOutOfRangeException(nameof(callback));

			SetupReferenceAndInvoker(callback);
		}

		/// <summary>
		/// Setups the (weak) target reference and (cached, lazy) invoker.
		/// </summary>
		/// <param name="callback">The callback.</param>
		private void SetupReferenceAndInvoker(Delegate callback)
		{
			_targetReference = new WeakReference<TEventTarget>((TEventTarget)callback.Target, true);
			_cachedCallback = new Lazy<Action<TEventTarget, object, TEventArgs>>(() =>
			{
				var instanceParameter = Expression.Parameter(typeof(TEventTarget));
				var senderParameterExpression = Expression.Parameter(typeof(object));
				var eventArgsParameterExpression = Expression.Parameter(typeof(TEventArgs));

				var callExpression = Expression.Call(instanceParameter, callback.Method, senderParameterExpression, eventArgsParameterExpression);

				return Expression.Lambda<Action<TEventTarget, object, TEventArgs>>(callExpression, instanceParameter, senderParameterExpression, eventArgsParameterExpression).Compile();
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>
		/// The actual Handler to register to your event.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
		[DebuggerNonUserCode]
		public void Handler(object sender, TEventArgs eventArgs)
		{
			TEventTarget target;
			if (_targetReference.TryGetTarget(out target))
			{
				_cachedCallback.Value.Invoke(target, sender, eventArgs);
			}
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="WeakEventHandler{TEventArgs, THandler}"/> to <see cref="EventHandler{TEventArgs}"/>.
		/// </summary>
		/// <param name="eventHandler">The event handler.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static implicit operator EventHandler<TEventArgs>(WeakEventHandler<TEventArgs, TEventTarget> eventHandler)
		{
			return eventHandler.Handler;
		}
	}
}