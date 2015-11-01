using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace JB.VisualStudio.ExtensionMethods
{
	/// <summary>
	/// Extension Methods for <see cref="IServiceProvider"/> instances specific to Visual Studio and its registered services.
	/// </summary>
	public static class VisualStudioServiceProviderExtensions
	{
        /// <summary>
        /// Gets the <see cref="Microsoft.VisualStudio.Shell.Interop.IVsImageService2">visual studio image service</see>.
        /// </summary>
        /// <value>
        /// The visual studio image service.
        /// </value>
        public static Microsoft.VisualStudio.Shell.Interop.IVsImageService2 GetVsImageService(this IServiceProvider serviceProvider)
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

			return (Microsoft.VisualStudio.Shell.Interop.IVsImageService2)serviceProvider.GetService(typeof(SVsImageService));
        }

        /// <summary>
        /// Gets the <see cref="IVsShell"/> service.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IVsShell GetVsShell(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return (IVsShell)serviceProvider.GetService(typeof(SVsShell));
        }

        /// <summary>
        /// Loads the visual studio package.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="packageGuid">The unique identifier for the package to load.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool LoadVisualStudioPackage(this IServiceProvider serviceProvider, Guid packageGuid)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            IVsShell vsShell = serviceProvider.GetVsShell();
            if (vsShell == null)
                return false;

            IVsPackage loadedPackage = null;
            return vsShell.LoadPackage(ref packageGuid, out loadedPackage) == Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}