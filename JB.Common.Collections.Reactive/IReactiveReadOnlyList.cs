using System.Collections.Generic;

namespace JB.Collections
{
	public interface IReactiveReadOnlyList<T> : IReactiveReadOnlyCollection<T>, IReadOnlyList<T>
	{
	}
}