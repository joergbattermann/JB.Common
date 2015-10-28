// -----------------------------------------------------------------------
// <copyright file="TeamExplorerSectionBase.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.TeamFoundation.Controls;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    ///     Base class for <see cref="ITeamExplorerSection">Team Explorer Sections</see> - these are basically sub-elements or
    ///     well.. sections
    ///     On a <see cref="ITeamExplorerPage" />. Besides deriving from this class, implementations also have to be marked
    ///     with the <see cref="TeamExplorerSectionAttribute" />
    ///     in order to be picked up by visual studio. As this attribute requires a
    ///     <see cref="TeamExplorerSectionAttribute.ParentPageId" /> - this can be
    ///     a standard TeamExplorer page, known via constants in <see cref="TeamExplorerPageIds" /> OR if one added custom
    ///     pages,
    ///     the <see cref="TeamExplorerPageNavigationItemBase.TargetPageId" /> can / should be used.
    ///     As everything is loaded via .Net's MEF / Composition functionality, sections should typically also be marked as
    ///     <see cref="PartCreationPolicyAttribute.CreationPolicy" /> set to
    ///     <see cref="CreationPolicy.NonShared" />.
    /// </summary>
    [PartNotDiscoverable]
    public abstract class TeamExplorerSectionBase : TeamExplorerContentBase, ITeamExplorerSection
    {
        private bool _isExpanded;
        private bool _isVisible;

        #region Implementation of ITeamExplorerSection

        /// <summary>
        ///     Initializes this instance, primarily handing in the ServiceProvider.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SectionInitializeEventArgs" /> instance containing the event data.</param>
        public virtual void Initialize(object sender, SectionInitializeEventArgs e)
        {
            ServiceProvider = e.ServiceProvider;
        }

        /// <summary>
        ///     This is called whenever the <see cref="ITeamExplorerPage" /> has been loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SectionLoadedEventArgs" /> instance containing the event data.</param>
        public virtual void Loaded(object sender, SectionLoadedEventArgs e)
        {
        }

        /// <summary>
        ///     Called whenever the <see cref="ITeamExplorer" /> instructs <see cref="ITeamExplorerPage">pages</see> to save their
        ///     context (not sure how, what, when and why).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SectionSaveContextEventArgs" /> instance containing the event data.</param>
        public virtual void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
        }

        /// <summary>
        ///     Gets or sets the content of the page - must be a (WPF) UserControl.
        /// </summary>
        /// <value>
        ///     The content of the page.
        /// </value>
        public object SectionContent
        {
            get { return Content; }
            set
            {
                Debug.Assert(value is UserControl, "Must be a (WPF) UserControl");
                Content = (UserControl) value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}