namespace JB.Collections.Reactive
{
    public interface IObservableCollectionChange<out T>
	{
		/// <summary>
		/// Gets the type of the change.
		/// </summary>
		/// <value>
		/// The type of the change.
		/// </value>
		ObservableCollectionChangeType ChangeType { get; }
        
		/// <summary>
		/// Gets the item that was added, changed or removed.
		/// </summary>
		/// <value>
		/// The affected item, if any.
		/// </value>
		T Item { get; }
	}
}