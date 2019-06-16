namespace Sys.Workflow.cloud.services.events
{
	using IProcessEngineEvent = api.events.IProcessEngineEvent;
	using TaskCandidateGroup = api.model.TaskCandidateGroup;

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