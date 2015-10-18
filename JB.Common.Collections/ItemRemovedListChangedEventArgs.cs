using System.ComponentModel;

namespace JB.Collections
{
	/// <summary>
	/// A custom <see cref="ListChangedEventArgs"/> that contains the removed Item.
	/// The default <see cref="ListChangedType.ItemDeleted"/> event does only contain
	/// the deleted index location, but no indication what was actually deleted.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ItemRemovedListChangedEventArgs<T> : ListChangedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ItemRemovedListChangedEventArgs{T}" /> class.
		/// </summary>
		/// <param name="item">The removed item.</param>
		/// <param name="index">The index of the removed item prior to removal.</param>
		public ItemRemovedListChangedEventArgs(T item, int index)
			: base(ListChangedType.ItemDeleted, -1, index)
		{
			Item = item;
		}

		/// <summary>
		/// Gets or sets the item.
		/// </summary>
		/// <value>
		/// The item.
		/// </value>
		public T Item { get; private set; }
	}
}