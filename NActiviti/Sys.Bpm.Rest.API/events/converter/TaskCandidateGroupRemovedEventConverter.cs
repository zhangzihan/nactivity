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
    public class TaskCandidateGroupRemovedEventConverter : AbstractEventConverter
    {

        private readonly TaskCandidateGroupConverter taskCandidateGroupConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupRemovedEventConverter(TaskCandidateGroupConverter identityLinkConverter, RuntimeBundleProperties runtimeBundleProperties) : base(runtimeBundleProperties)
        {
            this.taskCandidateGroupConverter = identityLinkConverter;
        }


        /// <summary>
        /// 
        /// </summary>
        public override IProcessEngineEvent From(IActivitiEvent @event)
        {
            return new TaskCandidateGroupRemovedEventImpl(RuntimeBundleProperties.AppName, RuntimeBundleProperties.AppVersion, RuntimeBundleProperties.ServiceName, RuntimeBundleProperties.ServiceFullName, RuntimeBundleProperties.ServiceType, RuntimeBundleProperties.ServiceVersion, @event.ExecutionId, @event.ProcessDefinitionId, @event.ProcessInstanceId, taskCandidateGroupConverter.From((IIdentityLink)((IActivitiEntityEvent)@event).Entity));
        }

        /// <summary>
        /// 
        /// </summary>

        public override string HandledType()
        {
            return "TaskCandidateGroup:" + ActivitiEventType.ENTITY_DELETED.ToString();
        }
    }

}