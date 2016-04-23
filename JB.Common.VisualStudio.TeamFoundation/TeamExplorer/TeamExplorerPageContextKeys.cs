using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Controls;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    /// When navigating to a <see cref="ITeamExplorerPage"/> a context can, sometimes must, be provided and they are typically
    /// dictionaroes with a specific key, and this class lists the known ones.
    /// </summary>
    public static class TeamExplorerPageContextKeys
    {

        public static readonly string Builds = "QueuedBuildId";

        public static readonly string CodeReviewWorkItem = CodeReviewContextKeyNames.WorkItem;
        public static readonly string CodeReviewWorkItemId = CodeReviewContextKeyNames.WorkItemId;

        public static readonly string CodeReviewWorkspace = CodeReviewContextKeyNames.Workspace;
        public static readonly string CodeReviewShelveset = CodeReviewContextKeyNames.Shelveset;
        public static readonly string CodeReviewShelvesetName = CodeReviewContextKeyNames.ShelvesetName;
        public static readonly string CodeReviewShelvesetExcludedCount = CodeReviewContextKeyNames.ShelvesetExcludedCount;
        public static readonly string CodeReviewChangesetId = CodeReviewContextKeyNames.ChangesetId;
    }
}