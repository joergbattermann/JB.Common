// -----------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

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

            return value.GetType().ImplementsInterface<TInterface>();
        }
    }
}