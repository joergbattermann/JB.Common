// -----------------------------------------------------------------------
// <copyright file="ProjectCollectionHttpClientExtensions.cs" company="Joerg Battermann">
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

namespace JB.TeamFoundationServer.Client.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods for <see cref="ProjectCollectionHttpClient"/> instances.
    /// </summary>
    public static class ProjectCollectionHttpClientExtensions
    {
        /// <summary>
        /// Get the project collection with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="client">The <see cref="ProjectCollectionHttpClient"/> to use.</param>
        /// <param name="id">The collection <paramref name="id"/>to search for.</param>
        /// <param name="userState">The user state object.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static Task<TeamProjectCollection> GetProjectCollection(this ProjectCollectionHttpClient client, Guid id, object userState = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (Equals(Guid.Empty, id)) throw new ArgumentOutOfRangeException(nameof(id));

            return client.GetProjectCollection(id.ToString(), userState);
        }

        /// <summary>
        /// Gets all project collections.
        /// </summary>
        /// <param name="client">The <see cref="ProjectCollectionHttpClient"/> to use.</param>
        /// <param name="pageSize">Page size to use while retrieving Project Collections.</param>
        /// <param name="userState">The user state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static async Task<IList<TeamProjectCollection>> GetAllProjectCollections(this ProjectCollectionHttpClient client, int pageSize = 10, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            
            var result = new List<TeamProjectCollection>();

            int currentPage = 0;
            var currentProjectCollectionReferences = (await client.GetProjectCollections(pageSize, currentPage, userState).ConfigureAwait(false)).ToList();
            while (currentProjectCollectionReferences.Count > 0)
            {
                foreach (var projectCollectionReference in currentProjectCollectionReferences)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result.Add(await client.GetProjectCollection(projectCollectionReference.Id, userState).ConfigureAwait(false));
                }
                
                // check whether the recently returned item(s) were less than the max page size
                if (currentProjectCollectionReferences.Count < pageSize)
                    break; // if so, break the loop as we've read all instances

                // otherwise continue
                cancellationToken.ThrowIfCancellationRequested();
                currentProjectCollectionReferences = (await client.GetProjectCollections(pageSize, currentPage++, userState).ConfigureAwait(false)).ToList();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
    }
}