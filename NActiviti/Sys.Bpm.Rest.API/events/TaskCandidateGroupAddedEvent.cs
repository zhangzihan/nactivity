namespace org.activiti.cloud.services.events
{
	using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
	using TaskCandidateGroup = org.activiti.cloud.services.api.model.TaskCandidateGroup;

    /// <summary>
    /// 
    /// </summary>
    public interface ITaskCandidateGroupAddedEvent : IProcessEngineEvent
	{

        /// <summary>
        /// 
        /// </summary>
        TaskCandidateGroup TaskCandidateGroup {get;}
	}
}