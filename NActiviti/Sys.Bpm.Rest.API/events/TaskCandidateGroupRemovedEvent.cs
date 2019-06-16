namespace Sys.Workflow.Cloud.Services.Events
{
    using Sys.Workflow.Cloud.Services.Api.Events;
    using Sys.Workflow.Cloud.Services.Api.Model;

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