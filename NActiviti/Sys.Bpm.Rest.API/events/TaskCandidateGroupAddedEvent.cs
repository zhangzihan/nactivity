namespace org.activiti.cloud.services.events
{
	using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
	using TaskCandidateGroup = org.activiti.cloud.services.api.model.TaskCandidateGroup;

	public interface ITaskCandidateGroupAddedEvent : IProcessEngineEvent
	{

		TaskCandidateGroup TaskCandidateGroup {get;}
	}
}