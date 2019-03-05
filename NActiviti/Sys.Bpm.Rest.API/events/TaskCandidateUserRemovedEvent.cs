namespace org.activiti.cloud.services.events
{
    using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
    using TaskCandidateUser = org.activiti.cloud.services.api.model.TaskCandidateUser;

    public interface ITaskCandidateUserRemovedEvent : IProcessEngineEvent
    {

        TaskCandidateUser TaskCandidateUser { get; }
    }

}