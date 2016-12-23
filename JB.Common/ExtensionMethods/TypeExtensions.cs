// -----------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace JB.ExtensionMethods
{
    /// <summary>
    /// Extension methods particularly for and around <see cref="System.Type"/> instances
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks whether the given type implements the provided interface.
        /// </summary>
        /// <typeparam name="TInterface">the interface type.</typeparam>
        /// <returns>[true] if <paramref name="type"/> implements the <typeparamref name="TInterface"/>, [false] if otherwise.</returns>
        /// <returns></returns>
        [Pure]
        public static bool ImplementsInterface<TInterface>(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (typeof(TInterface).IsInterface == false) throw new ArgumentOutOfRangeException(nameof(type), "Type Parameter must be an interface type.");
            
            var interfaceType = typeof(TInterface);

            return interfaceType.IsAssignableFrom(type);
        }

        /// <summary>
        /// Checks whether the given type is a subclass of a super type.
        /// </summary>
        /// <typeparam name="TSuperType">The super type / base class type</typeparam>
        /// <param name="subType">The type.</param>
        /// <returns>[true] if <paramref name="subType"/> is a sub class of <typeparamref name="TSuperType"/>, [false] if otherwise.</returns>
        [Pure]
        public static bool IsSubclassOf<TSuperType>(this Type subType)
        {
            if (subType == null) throw new ArgumentNullException(nameof(subType));
            if (typeof(TSuperType).IsClass == false) throw new ArgumentOutOfRangeException(nameof(subType), "(Sub)Type Parameter must be a class type.");

            var baseType = typeof(TSuperType);

            return subType.IsSubclassOf(baseType);
        }
    }
}