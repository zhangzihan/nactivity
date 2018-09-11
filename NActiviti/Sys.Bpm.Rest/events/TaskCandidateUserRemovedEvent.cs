namespace org.activiti.cloud.services.events
{
    using ProcessEngineEvent = org.activiti.cloud.services.api.events.ProcessEngineEvent;
    using TaskCandidateUser = org.activiti.cloud.services.api.model.TaskCandidateUser;

    public interface TaskCandidateUserRemovedEvent : ProcessEngineEvent
    {

        TaskCandidateUser TaskCandidateUser { get; }
    }

}