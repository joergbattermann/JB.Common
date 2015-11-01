// -----------------------------------------------------------------------
// <copyright file="VisualStudioServiceProviderExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using Microsoft.TeamFoundation.Controls;
using JB.ExtensionMethods;
using Microsoft.TeamFoundation.Client;

namespace JB.VisualStudio.TeamFoundation.ExtensionMethods
{
    /// <summary>
	/// Extension Methods for <see cref="IServiceProvider"/> instances specific to Visual Studio and its Team Foundation related / registered services.
	/// </summary>
    public static class VisualStudioServiceProviderExtensions
    {
        /// <summary>
        /// Gets the <see cref="ITeamExplorer"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorer GetTeamExplorer(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetService<ITeamExplorer>();
        }

        /// <summary>
        /// Gets the <see cref="ITeamFoundationContextManager4"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamFoundationContextManager4 GetTeamFoundationContextManager(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetService<ITeamFoundationContextManager4>();
        }

        /// <summary>
        /// Gets the current <see cref="ITeamFoundationContext"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public static ITeamFoundationContext GetCurrentTeamFoundationContext(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetTeamFoundationContextManager()?.CurrentContext;
        }

        /// <summary>
        /// Gets the current <see cref="ITeamExplorerPage"/>, if any.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage GetCurrentTeamExplorerPage(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetService<ITeamExplorerPage>();
        }
    }
}