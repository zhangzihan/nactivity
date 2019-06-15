namespace org.activiti.cloud.services.events
{
    using IProcessEngineEvent = api.events.IProcessEngineEvent;
    using TaskCandidateUser = api.model.TaskCandidateUser;

    /// <summary>
    /// 
    /// </summary>
    public interface ITaskCandidateUserAddedEvent : IProcessEngineEvent
    {

        /// <summary>
        /// 
        /// </summary>
        TaskCandidateUser TaskCandidateUser { get; }
    }

}