using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JB.Collections
{
    /// <summary>
    /// An exception indicating that a given <see cref="Key"/> already existed, typically in an <see cref="IDictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    [Serializable()]
    public class KeyAlreadyExistsException<TKey> : Exception
    {
        /// <summary>
        /// Gets the key that wasn't found.
        /// </summary>
        /// <value>
        /// The key that wasn't found.
        /// </value>
        public TKey Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyExistsException{TKey}" /> class.
        /// </summary>
        /// <param name="key">The key that was not found.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public KeyAlreadyExistsException(TKey key, string message = null, Exception innerException = null)
            : base(message ?? "An element with the same key already exists.", innerException)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyExistsException{TKey}"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. </param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public KeyAlreadyExistsException(string message = null, Exception innerException = null)
            : base(message ?? "An element with the same key already exists.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyExistsException{TKey}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected KeyAlreadyExistsException(SerializationInfo info, StreamingContext context)
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