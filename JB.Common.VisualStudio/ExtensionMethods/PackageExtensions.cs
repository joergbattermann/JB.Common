using System;
using Microsoft.VisualStudio.Shell;

namespace JB.VisualStudio.ExtensionMethods
{
    /// <summary>
    /// Extension Methods for <see cref="Package"/> instances.
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Loads a(nother) package for the provided package Id.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="packageToLoadGuid">The unique identifier for the package to load.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool LoadOtherPackage(this Package package, Guid packageToLoadGuid)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            return package.LoadVisualStudioPackage(packageToLoadGuid);
        }
    }
}