// -----------------------------------------------------------------------
// <copyright file="CountAnalysisResult.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace JB.Reactive.Analytics.AnalysisResults
{
    [DebuggerDisplay("{" + nameof(Count) + "}")]
    public class CountAnalysisResult : ICountBasedAnalysisResult, IEquatable<CountAnalysisResult>, IEquatable<ICountBasedAnalysisResult>
    {
        #region Implementation of ICountBasedAnalysisResult

        /// <summary>
        /// Gets the elements count.
        /// </summary>
        /// <value>
        /// The elements count.
        /// </value>
        public long Count { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CountAnalysisResult"/> class.
        /// </summary>
        /// <param name="count">The count.</param>
        public CountAnalysisResult(long count)
        {
            Count = count;
        }

        #region Implementation of IEquatable<CountAnalysisResult>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(CountAnalysisResult other)
        {
            if (other == null)
                return false;

            return Equals(Count, other.Count);
        }

        #endregion

        #region Implementation of IEquatable<ICountBasedAnalysisResult>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ICountBasedAnalysisResult other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(Count, other.Count);
        }

        #endregion
    }
}