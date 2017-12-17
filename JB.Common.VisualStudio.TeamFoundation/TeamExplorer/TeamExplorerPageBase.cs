// -----------------------------------------------------------------------
// <copyright file="TeamExplorerPageBase.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Controls;
using Microsoft.TeamFoundation.Controls;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    ///     <see cref="ITeamExplorerPage" /> are, as the name implies, entire pages one can navigate to and display
    ///     <see cref="PageContent" /> on.
    ///     Besides or instead of its 'main' <see cref="PageContent" />, each page can also have (multiple)
    ///     <see cref="ITeamExplorerSection">sections</see>.
    /// </summary>
    [PartNotDiscoverable]
    public abstract class TeamExplorerPageBase : TeamExplorerContentBase, ITeamExplorerPage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamExplorerPageBase" /> class.
        /// </summary>
        protected TeamExplorerPageBase()
        {
            this.SetProperty(PageProperties.HorizontalScrollBarVisible, true);
        }

        #region Implementation of ITeamExplorerPage

        /// <summary>
        ///     Gets or sets the content of the page - must be a (WPF) UserControl.
        /// </summary>
        /// <value>
        ///     The content of the page.
        /// </value>
        public object PageContent
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
        ///     Initializes this instance, primarily handing in the ServiceProvider.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PageInitializeEventArgs" /> instance containing the event data.</param>
        public virtual void Initialize(object sender, PageInitializeEventArgs e)
        {
            ServiceProvider = e.ServiceProvider;
        }

        /// <summary>
        ///     This is called whenever the <see cref="ITeamExplorerPage" /> has been loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PageLoadedEventArgs" /> instance containing the event data.</param>
        public virtual void Loaded(object sender, PageLoadedEventArgs e)
        {
        }

        /// <summary>
        ///     Called whenever the <see cref="ITeamExplorer" /> instructs <see cref="ITeamExplorerPage">pages</see> to save their
        ///     context (not sure how, what, when and why).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PageSaveContextEventArgs" /> instance containing the event data.</param>
        public virtual void SaveContext(object sender, PageSaveContextEventArgs e)
        {
        }

        #endregion
    }
}