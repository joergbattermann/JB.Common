namespace JB.Collections.Reactive
{
	public interface IReactiveCollectionChange<out T>
	{
		/// <summary>
		/// Gets the type of the change.
		/// </summary>
		/// <value>
		/// The type of the change.
		/// </value>
		ReactiveCollectionChangeType ChangeType { get; }

		/// <summary>
		/// Gets the new, post-change (starting) index for the <see cref="Item"/>.
		/// </summary>
		/// <value>
		/// The post-change starting index, -1 for removals, otherwise 0 or greater.
		/// </value>
		int Index { get; }

        /// <summary>
        /// Gets the previous, pre-change (starting) index for the <see cref="Item"/>.
        /// </summary>
        /// <value>
        /// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
        /// </value>
        int OldIndex { get; }

		/// <summary>
		/// Gets the item that was added, changed or removed.
		/// </summary>
		/// <value>
		/// The affected item, if any.
		/// </value>
		T Item { get; }
	}
}