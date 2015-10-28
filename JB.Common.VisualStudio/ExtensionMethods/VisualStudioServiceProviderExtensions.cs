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
		/// Gets the visual studio image service.
		/// </summary>
		/// <value>
		/// The visual studio image service.
		/// </value>
		public static IVsImageService2 GetVisualStudioImageService(this IServiceProvider serviceProvider)
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

			return (IVsImageService2)serviceProvider.GetService(typeof(SVsImageService));
        }
	}
}