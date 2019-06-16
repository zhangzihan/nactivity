using Sys.Workflow.Cloud.Services.Api.Events;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Events.Configurations;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Tasks;

namespace Sys.Workflow.Cloud.Services.Events.Converters
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