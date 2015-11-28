// -----------------------------------------------------------------------
// <copyright file="CountAnalyzer.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Reactive.Subjects;
using JB.Reactive.Analytics.AnalysisResults;

namespace JB.Reactive.Analytics.Analyzers
{
    public class CountAnalyzer<TSource> : IAnalyzer<TSource>, IAnalyzer<TSource, ICountBasedAnalysisResult>, IDisposable
    {
    }
}