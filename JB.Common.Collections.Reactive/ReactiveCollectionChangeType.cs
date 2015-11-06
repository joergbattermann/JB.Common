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
		/// Indicates a major change and consumers should reset any existing knowledge and state about the affected collection and refetch its current state.
		/// </summary>
		Reset,
	}
}