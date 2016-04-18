using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;

namespace JB.Common.TeamFoundationServer.Client.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods for <see cref="ProjectHttpClient"/> instances.
    /// </summary>
    public static class ProjectHttpClientExtensions
    {
        /// <summary>
        /// Get the project with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="client">The <see cref="ProjectHttpClient"/> to use.</param>
        /// <param name="id">The project <paramref name="id"/>to search for.</param>
        /// <param name="includeCapabilities">Include capabilities (such as source control) in the team project result.</param>
        /// <param name="userState">The user state object.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static Task<TeamProject> GetProject(this ProjectHttpClient client, Guid id, bool? includeCapabilities = null, object userState = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (Equals(Guid.Empty, id)) throw new ArgumentException(nameof(id));

            return client.GetProject(id.ToString(), includeCapabilities, includeHistory: false, userState: userState);
        }

        /// <summary>
        /// Gets all projects.
        /// </summary>
        /// <param name="client">The <see cref="ProjectHttpClient"/> to use.</param>
        /// <param name="stateFilter">Filter on team projects in a specific team project state.</param>
        /// <param name="pageSize">Page size to use while retrieving the projects.</param>
        /// <param name="includeCapabilities">Include capabilities (such as source control) in the team project result.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static async Task<IList<TeamProject>> GetAllProjects(this ProjectHttpClient client, ProjectState? stateFilter = null, int pageSize = 25, bool? includeCapabilities = null, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var result = new List<TeamProject>();

            int currentPage = 0;
            var currentProjectReferences = (await client.GetProjects(stateFilter, pageSize, currentPage, userState).ConfigureAwait(false)).ToList();
            while (currentProjectReferences.Count > 0)
            {
                foreach (var projectReference in currentProjectReferences)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result.Add(await client.GetProject(projectReference.Id, includeCapabilities, userState).ConfigureAwait(false));
                }

                // check whether the recently returned item(s) were less than the max page size
                if (currentProjectReferences.Count < pageSize)
                    break; // if so, break the loop as we've read all instances

                // otherwise continue
                cancellationToken.ThrowIfCancellationRequested();
                currentProjectReferences = (await client.GetProjects(stateFilter, pageSize, currentPage, userState).ConfigureAwait(false)).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
    }
}