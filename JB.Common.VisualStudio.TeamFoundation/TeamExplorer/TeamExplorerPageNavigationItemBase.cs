// -----------------------------------------------------------------------
// <copyright file="TeamExplorerPageNavigationItemBase.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    ///     This is merely a helper wrapper around <see cref="TeamExplorerNavigationItemBase" />, typically to be used as a
    ///     base class for
    ///     <see cref="ITeamExplorerNavigationItem" /> instaces that navigate to a target page, more particularly the
    ///     <see cref="TeamExplorerNavigationItemAttribute" /> the classes
    ///     have to be marked with has a <see cref="TeamExplorerNavigationItemAttribute.TargetPageId" />, which should
    ///     reference an existing <see cref="ITeamExplorerPage" />'s Id.
    ///     to navigate to and from.
    /// </summary>
    [PartNotDiscoverable]
    public abstract class TeamExplorerPageNavigationItemBase : TeamExplorerNavigationItemBase
    {
        /// <summary>
        ///     Gets the page identifier.
        /// </summary>
        /// <value>
        ///     The page identifier.
        /// </value>
        public Guid TargetPageId { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamExplorerPageNavigationItemBase" /> class.
        /// </summary>
        /// <param name="targetPageId">The page identifier.</param>
        protected TeamExplorerPageNavigationItemBase(string targetPageId)
        {
            if (string.IsNullOrWhiteSpace(targetPageId)) throw new ArgumentOutOfRangeException(nameof(targetPageId));

            TargetPageId = new Guid(targetPageId);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamExplorerPageNavigationItemBase" /> class.
        /// </summary>
        /// <param name="targetPageId">The page identifier.</param>
        protected TeamExplorerPageNavigationItemBase(Guid targetPageId)
        {
            if (Equals(targetPageId, Guid.Empty)) throw new ArgumentOutOfRangeException(nameof(targetPageId));

            TargetPageId = targetPageId;
        }

        /// <summary>
        /// Whenever this <see cref="ITeamExplorerNavigationItem"/> shall execute its activity, it navigates to the <see cref="TargetPageId"/>.
        /// </summary>
        public override void Execute()
        {
            TeamExplorerUtils.Instance.NavigateToPage(TargetPageId.ToString(), TeamExplorer, null);
        }
    }
}