using org.activiti.cloud.services.api.events;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.task;

namespace org.activiti.cloud.services.events.converter
{
    public class TaskCandidateUserRemovedEventConverter : AbstractEventConverter
    {
        private readonly TaskCandidateUserConverter taskCandidateUserConverter;

        public TaskCandidateUserRemovedEventConverter(TaskCandidateUserConverter identityLinkConverter, RuntimeBundleProperties runtimeBundleProperties) : base(runtimeBundleProperties)
        {
            this.taskCandidateUserConverter = identityLinkConverter;
        }

        public override ProcessEngineEvent from(IActivitiEvent @event)
        {
            return new TaskCandidateUserRemovedEventImpl(RuntimeBundleProperties.AppName, RuntimeBundleProperties.AppVersion, RuntimeBundleProperties.ServiceName, RuntimeBundleProperties.ServiceFullName, RuntimeBundleProperties.ServiceType, RuntimeBundleProperties.ServiceVersion, @event.ExecutionId, @event.ProcessDefinitionId, @event.ProcessInstanceId, taskCandidateUserConverter.from((IIdentityLink)((IActivitiEntityEvent)@event).Entity));
        }

        public override string handledType()
        {
            return "TaskCandidateUser:" + ActivitiEventType.ENTITY_DELETED.ToString();
        }
    }

}