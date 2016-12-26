using System;
using Microsoft.VisualStudio.Services.Common;

namespace JB.TeamFoundationServer.Reactive.Client
{
    public class WorkItemQueryClient : TeamProjectCollectionClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemQueryClient"/> class.
        /// </summary>
        /// <param name="teamProjectCollectionUri">The team project collection URI.</param>
        /// <param name="vssCredentials">The VSS credentials.</param>
        public WorkItemQueryClient(Uri teamProjectCollectionUri, VssCredentials vssCredentials) : base(teamProjectCollectionUri, vssCredentials)
        {
        }
    }
}