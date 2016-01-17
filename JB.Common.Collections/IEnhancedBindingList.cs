using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace JB.Collections
{
    /// <summary>
    /// A slightly enhanced <see cref="IBindingList"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnhancedBindingList<T> :
        IBindingList,
        ICollection<T>,
        ICollection,
        IReadOnlyCollection<T>,
        IBulkModifiableCollection<T>,
        IList<T>,
        IReadOnlyList<T>,
        IItemMovableList<T>,
        IEnumerable<T>,
        IEnumerable,
        ICancelAddNew,
        IRaiseItemChangedEvents
    {

        /// <summary>
        /// Raises a <see cref="E:IBindingList.ListChanged"/> event of type <see cref="F:System.ComponentModel.ListChangedType.ItemChanged"/>
        /// for the item at the specified position.
        /// </summary>
        /// <param name="position">A zero-based index of the item to be reset.</param>
        void ResetItem(int position);

        /// <summary>
        /// Raises a <see cref="E:IBindingList.ListChanged"/> event of type <see cref="F:System.ComponentModel.ListChangedType.Reset"/>.
        /// </summary>
        void ResetBindings();

        /// <summary>
        /// Gets or sets a value indicating whether adding or removing items within the list raises <see cref="E:IBindingList.ListChanged"/> events.
        /// </summary>
        /// 
        /// <returns>
        /// true if adding or removing items raises <see cref="E:IBindingList.ListChanged"/> events; otherwise, false. The default is true.
        /// </returns>
        bool RaiseListChangedEvents { get; set; }
    }
}