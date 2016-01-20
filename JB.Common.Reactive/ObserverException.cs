// -----------------------------------------------------------------------
// <copyright file="ObserverException.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading;

namespace JB.Reactive
{
    [Serializable()]
    public class ObserverException : Exception
    {
        private long _handled = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ObserverException"/> has been handled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if handled; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Handled has already been set to true - a revert back to false is not permitted.</exception>
        public bool Handled
        {
            get
            {
                return Interlocked.Read(ref _handled) == 1;
            }
            set
            {
                if (Handled == true && value == false)
                {
                    throw new InvalidOperationException("Handled has already been set to true - a revert back to false is not permitted.");
                }

                Interlocked.Exchange(ref _handled, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. </param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ObserverException(string message, Exception innerException)
            : base(message ?? string.Empty, innerException)
        {
            if (innerException == null)
                throw new ArgumentNullException(nameof(innerException));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected ObserverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.Handled = info.GetBoolean(nameof(Handled));
            }
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info?.AddValue(nameof(Handled), this.Handled);
        }
    }
}
