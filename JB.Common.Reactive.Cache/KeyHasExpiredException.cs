using System;
using System.Runtime.Serialization;

namespace JB.Reactive.Cache
{
    [Serializable()]
    public class KeyHasExpiredException<TKey> : Exception
    {
        /// <summary>
        /// Gets or sets the expired key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        private TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> the <see cref="Key"/> has expired.
        /// </summary>
        /// <value>
        /// The <see cref="DateTime"/> the <see cref="Key"/> has expired.
        /// </value>
        private DateTime ExpiredAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHasExpiredException{TKey}"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. </param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public KeyHasExpiredException(string message, Exception innerException)
            : base(message ?? string.Empty, innerException)
        {
            if (innerException == null)
                throw new ArgumentNullException(nameof(innerException));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHasExpiredException{TKey}"/> class.
        /// </summary>
        /// <param name="key">The expired key.</param>
        /// <param name="expiredAt">The <see cref="DateTime"/> the <see cref="Key"/> has expired.</param>
        /// <param name="message">The message that describes the error. </param>
        public KeyHasExpiredException(TKey key, DateTime expiredAt, string message = "The key has already expired.")
            : base(message ?? string.Empty)
        {
            Key = key;
            ExpiredAt = expiredAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHasExpiredException{TKey}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected KeyHasExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.Key = ((TKey)info.GetValue(nameof(Key), typeof(TKey)));
                this.ExpiredAt = info.GetDateTime(nameof(ExpiredAt));
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
            info?.AddValue(nameof(ExpiredAt), this.ExpiredAt, typeof(DateTime));
        }
    }
}