// -----------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JB.Linq;

namespace JB.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="object"/> instances
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determines whether the provided object is one of the given type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Pure]
        public static bool IsObjectOfType<TObject>(this object value)
        {
            return value is TObject;
        }

        /// <summary>
        /// Checks whether the given <paramref name="value"/>'s <see cref="Type"/> implements the provided interface.
        /// </summary>
        /// <typeparam name="TInterface">the interface type.</typeparam>
        /// <returns>[true] if <paramref name="value"/>'s type implements the <typeparamref name="TInterface"/>, [false] if otherwise.</returns>
        /// <returns></returns>
        [Pure]
        public static bool ImplementsInterface<TInterface>(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return value is TInterface;
        }

        /// <summary>
        /// Returns the specified value as an <see cref="IEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the <paramref name="value"/>.</typeparam>
        /// <param name="value">The value to return as an enumerable.</param>
        /// <returns></returns>
        [Pure]
        public static IEnumerable<TSource> AsEnumerable<TSource>(this TSource value)
        {
            return Enumerable.From(value);
        }
    }
}