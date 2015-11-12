namespace JB.Collections.Reactive
{
	public interface IReactivePropertyChangedEventArgs<out TSender>
	{
		/// <summary>
		/// The name of the property that has changed on Sender.
		/// </summary>
		string PropertyName { get; }

		/// <summary>
		/// The object that has raised the change.
		/// </summary>
		TSender Sender { get; }
	}
}