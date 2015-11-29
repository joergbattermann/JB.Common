// -----------------------------------------------------------------------
// <copyright file="CountAnalysisResult.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------
namespace JB.Reactive.Analytics.AnalysisResults
{
    public class CountAnalysisResult : ICountBasedAnalysisResult
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
    }
}