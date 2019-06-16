namespace Sys.Workflow.cloud.services.events
{


    using TaskCandidateGroup = api.model.TaskCandidateGroup;


    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateGroupAddedEventImpl : AbstractProcessEngineEvent, ITaskCandidateGroupAddedEvent
    {

        private readonly TaskCandidateGroup taskCandidateGroup;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupAddedEventImpl()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupAddedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateGroup taskCandidateGroup) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.taskCandidateGroup = taskCandidateGroup;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskCandidateGroup TaskCandidateGroup
        {
            get
            {
                return taskCandidateGroup;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public override string EventType
        {
            get
            {
                return "TaskCandidateGroupAddedEvent";
            }
        }

    }

}