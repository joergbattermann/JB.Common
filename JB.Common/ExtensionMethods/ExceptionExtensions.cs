// -----------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Throws the <paramref name="exception"/> if it isn't null.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void ThrowIfNotNull(this Exception exception)
        {
            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}