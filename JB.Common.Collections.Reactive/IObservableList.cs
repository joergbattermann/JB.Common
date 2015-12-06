// -----------------------------------------------------------------------
// <copyright file="IObservableList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace JB.Collections.Reactive
{
	public interface IObservableList<T> :
        IObservableReadOnlyList<T>,
        IObservableCollection<T>,
        IList<T>,
        IList,
        ICollection<T>,
        ICollection,
        IEnumerable<T>,
        IEnumerable
    {
		 
	}
}