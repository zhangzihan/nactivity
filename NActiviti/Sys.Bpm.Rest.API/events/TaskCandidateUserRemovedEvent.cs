namespace Sys.Workflow.cloud.services.events
{
    using IProcessEngineEvent = api.events.IProcessEngineEvent;
    using TaskCandidateUser = api.model.TaskCandidateUser;

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