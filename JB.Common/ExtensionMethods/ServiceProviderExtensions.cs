// -----------------------------------------------------------------------
// <copyright file="ServiceProviderExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace JB.ExtensionMethods
{
	/// <summary>
	/// Extension Methods for <see cref="IServiceProvider"/> instances.
	/// </summary>
	public static class ServiceProviderExtensions
	{
        /// <summary>
        ///     Gets the service of the specific type.
        /// </summary>
        /// <typeparam name="TService">Type of the service</typeparam>
        /// <returns></returns>
        [Pure]
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            
            return (TService)serviceProvider.GetService(typeof(TService));
		}

        /// <summary>
        ///     Gets the service of the specific type for a given, known implementation.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <typeparam name="TKnownServiceImplementation">The known type of the service implementation.</typeparam>
        /// <returns></returns>
        [Pure]
        public static TKnownServiceImplementation GetService<TService, TKnownServiceImplementation>(this IServiceProvider serviceProvider)
            where TKnownServiceImplementation : class, TService
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetService(typeof(TService)) as TKnownServiceImplementation;
        }
	}
}
