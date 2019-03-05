namespace org.activiti.cloud.services.events
{


    using TaskCandidateGroup = org.activiti.cloud.services.api.model.TaskCandidateGroup;

    public class TaskCandidateGroupRemovedEventImpl : AbstractProcessEngineEvent, ITaskCandidateGroupRemovedEvent
    {

        private TaskCandidateGroup taskCandidateGroup;

        public TaskCandidateGroupRemovedEventImpl()
        {
        }

        public TaskCandidateGroupRemovedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, TaskCandidateGroup taskCandidateGroup) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.taskCandidateGroup = taskCandidateGroup;
        }

        public virtual TaskCandidateGroup TaskCandidateGroup
        {
            get
            {
                return taskCandidateGroup;
            }
        }

        public override string EventType
        {
            get
            {
                return "TaskCandidateGroupRemovedEvent";
            }
        }
    }

}