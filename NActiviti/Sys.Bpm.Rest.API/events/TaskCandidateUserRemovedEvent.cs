namespace Sys.Workflow.Cloud.Services.Events
{
    using Sys.Workflow.Cloud.Services.Api.Events;
    using Sys.Workflow.Cloud.Services.Api.Model;

    /// <summary>
    /// 
    /// </summary>
    public interface ITaskCandidateUserRemovedEvent : IProcessEngineEvent
    {

        /// <summary>
        /// 
        /// </summary>
        TaskCandidateUser TaskCandidateUser { get; }
    }

}