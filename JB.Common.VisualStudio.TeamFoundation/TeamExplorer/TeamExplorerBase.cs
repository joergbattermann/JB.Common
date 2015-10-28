// -----------------------------------------------------------------------
// <copyright file="TeamExplorerBase.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using JB.ExtensionMethods;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    ///     (Very) Base class for all other ITeamExplorer* base classes
    /// </summary>
    [PartNotDiscoverable]
    public abstract class TeamExplorerBase : INotifyPropertyChanged, IDisposable
    {
        private IServiceProvider _serviceProvider;
        private long _teamFoundationContextChangedSubscribed;

        /// <summary>
        ///     Gets the team explorer page.
        /// </summary>
        /// <value>
        ///     The team explorer page.
        /// </value>
        protected ITeamExplorerPage CurrentTeamExplorerPage => ServiceProvider?.GetService<ITeamExplorerPage>();

        /// <summary>
        ///     Gets the current context.
        /// </summary>
        /// <value>
        ///     The current context.
        /// </value>
        protected ITeamFoundationContext CurrentTeamFoundationContext
        {
            get
            {
                Debug.Assert(ServiceProvider != null, string.Format(".{0} accessed before .{1} has been set", nameof(CurrentTeamFoundationContext), nameof(ServiceProvider)));
                return TeamFoundationContextManager?.CurrentContext;
            }
        }

        /// <summary>
        ///     Gets or sets the service provider.
        /// </summary>
        /// <value>
        ///     The service provider.
        /// </value>
        protected IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                if (_serviceProvider != null)
                {
                    UnsubscribeTeamFoundationContextChanges();
                }

                _serviceProvider = value;

                if (_serviceProvider != null)
                {
                    SubscribeTeamFoundationContextChanges();
                }
            }
        }

        /// <summary>
        ///     Gets the team explorer.
        /// </summary>
        /// <value>
        ///     The team explorer.
        /// </value>
        protected ITeamExplorer TeamExplorer
        {
            get
            {
                Debug.Assert(ServiceProvider != null, string.Format(".{0} accessed before .{1} has been set", nameof(TeamExplorer), nameof(ServiceProvider)));

                return ServiceProvider.GetService<ITeamExplorer>();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [team foundation context changed subscribed].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [team foundation context changed subscribed]; otherwise, <c>false</c>.
        /// </value>
        protected bool TeamFoundationContextChangedSubscribed
        {
            get { return Interlocked.Read(ref _teamFoundationContextChangedSubscribed) == 1; }
            private set { Interlocked.Exchange(ref _teamFoundationContextChangedSubscribed, value ? 1 : 0); }
        }

        /// <summary>
        ///     Gets the team foundation context manager.
        /// </summary>
        /// <value>
        ///     The team foundation context manager.
        /// </value>
        protected ITeamFoundationContextManager4 TeamFoundationContextManager
        {
            get
            {
                Debug.Assert(ServiceProvider != null, string.Format(".{0} accessed before .{1} has been set", nameof(TeamFoundationContextManager), nameof(ServiceProvider)));

                return ServiceProvider.GetService<ITeamFoundationContextManager4>();
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Clears any notifications in the team explorer.
        /// </summary>
        /// <param name="teamExplorerPage">
        ///     The team explorer page. If none is provided, the <see cref="CurrentTeamExplorerPage" />
        ///     will be used.
        /// </param>
        protected void ClearNotificationsInTeamExplorer(ITeamExplorerPage teamExplorerPage = null)
        {
            TeamExplorerUtils.Instance.ClearNotifications(ServiceProvider, teamExplorerPage);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeTeamFoundationContextChanges();
            }
        }

        /// <summary>
        ///     Hides the notification for the <paramref name="notificationId" /> in the team explorer.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        /// <returns></returns>
        protected bool HideNotificationInTeamExplorer(Guid notificationId)
        {
            return TeamExplorerUtils.Instance.HideNotification(ServiceProvider, notificationId);
        }

        /// <summary>
        ///     Determines whether the notification for the <paramref name="notificationId" /> is currently (still) shown in the
        ///     team explorer.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        /// <returns></returns>
        protected bool IsNotificationVisibleInTeamExplorer(Guid notificationId)
        {
            return TeamExplorerUtils.Instance.IsNotificationVisible(ServiceProvider, notificationId);
        }

        /// <summary>
        ///     Navigates to the team explorer page for the given <paramref name="pageId" />.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        protected void NavigateToTeamExplorerPage(Guid pageId)
        {
            Debug.Assert(pageId != default(Guid), "Cannot use an empty pageId");

            NavigateToTeamExplorerPage(pageId.ToString(), TeamExplorerUtils.NavigateOptions.None);
        }


        /// <summary>
        ///     Navigates to the team explorer page for the given <paramref name="pageId" />.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="options">The options.</param>
        protected void NavigateToTeamExplorerPage(Guid pageId, TeamExplorerUtils.NavigateOptions options)
        {
            Debug.Assert(pageId != default(Guid), "Cannot use an empty pageId");

            TeamExplorerUtils.Instance.NavigateToPage(pageId.ToString(), ServiceProvider, null, options);
        }

        /// <summary>
        ///     Navigates to the team explorer page for the given <paramref name="pageId" />.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        protected void NavigateToTeamExplorerPage(string pageId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(pageId), "Cannot use an empty pageId");

            NavigateToTeamExplorerPage(pageId, TeamExplorerUtils.NavigateOptions.None);
        }

        /// <summary>
        ///     Navigates to the team explorer page for the given <paramref name="pageId" />.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="options">The options.</param>
        protected void NavigateToTeamExplorerPage(string pageId, TeamExplorerUtils.NavigateOptions options)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(pageId), "Cannot use an empty pageId");

            TeamExplorerUtils.Instance.NavigateToPage(pageId, ServiceProvider, null, options);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(propertyName), string.Format("{0} Should not be null or empty", nameof(propertyName)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Shows the error message in the team explorer.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="teamExplorerPage">
        ///     The team explorer page. If none / [null] is provided, it will be displayed on the
        ///     <see cref="CurrentTeamExplorerPage">currently active page</see>.
        /// </param>
        /// <param name="commandToExecuteOnClick">
        ///     The command to execute on click. Use [null] for no command / action to perform on
        ///     click.
        /// </param>
        /// <param name="notificationId">
        ///     The notification identifier. Optional, if one is provided it can either be used in
        ///     <see cref="IsNotificationVisibleInTeamExplorer" /> or <see cref="HideNotificationInTeamExplorer" />.
        /// </param>
        protected void ShowErrorInTeamExplorer(string errorMessage, ITeamExplorerPage teamExplorerPage = null, ICommand commandToExecuteOnClick = null, Guid notificationId = default(Guid))
        {
            ShowNotificationInTeamExplorer(errorMessage, NotificationType.Error, teamExplorerPage, commandToExecuteOnClick, notificationId);
        }

        /// <summary>
        ///     Shows the (information) message in the team explorer.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="teamExplorerPage">
        ///     The team explorer page. If none / [null] is provided, it will be displayed on the
        ///     <see cref="CurrentTeamExplorerPage">currently active page</see>.
        /// </param>
        /// <param name="commandToExecuteOnClick">
        ///     The command to execute on click. Use [null] for no command / action to perform on
        ///     click.
        /// </param>
        /// <param name="notificationId">
        ///     The notification identifier. Optional, if one is provided it can either be used in
        ///     <see cref="IsNotificationVisibleInTeamExplorer" /> or <see cref="HideNotificationInTeamExplorer" />.
        /// </param>
        protected void ShowMessageInTeamExplorer(string message, ITeamExplorerPage teamExplorerPage = null, ICommand commandToExecuteOnClick = null, Guid notificationId = default(Guid))
        {
            ShowNotificationInTeamExplorer(message, NotificationType.Information, teamExplorerPage, commandToExecuteOnClick, notificationId);
        }

        /// <summary>
        ///     Shows a notification in the team explorer.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="notificationType">The notification type.</param>
        /// <param name="teamExplorerPage">
        ///     The team explorer page. If none / [null] is provided, it will be displayed on the
        ///     <see cref="CurrentTeamExplorerPage">currently active page</see>.
        /// </param>
        /// <param name="commandToExecuteOnClick">
        ///     The command to execute on click. Use [null] for no command / action to perform on
        ///     click.
        /// </param>
        /// <param name="notificationId">
        ///     The notification identifier. Optional, if one is provided it can either be used in
        ///     <see cref="IsNotificationVisibleInTeamExplorer" /> or <see cref="HideNotificationInTeamExplorer" />.
        /// </param>
        /// <param name="notificationFlags">The notification flags.</param>
        protected void ShowNotificationInTeamExplorer(string message, NotificationType notificationType = NotificationType.Information, ITeamExplorerPage teamExplorerPage = null, ICommand commandToExecuteOnClick = null, Guid notificationId = default(Guid), NotificationFlags notificationFlags = NotificationFlags.None)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(message), "No empty notifications, please.");

            TeamExplorerUtils.Instance.ShowNotification(ServiceProvider, message, notificationType, notificationFlags, commandToExecuteOnClick, notificationId == default(Guid) ? Guid.NewGuid() : notificationId, teamExplorerPage);
        }

        /// <summary>
        ///     Shows the warning message in the team explorer.
        /// </summary>
        /// <param name="warningMessage">The warning message.</param>
        /// <param name="teamExplorerPage">
        ///     The team explorer page. If none / [null] is provided, it will be displayed on the
        ///     <see cref="CurrentTeamExplorerPage">currently active page</see>.
        /// </param>
        /// <param name="commandToExecuteOnClick">
        ///     The command to execute on click. Use [null] for no command / action to perform on
        ///     click.
        /// </param>
        /// <param name="notificationId">
        ///     The notification identifier. Optional, if one is provided it can either be used in
        ///     <see cref="IsNotificationVisibleInTeamExplorer" /> or <see cref="HideNotificationInTeamExplorer" />.
        /// </param>
        protected void ShowWarningInTeamExplorer(string warningMessage, ITeamExplorerPage teamExplorerPage = null, ICommand commandToExecuteOnClick = null, Guid notificationId = default(Guid))
        {
            ShowNotificationInTeamExplorer(warningMessage, NotificationType.Warning, teamExplorerPage, commandToExecuteOnClick, notificationId);
        }

        /// <summary>
        ///     Subscribes from <see cref="ITeamFoundationContextManager.ContextChanging" /> and
        ///     <see cref="ITeamFoundationContextManager.ContextChanged" /> events.
        /// </summary>
        protected void SubscribeTeamFoundationContextChanges()
        {
            if (ServiceProvider == null || TeamFoundationContextChangedSubscribed)
            {
                return;
            }

            if (TeamFoundationContextManager != null)
            {
                TeamFoundationContextManager.ContextChanging += TeamFoundationContextChanging;
                TeamFoundationContextManager.ContextChanged += TeamFoundationContextChanged;

                TeamFoundationContextChangedSubscribed = true;
            }
        }

        /// <summary>
        ///     Event handler for the <see cref="ITeamFoundationContextManager.ContextChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ContextChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void TeamFoundationContextChanged(object sender, ContextChangedEventArgs e)
        {
        }


        /// <summary>
        ///     Event handler for the <see cref="ITeamFoundationContextManager.ContextChanging" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ContextChangingEventArgs" /> instance containing the event data.</param>
        protected virtual void TeamFoundationContextChanging(object sender, ContextChangingEventArgs e)
        {
        }

        /// <summary>
        ///     Unsubscribes from <see cref="ITeamFoundationContextManager.ContextChanging" /> and
        ///     <see cref="ITeamFoundationContextManager.ContextChanged" /> events.
        /// </summary>
        protected void UnsubscribeTeamFoundationContextChanges()
        {
            if (ServiceProvider == null || !TeamFoundationContextChangedSubscribed)
            {
                return;
            }

            if (TeamFoundationContextManager != null)
            {
                TeamFoundationContextManager.ContextChanging -= TeamFoundationContextChanging;
                TeamFoundationContextManager.ContextChanged -= TeamFoundationContextChanged;

                TeamFoundationContextChangedSubscribed = false;
            }
        }
    }
}