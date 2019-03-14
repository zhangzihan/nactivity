namespace org.activiti.cloud.services.events
{


    using TaskCandidateGroup = org.activiti.cloud.services.api.model.TaskCandidateGroup;


    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateGroupRemovedEventImpl : AbstractProcessEngineEvent, ITaskCandidateGroupRemovedEvent
    {

        private TaskCandidateGroup taskCandidateGroup;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupRemovedEventImpl()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupRemovedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateGroup taskCandidateGroup) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
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
                return "TaskCandidateGroupRemovedEvent";
            }
        }
    }

}