using System;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace JB.TeamFoundationServer.Reactive.Client
{
    /// <summary>
    /// Base class for VSTS / TFS clients on the Team Project Collection level
    /// </summary>
    public abstract class TeamProjectCollectionClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamProjectCollectionClient"/> class.
        /// </summary>
        /// <param name="teamProjectCollectionUri">The team project collection URI.</param>
        /// <param name="vssCredentials">The VSS credentials.</param>
        /// <exception cref="ArgumentNullException">
        /// teamProjectCollectionUri
        /// or
        /// vssCredentials
        /// </exception>
        protected TeamProjectCollectionClient(Uri teamProjectCollectionUri, VssCredentials vssCredentials)
        {
            if (teamProjectCollectionUri == null) throw new ArgumentNullException(nameof(teamProjectCollectionUri));
            if (vssCredentials == null) throw new ArgumentNullException(nameof(vssCredentials));

            VisualStudioServicesConnection = new VssConnection(teamProjectCollectionUri, vssCredentials);
        }

        /// <summary>
        /// Gets the visual studio services connection.
        /// </summary>
        /// <value>
        /// The visual studio services connection.
        /// </value>
        protected VssConnection VisualStudioServicesConnection { get; private set; }
    }
}