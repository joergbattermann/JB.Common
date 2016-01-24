// -----------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Runtime.ExceptionServices;

namespace JB.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// (Re-)Throws the given <paramref name="exception"/> with the correct stack trace using
        /// <see cref="ExceptionDispatchInfo"/>, as long as it isn't [null].
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void ThrowIfNotNull(this Exception exception)
        {
            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}