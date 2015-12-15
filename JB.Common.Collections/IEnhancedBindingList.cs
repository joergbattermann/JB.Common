using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace JB.Collections
{
    public interface IEnhancedBindingList<T> :
        IBindingList,
        ICollection<T>,
        ICollection,
        IReadOnlyCollection<T>,
        IList<T>,
        IList,
        IReadOnlyList<T>,
        IItemMovableList<T>,
        IEnumerable<T>,
        IEnumerable,
        ICancelAddNew,
        IRaiseItemChangedEvents
    {
        
    }
}