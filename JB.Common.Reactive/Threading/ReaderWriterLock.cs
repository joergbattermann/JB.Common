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
		/// Gets or sets the inner disposable.
		/// </summary>
		/// <value>
		/// The inner disposable.
		/// </value>
		private IDisposable InnerDisposable { get; set; }

		/// <summary>
		/// Prevents a default instance of the <see cref="ReaderWriterLock"/> class from being created.
		/// </summary>
		internal ReaderWriterLock(long id, IDisposable innerDisposable = null)
		{
			Id = id;
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