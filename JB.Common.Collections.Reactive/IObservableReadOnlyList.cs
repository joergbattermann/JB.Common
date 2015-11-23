using System.Collections.Generic;

namespace JB.Collections.Reactive
{
	public interface IObservableReadOnlyList<T> : IObservableReadOnlyCollection<T>, IReadOnlyList<T>
	{
	}
}