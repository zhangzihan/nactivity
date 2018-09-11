namespace org.activiti.cloud.services.events
{
	using ProcessEngineEvent = org.activiti.cloud.services.api.events.ProcessEngineEvent;
	using TaskCandidateGroup = org.activiti.cloud.services.api.model.TaskCandidateGroup;

	public interface TaskCandidateGroupAddedEvent : ProcessEngineEvent
	{

		TaskCandidateGroup TaskCandidateGroup {get;}
	}
}