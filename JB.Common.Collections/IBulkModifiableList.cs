using System.Collections.Generic;

namespace JB.Collections
{
    /// <summary>
    /// A <see cref="IList{T}"/> that provides bulk add- and remove modifications.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public interface IBulkModifiableList<T> : IList<T>, IBulkModifiable<T>
    {
    }
}