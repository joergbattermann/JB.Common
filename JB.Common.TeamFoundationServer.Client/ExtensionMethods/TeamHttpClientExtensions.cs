// -----------------------------------------------------------------------
// <copyright file="TeamHttpClientExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace JB.Common.TeamFoundationServer.Client.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods for <see cref="TeamHttpClient"/> instances.
    /// </summary>
    public static class TeamHttpClientExtensions
    {
        /// <summary>
        /// Gets all teams for the given <paramref name="project"/>.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient"/> to use.</param>
        /// <param name="project">The project.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static Task<IList<WebApiTeam>> GetAllTeams(this TeamHttpClient client, TeamProject project, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            return client.GetAllTeams(project.Id, pageSize, userState, cancellationToken);
        }

        /// <summary>
        /// Gets all teams for the given <paramref name="projectId"/>.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient"/> to use.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static Task<IList<WebApiTeam>> GetAllTeams(this TeamHttpClient client, Guid projectId, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (Equals(Guid.Empty, projectId))
                throw new ArgumentOutOfRangeException(nameof(projectId));

            return client.GetAllTeams(projectId.ToString(), pageSize, userState, cancellationToken);
        }

        /// <summary>
        /// Gets all teams for the given <paramref name="projectId"/>.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient"/> to use.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static async Task<IList<WebApiTeam>> GetAllTeams(this TeamHttpClient client, string projectId, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));

            var result = new List<WebApiTeam>();

            int currentPage = 0;
            var teamsForCurrentPage = (await client.GetTeamsAsync(projectId, pageSize, currentPage, userState, cancellationToken).ConfigureAwait(false)).ToList();
            while (teamsForCurrentPage.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.AddRange(teamsForCurrentPage);

                // check whether the recently returned item(s) were less than the max page size
                if (teamsForCurrentPage.Count < pageSize)
                    break; // if so, break the loop as we've read all instances

                // otherwise continue
                cancellationToken.ThrowIfCancellationRequested();
                teamsForCurrentPage = (await client.GetTeamsAsync(projectId, pageSize, currentPage, userState, cancellationToken).ConfigureAwait(false)).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }

        /// <summary>
        /// Gets all team members for the given <paramref name="project" /> and <paramref name="team" />.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient" /> to use.</param>
        /// <param name="connection">The connection for the <paramref name="client" /> that will be used to retrieve the identities for the team members.</param>
        /// <param name="project">The project.</param>
        /// <param name="team">The team.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static Task<IReadOnlyCollection<Identity>> GetAllTeamMembers(this TeamHttpClient client, VssConnection connection, TeamProject project, WebApiTeam team, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (team == null)
                throw new ArgumentNullException(nameof(team));

            return client.GetAllTeamMembers(connection, project.Id, team.Id, pageSize, userState, cancellationToken);
        }

        /// <summary>
        /// Gets all team members for the given <paramref name="projectId" /> and <paramref name="teamId"/>.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient" /> to use.</param>
        /// <param name="connection">The connection for the <paramref name="client"/> that will be used to retrieve the identities for the team members.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="teamId">The team identifier whose members to retrieve.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentException">$The '{nameof(connection)}' parameter must be for the given '{nameof(client)}'</exception>
        public static Task<IReadOnlyCollection<Identity>> GetAllTeamMembers(this TeamHttpClient client, VssConnection connection, Guid projectId, Guid teamId, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (Equals(Guid.Empty, projectId))
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (Equals(Guid.Empty, teamId))
                throw new ArgumentOutOfRangeException(nameof(teamId));

            return client.GetAllTeamMembers(connection, projectId.ToString(), teamId.ToString(), pageSize, userState, cancellationToken);
        }

        /// <summary>
        /// Gets all team members for the given <paramref name="projectId" /> and <paramref name="teamId"/>.
        /// </summary>
        /// <param name="client">The <see cref="TeamHttpClient" /> to use.</param>
        /// <param name="connection">The connection for the <paramref name="client"/> that will be used to retrieve the identities for the team members.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="teamId">The team identifier whose members to retrieve.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.ArgumentException">$The '{nameof(connection)}' parameter must be for the given '{nameof(client)}'</exception>
        public static async Task<IReadOnlyCollection<Identity>> GetAllTeamMembers(this TeamHttpClient client, VssConnection connection, string projectId, string teamId, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));
            if (teamId == null)
                throw new ArgumentNullException(nameof(teamId));
            
            if(Equals(client.BaseAddress, connection.Uri))
                throw new ArgumentException($"The '{nameof(connection)}' parameter must be for the base uri of the VSTS / TFS system", nameof(connection));

            var result = new List<Identity>();
            var identityReferences = new List<IdentityRef>();

            int currentPage = 0;
            var teamMembersForCurrentPage = (await client.GetTeamMembersAsync(projectId, teamId, pageSize, currentPage, userState, cancellationToken).ConfigureAwait(false)).ToList();
            while (teamMembersForCurrentPage.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                identityReferences.AddRange(teamMembersForCurrentPage);

                // check whether the recently returned item(s) were less than the max page size
                if (teamMembersForCurrentPage.Count < pageSize)
                    break; // if so, break the loop as we've read all instances

                // otherwise continue
                cancellationToken.ThrowIfCancellationRequested();
                teamMembersForCurrentPage = (await client.GetTeamMembersAsync(projectId, teamId, pageSize, currentPage, userState, cancellationToken).ConfigureAwait(false)).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (identityReferences.Count > 0)
            {
                using (var identityHttpClient = await connection.GetClientAsync<IdentityHttpClient>(cancellationToken).ConfigureAwait(false))
                {
                    result.AddRange(await identityHttpClient.ReadIdentitiesAsync(identityReferences.Select(identityReference => new Guid(identityReference.Id)).ToList(), cancellationToken: cancellationToken).ConfigureAwait(false));
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
    }
}