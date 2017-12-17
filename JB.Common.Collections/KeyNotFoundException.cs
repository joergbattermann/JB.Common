// -----------------------------------------------------------------------
// <copyright file="KeyNotFoundException.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace JB.Collections
{
    /// <summary>
    /// A <see cref="System.Collections.Generic.KeyNotFoundException"/> that provides the missing <see cref="Key"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    [Serializable()]
    public class KeyNotFoundException<TKey> : System.Collections.Generic.KeyNotFoundException
    {
        /// <summary>
        /// Gets the key that wasn't found.
        /// </summary>
        /// <value>
        /// The key that wasn't found.
        /// </value>
        public TKey Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyNotFoundException{TKey}" /> class.
        /// </summary>
        /// <param name="key">The key that was not found.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public KeyNotFoundException(TKey key, string message = null, Exception innerException = null)
            : base(message ?? "The given key was not found.", innerException)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyNotFoundException{TKey}"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. </param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public KeyNotFoundException(string message = null, Exception innerException = null)
            : base(message ?? "The given key was not found.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyNotFoundException{TKey}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected KeyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.Key = ((TKey)info.GetValue(nameof(Key), typeof(TKey)));
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

            info?.AddValue(nameof(Key), this.Key, typeof(TKey));
        }
    }
}