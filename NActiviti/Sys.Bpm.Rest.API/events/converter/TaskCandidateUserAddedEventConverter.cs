using Sys.Workflow.cloud.services.api.events;
using Sys.Workflow.cloud.services.api.model.converter;
using Sys.Workflow.cloud.services.events.configuration;
using Sys.Workflow.engine.@delegate.@event;
using Sys.Workflow.engine.task;

namespace Sys.Workflow.cloud.services.events.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateUserAddedEventConverter : AbstractEventConverter
    {
        private readonly TaskCandidateUserConverter taskCandidateUserConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUserAddedEventConverter(TaskCandidateUserConverter identityLinkConverter, RuntimeBundleProperties runtimeBundleProperties) : base(runtimeBundleProperties)
        {
            this.taskCandidateUserConverter = identityLinkConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public override IProcessEngineEvent From(IActivitiEvent @event)
        {
            return new TaskCandidateUserAddedEventImpl(RuntimeBundleProperties.AppName, RuntimeBundleProperties.AppVersion, RuntimeBundleProperties.ServiceName, RuntimeBundleProperties.ServiceFullName, RuntimeBundleProperties.ServiceType, RuntimeBundleProperties.ServiceVersion, @event.ExecutionId, @event.ProcessDefinitionId, @event.ProcessInstanceId, taskCandidateUserConverter.From((IIdentityLink)((IActivitiEntityEvent)@event).Entity));
        }

        /// <summary>
        /// 
        /// </summary>

        public override string HandledType()
        {
            return "TaskCandidateUser:" + ActivitiEventType.ENTITY_CREATED.ToString();
        }
    }

}