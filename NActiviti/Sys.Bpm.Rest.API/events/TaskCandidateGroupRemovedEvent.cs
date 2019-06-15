namespace org.activiti.cloud.services.events
{
	using IProcessEngineEvent = api.events.IProcessEngineEvent;
	using TaskCandidateGroup = api.model.TaskCandidateGroup;

    /// <summary>
    /// 
    /// </summary>
    public interface ITaskCandidateGroupRemovedEvent : IProcessEngineEvent
	{

        /// <summary>
        /// 
        /// </summary>
        TaskCandidateGroup TaskCandidateGroup {get;}
	}

}