using System.Collections.Generic;

namespace JB.Collections.Reactive
{
	public interface IReactiveReadOnlyList<T> : IReactiveReadOnlyCollection<T>, IReadOnlyList<T>
	{
	}
}