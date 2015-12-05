namespace JB.Collections.Reactive
{
	public interface IObservableBindingDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>, IObservableBindingList<TValue>
    {

	}
}