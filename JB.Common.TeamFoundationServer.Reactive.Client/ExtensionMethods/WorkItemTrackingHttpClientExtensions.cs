using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace JB.TeamFoundationServer.Reactive.Client.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="WorkItemTrackingHttpClient"/> instances.
    /// </summary>
    public static class WorkItemTrackingHttpClientExtensions
    {
        /// <summary>
        /// Gets the <see cref="WorkItem" /> for the specified <paramref name="id" />.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="id">The work item identifier.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="asOf">As of.</param>
        /// <param name="expand">The expand.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">client</exception>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public static IObservable<WorkItem> GetWorkItem(this WorkItemTrackingHttpClient client, int id, IEnumerable<string> fields = null,
            DateTime? asOf = null, WorkItemExpand? expand = null, object userState = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return id <= 0
                ? Observable.Empty<WorkItem>()
                : Observable.FromAsync(token => client.GetWorkItemAsync(id, fields, asOf, expand, userState, token));
        }

        /// <summary>
        /// Gets the work items for the provided <paramref name="ids"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="ids">The work item identifiers.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="asOf">As of.</param>
        /// <param name="expand">The expand.</param>
        /// <param name="errorPolicy">The error policy.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">client</exception>
        public static IObservable<WorkItem> GetWorkItems(this WorkItemTrackingHttpClient client, IEnumerable<int> ids, IEnumerable<string> fields = null,
            DateTime? asOf = null, WorkItemExpand? expand = null,
            WorkItemErrorPolicy? errorPolicy = null,
            object userState = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (ids == null)
            {
                return Observable.Empty<WorkItem>();
            }

            // else
            return Observable.FromAsync(
                    token => client.GetWorkItemsAsync(
                        ids,
                        fields,
                        asOf,
                        expand,
                        errorPolicy,
                        userState,
                        token))
                .SelectMany(workItems => workItems);
        }
    }
}