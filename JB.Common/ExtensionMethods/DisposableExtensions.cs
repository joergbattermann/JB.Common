using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JB.ExtensionMethods
{
    /// <summary>
    /// Extension Methods for <see cref="IDisposable"/> instances.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Checks the and disposes the <paramref name="disposable" />, if applicable.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="swallowObjectDisposedException">if set to <c>true</c> this method swallows an <see cref="ObjectDisposedException"/> if it occurs when attempting to call <see cref="IDisposable.Dispose" />, typically meaning the instance has already been disposed elsewhere.</param>
        /// <param name="nullifyAfterDisposal">if set to <c>true</c>, the <paramref name="disposable" /> will be set to [null] after calling <see cref="IDisposable.Dispose" />.</param>
        public static void CheckAndDispose(this IDisposable disposable, bool swallowObjectDisposedException = true, bool nullifyAfterDisposal = true)
        {
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // nothing to do here - a bit dirty but oh well
                    if (swallowObjectDisposedException == false)
                        throw;
                }
                finally
                {
                    if (nullifyAfterDisposal)
                    {
                        disposable = null;
                    }
                }
            }
        }
    }
}
