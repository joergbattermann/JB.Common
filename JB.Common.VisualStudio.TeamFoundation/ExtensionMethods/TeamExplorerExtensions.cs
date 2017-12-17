// -----------------------------------------------------------------------
// <copyright file="TeamExplorerExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using JB.VisualStudio.TeamFoundation.TeamExplorer;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TeamExplorerPageIds = JB.VisualStudio.TeamFoundation.TeamExplorer.TeamExplorerPageIds;

namespace JB.VisualStudio.TeamFoundation.ExtensionMethods
{
    /// <summary>
    ///     Extension Methods for <see cref="ITeamExplorer" /> instances.
    /// </summary>
    public static class TeamExplorerExtensions
    {
        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Builds page, optionally to a specific one for the provided
        ///     <paramref name="buildId" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="buildId">The build identifier.</param>
        /// <returns>The <see cref="ITeamExplorerPage" /> that was navigated to.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToBuildsPage(this ITeamExplorer teamExplorer, int buildId = default(int))
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            if (buildId <= 0)
            {
                return teamExplorer.NavigateToPage(TeamExplorerPageIds.Builds, null);
            }
            else
            {
                return teamExplorer.NavigateToPage<Dictionary<string, object>>(
                    TeamExplorerPageIds.Builds,
                    new Dictionary<string, object>
                    {
                        {TeamExplorerPageContextKeys.Builds, buildId}
                    });
            }
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Connect page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToConnectPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Connect, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Documents page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToDocumentsPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Documents, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Reports page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToReportsPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Reports, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Settings page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToSettingsPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Settings, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the 'Pending Changes' page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToPendingChangesPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.PendingChanges, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Search page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToSearchPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Search, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the 'My Work' page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToMyWorkPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.MyWork, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the 'All Changes' page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToAllChangesPage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.AllChanges, null);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Home page.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ITeamExplorerPage NavigateToHomePage(this ITeamExplorer teamExplorer)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.Home, null);
        }

        /// <summary>
        ///     Navigates to the target <typeparamref name="TTeamExplorerPage" /> for the given <paramref name="pageId" /> and an
        ///     optional <paramref name="contextParameter" />.
        /// </summary>
        /// <typeparam name="TTeamExplorerPage">The type of the team explorer page.</typeparam>
        /// <typeparam name="TContextParameter">The type of the context parameter.</typeparam>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="pageId">The target <see cref="ITeamExplorer" /> page identifier.</param>
        /// <param name="contextParameter">The (context) parameter for the target page navigation.</param>
        /// <returns>The <typeparamref name="TTeamExplorerPage" /> that was navigated to.</returns>
        public static TTeamExplorerPage NavigateToPage<TTeamExplorerPage, TContextParameter>(this ITeamExplorer teamExplorer, Guid pageId, TContextParameter contextParameter = default(TContextParameter))
            where TTeamExplorerPage : class, ITeamExplorerPage
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            if (Equals(pageId, Guid.Empty))
                throw new ArgumentException($"'{nameof(pageId)}' cannot be an empty {nameof(Guid)}.");

            return teamExplorer.NavigateToPage(pageId, contextParameter) as TTeamExplorerPage;
        }

        /// <summary>
        ///     Navigates to the target <see cref="ITeamExplorerPage" /> for the given <paramref name="pageId" /> and an optional
        ///     <paramref name="contextParameter" />.
        /// </summary>
        /// <typeparam name="TContextParameter">The type of the context parameter.</typeparam>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="pageId">The target <see cref="ITeamExplorer" /> page identifier.</param>
        /// <param name="contextParameter">The (context) parameter for the target page navigation.</param>
        /// <returns>The <see cref="ITeamExplorerPage" /> that was navigated to.</returns>
        public static ITeamExplorerPage NavigateToPage<TContextParameter>(this ITeamExplorer teamExplorer, Guid pageId, TContextParameter contextParameter = default(TContextParameter))
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));

            if (Equals(pageId, Guid.Empty))
                return null;

            return teamExplorer.NavigateToPage<ITeamExplorerPage, TContextParameter>(pageId, contextParameter);
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the (Request) Code Review Page, particularly for the given
        ///     <paramref name="workspace" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="workspace">The <see cref="Workspace" />.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static ITeamExplorerPage NavigateToRequestCodeReviewPage(this ITeamExplorer teamExplorer, Workspace workspace)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.RequestCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewWorkspace, workspace}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the (Request) Code Review Page, particularly for the given
        ///     <paramref name="shelveset" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="shelveset">The shelveset.</param>
        /// <param name="shelvesetExcludedChangesCount">The shelveset excluded changes count.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentException">$'{nameof(shelvesetExcludedChangesCount)}' must be 0 or higher</exception>
        public static ITeamExplorerPage NavigateToRequestCodeReviewPage(this ITeamExplorer teamExplorer, Shelveset shelveset, int shelvesetExcludedChangesCount = 0)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (shelveset == null)
                throw new ArgumentNullException(nameof(shelveset));
            if (shelvesetExcludedChangesCount < 0)
                throw new ArgumentException($"'{nameof(shelvesetExcludedChangesCount)}' must be 0 or higher");

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.RequestCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewShelveset, shelveset},
                    {TeamExplorerPageContextKeys.CodeReviewShelvesetExcludedCount, shelvesetExcludedChangesCount}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the (Request) Code Review Page, particularly for the given
        ///     <paramref name="shelvesetName" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="shelvesetName">Name of the shelveset.</param>
        /// <param name="shelvesetExcludedChangesCount">The shelveset excluded changes count.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">
        ///     $'{nameof(shelvesetName)}' must be a valid Shelveset name
        ///     or
        ///     $'{nameof(shelvesetExcludedChangesCount)}' must be 0 or higher
        /// </exception>
        public static ITeamExplorerPage NavigateToRequestCodeReviewPage(this ITeamExplorer teamExplorer, string shelvesetName, int shelvesetExcludedChangesCount = 0)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (string.IsNullOrWhiteSpace(shelvesetName))
                throw new ArgumentException($"'{nameof(shelvesetName)}' must be a valid Shelveset name");
            if (shelvesetExcludedChangesCount < 0)
                throw new ArgumentException($"'{nameof(shelvesetExcludedChangesCount)}' must be 0 or higher");

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.RequestCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewShelvesetName, shelvesetName},
                    {TeamExplorerPageContextKeys.CodeReviewShelvesetExcludedCount, shelvesetExcludedChangesCount}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the (Request) Code Review Page, particularly for the given
        ///     <paramref name="changesetId" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="changesetId">The <see cref="Changeset.ChangesetId" />.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">$'{nameof(changesetId)}' must be a valid Changeset Id</exception>
        public static ITeamExplorerPage NavigateToRequestCodeReviewPage(this ITeamExplorer teamExplorer, int changesetId)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (changesetId <= 0)
                throw new ArgumentException($"'{nameof(changesetId)}' must be a valid Changeset Id");

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.RequestCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewChangesetId, changesetId}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the (Request) Code Review Page, particularly for the given
        ///     <paramref name="changeset" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="changeset">The <see cref="Changeset" />.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static ITeamExplorerPage NavigateToRequestCodeReviewPage(this ITeamExplorer teamExplorer, Changeset changeset)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (changeset == null)
                throw new ArgumentNullException(nameof(changeset));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.RequestCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewChangesetId, changeset.ChangesetId}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Code Review Page, particularly for the given
        ///     <paramref name="codeReviewWorkItem" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="codeReviewWorkItem">The code review work item.</param>
        /// <returns>The <see cref="ITeamExplorerPage" /> that was navigated to.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static ITeamExplorerPage NavigateToViewCodeReviewPage(this ITeamExplorer teamExplorer, WorkItem codeReviewWorkItem)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (codeReviewWorkItem == null)
                throw new ArgumentNullException(nameof(codeReviewWorkItem));

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.ViewCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewWorkItem, codeReviewWorkItem}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Code Review Page, particularly for the given
        ///     <paramref name="codeReviewWorkItemId" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="codeReviewWorkItemId">The code review work item identifier.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">$'{nameof(codeReviewWorkItemId)}' must be a valid Work Item Id</exception>
        public static ITeamExplorerPage NavigateToViewCodeReviewPage(this ITeamExplorer teamExplorer, int codeReviewWorkItemId)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (codeReviewWorkItemId <= 0)
                throw new ArgumentException($"'{nameof(codeReviewWorkItemId)}' must be a valid Work Item Id");

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.ViewCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewWorkItemId, codeReviewWorkItemId}
                });
        }

        /// <summary>
        ///     Navigates the <see cref="ITeamExplorer" /> to the Code Review Page, particularly for the given
        ///     <paramref name="codeReviewWorkItemId" />.
        /// </summary>
        /// <param name="teamExplorer">The team explorer.</param>
        /// <param name="codeReviewWorkItemId">The code review work item identifier.</param>
        /// <returns>
        ///     The <see cref="ITeamExplorerPage" /> that was navigated to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">$'{nameof(workItemId)}' must be a valid Work Item Id</exception>
        public static ITeamExplorerPage NavigateToViewCodeReviewPage(this ITeamExplorer teamExplorer, string codeReviewWorkItemId)
        {
            if (teamExplorer == null)
                throw new ArgumentNullException(nameof(teamExplorer));
            if (string.IsNullOrWhiteSpace(codeReviewWorkItemId))
                throw new ArgumentException($"'{nameof(codeReviewWorkItemId)}' must be a valid Work Item Id");

            return teamExplorer.NavigateToPage(TeamExplorerPageIds.ViewCodeReview,
                new Dictionary<string, object>
                {
                    {TeamExplorerPageContextKeys.CodeReviewWorkItemId, codeReviewWorkItemId}
                });
        }
    }
}