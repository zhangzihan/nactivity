namespace org.activiti.cloud.services.events
{


    using TaskCandidateUser = api.model.TaskCandidateUser;


    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateUserRemovedEventImpl : AbstractProcessEngineEvent, ITaskCandidateUserRemovedEvent
    {

        private TaskCandidateUser taskCandidateUser;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUserRemovedEventImpl()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUserRemovedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateUser taskCandidateUser) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.taskCandidateUser = taskCandidateUser;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual TaskCandidateUser TaskCandidateUser
        {
            get
            {
                return taskCandidateUser;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override string EventType
        {
            get
            {
                return "TaskCandidateUserRemovedEvent";
            }
        }
    }

}