namespace JB.Collections.Reactive
{
	public interface IObservablePropertyChangedEventArgs<out TSender>
	{
		/// <summary>
		/// The name of the property that has changed for the Sender.
		/// </summary>
		string PropertyName { get; }

		/// <summary>
		/// The object that has raised the change.
		/// </summary>
		TSender Sender { get; }
	}
}