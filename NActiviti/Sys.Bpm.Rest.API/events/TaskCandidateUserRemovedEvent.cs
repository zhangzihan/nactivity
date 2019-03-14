namespace org.activiti.cloud.services.events
{
    using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
    using TaskCandidateUser = org.activiti.cloud.services.api.model.TaskCandidateUser;

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