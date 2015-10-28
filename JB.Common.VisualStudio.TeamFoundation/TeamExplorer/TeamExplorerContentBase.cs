// -----------------------------------------------------------------------
// <copyright file="TeamExplorerContentBase.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    [PartNotDiscoverable]
    public abstract class TeamExplorerContentBase : TeamExplorerBase
    {
        private bool _isBusy;
        private string _title;
        private UserControl _userControl;

        /// <summary>
        ///     Gets or sets the content of the page - must be a <see cref="UserControl" />.
        /// </summary>
        /// <value>
        ///     The content of the page.
        /// </value>
        public UserControl Content
        {
            get { return _userControl; }
            internal set
            {
                _userControl = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title
        {
            get { return _title; }
            protected set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Called whenever cancellation has been requested.
        /// </summary>
        public virtual void Cancel()
        {
        }

        /// <summary>
        ///     Gets the extensibility service (???!?).
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        /// <summary>
        ///     Called when this instance shall be refreshed.
        /// </summary>
        public virtual void Refresh()
        {
        }
    }
}