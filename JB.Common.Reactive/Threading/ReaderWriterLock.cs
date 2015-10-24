using System;

namespace JB.Reactive.Threading
{
	public class ReaderWriterLock : IDisposable
	{
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public long Id { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is exclusive.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is exclusive; otherwise, <c>false</c>.
		/// </value>
		public bool IsExclusive { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is a non-exclusive, concurrent one.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is concurrent; otherwise, <c>false</c>.
		/// </value>
		public bool IsConcurrent => !IsExclusive;

		/// <summary>
		/// Gets or sets the inner disposable.
		/// </summary>
		/// <value>
		/// The inner disposable.
		/// </value>
		private IDisposable InnerDisposable { get; set; }

		/// <summary>
		/// Prevents a default instance of the <see cref="ReaderWriterLock" /> class from being created.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="isExclusive">if set to <c>true</c> [is exclusive].</param>
		/// <param name="innerDisposable">The inner disposable.</param>
		internal ReaderWriterLock(long id, bool isExclusive, IDisposable innerDisposable = null)
		{
			Id = id;
			IsExclusive = isExclusive;
			InnerDisposable = innerDisposable;
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (InnerDisposable != null)
			{
				InnerDisposable.Dispose();
				InnerDisposable = null;
			}
		}

		#endregion
	}
}