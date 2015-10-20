// -----------------------------------------------------------------------
// <copyright file="IReactiveList.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace JB.Collections
{
	public interface IReactiveList<T> : IReactiveCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>,
		ICollection, IEnumerable, IList
    {
		 
	}
}