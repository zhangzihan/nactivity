namespace org.activiti.cloud.services.events
{
    using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
    using TaskCandidateUser = org.activiti.cloud.services.api.model.TaskCandidateUser;

    public interface ITaskCandidateUserAddedEvent : IProcessEngineEvent
    {

        TaskCandidateUser TaskCandidateUser { get; }
    }

}