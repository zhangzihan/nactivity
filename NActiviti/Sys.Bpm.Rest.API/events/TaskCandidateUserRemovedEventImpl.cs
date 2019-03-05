namespace org.activiti.cloud.services.events
{


    using TaskCandidateUser = org.activiti.cloud.services.api.model.TaskCandidateUser;

    public class TaskCandidateUserRemovedEventImpl : AbstractProcessEngineEvent, ITaskCandidateUserRemovedEvent
    {

        private TaskCandidateUser taskCandidateUser;

        public TaskCandidateUserRemovedEventImpl()
        {
        }

        public TaskCandidateUserRemovedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateUser taskCandidateUser) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.taskCandidateUser = taskCandidateUser;
        }

        public virtual TaskCandidateUser TaskCandidateUser
        {
            get
            {
                return taskCandidateUser;
            }
        }

        public override string EventType
        {
            get
            {
                return "TaskCandidateUserRemovedEvent";
            }
        }
    }

}