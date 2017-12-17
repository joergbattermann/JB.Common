// -----------------------------------------------------------------------
// <copyright file="ForwardedEventArgs.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;

namespace JB
{
    public class ForwardedEventArgs<TArgs> where TArgs : EventArgs
    {
        /// <summary>
        /// Gets the original event arguments.
        /// </summary>
        /// <value>
        /// The original event arguments.
        /// </value>
        public TArgs OriginalEventArgs { get; }

        /// <summary>
        /// Gets the original sender.
        /// </summary>
        /// <value>
        /// The original sender.
        /// </value>
        public object OriginalSender { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedEventArgs{TArgs}"/> class.
        /// </summary>
        /// <param name="originalSender">The original sender.</param>
        /// <param name="originalEventArgs">The original event arguments.</param>
        public ForwardedEventArgs(object originalSender, TArgs originalEventArgs)
        {
            OriginalSender = originalSender;
            OriginalEventArgs = originalEventArgs;
        }
    }
}