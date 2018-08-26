using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using JB.TeamFoundationServer.Reactive.Client.ExtensionMethods;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace JB.TeamFoundationServer.Reactive.Client
{
    public class WorkItemTrackingClient : TeamProjectCollectionClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemTrackingClient"/> class.
        /// </summary>
        /// <param name="teamProjectCollectionUri">The team project collection URI.</param>
        /// <param name="vssCredentials">The VSS credentials.</param>
        public WorkItemTrackingClient(Uri teamProjectCollectionUri, VssCredentials vssCredentials)
            :base(teamProjectCollectionUri, vssCredentials)
        {
        }

        /// <summary>
        /// Gets the <see cref="WorkItem"/> for the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="asOf">As of.</param>
        /// <param name="expand">The expand.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public virtual IObservable<WorkItem> GetWorkItem(int id, IEnumerable<string> fields = null, DateTime? asOf = null,
            WorkItemExpand? expand = null, object userState = null)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return Observable.Create<WorkItem>(observer =>
            {
                var workItemTrackingHttpClient = VisualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>();

                var retrievalObservable = workItemTrackingHttpClient
                    .GetWorkItem(id, fields, asOf, expand, userState)
                    .Subscribe(observer);

                return new CompositeDisposable(workItemTrackingHttpClient, retrievalObservable);
            });
        }

        /// <summary>
        /// Gets the work items for the provided <paramref name="ids" />.
        /// </summary>
        /// <param name="ids">The work item identifiers.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="asOf">As of.</param>
        /// <param name="expand">The expand.</param>
        /// <param name="errorPolicy">The error policy.</param>
        /// <param name="userState">State of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">client</exception>
        public virtual IObservable<WorkItem> GetWorkItemsAsync(IEnumerable<int> ids, IEnumerable<string> fields = null,
            DateTime? asOf = null, WorkItemExpand? expand = null,
            WorkItemErrorPolicy? errorPolicy = null,
            object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Observable.Create<WorkItem>(observer =>
            {
                var workItemTrackingHttpClient = VisualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>();

                var retrievalObservable = workItemTrackingHttpClient
                    .GetWorkItems(ids, fields, asOf, expand,
                        errorPolicy,
                        userState)
                    .Subscribe(observer);

                return new CompositeDisposable(workItemTrackingHttpClient, retrievalObservable);
            });
        }
    }
}