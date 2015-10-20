namespace JB.Collections
{
	public enum ReactiveCollectionChangeType
	{
		/// <summary>
		/// One Item has been added.
		/// </summary>
		ItemAdded,
		/// <summary>
		/// One Item has changed.
		/// </summary>
		ItemChanged,
		/// <summary>
		/// One Item has been moved.
		/// </summary>
		ItemMoved,
		/// <summary>
		/// One Item has been removed.
		/// </summary>
		ItemRemoved,
		/// <summary>
		/// More than one Item have been added.
		/// </summary>
		ItemsAdded,
		/// <summary>
		/// More than one Item have been moved.
		/// </summary>
		ItemsMoved,
		/// <summary>
		/// More than one Item have been removed.
		/// </summary>
		ItemsRemoved,
		/// <summary>
		/// A major change has taken place and consumers should reset (not clear!) any existing knowledge about the collection and refetch its current state.
		/// </summary>
		Reset,
	}
}