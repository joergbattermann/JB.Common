using System.Collections.Generic;

namespace JB.Collections
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
		/// Gets the new, post-change (starting) index for the <see cref="Items"/>.
		/// </summary>
		/// <value>
		/// The post-change starting index, -1 for removals, otherwise 0 or greater.
		/// </value>
		int NewStartingIndex { get; }
		/// <summary>
		/// Gets the previous, pre-change (starting) index for the <see cref="Items"/>.
		/// </summary>
		/// <value>
		/// The pre-change (starting) index, -1 for additions, otherwise 0 or greater.
		/// </value>
		int OldStartingIndex { get; }

		/// <summary>
		/// Gets the items that were changed or removed.
		/// </summary>
		/// <value>
		/// The affected items.
		/// </value>
		IReadOnlyList<T> Items { get; }
	}
}