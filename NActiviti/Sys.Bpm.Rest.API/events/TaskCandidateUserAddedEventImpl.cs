namespace org.activiti.cloud.services.events
{


    using TaskCandidateUser = api.model.TaskCandidateUser;


    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateUserAddedEventImpl : AbstractProcessEngineEvent, ITaskCandidateUserAddedEvent
    {

        private TaskCandidateUser taskCandidateUser;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUserAddedEventImpl()
        {
        }

        /// <summary>
        /// 
        /// </summary>

        public TaskCandidateUserAddedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateUser taskCandidateUser) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
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
                return "TaskCandidateUserAddedEvent";
            }
        }

    }

}