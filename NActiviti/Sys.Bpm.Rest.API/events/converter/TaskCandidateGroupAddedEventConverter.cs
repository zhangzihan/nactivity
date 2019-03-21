﻿using org.activiti.cloud.services.api.events;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.task;

namespace org.activiti.cloud.services.events.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateGroupAddedEventConverter : AbstractEventConverter
    {
        private readonly TaskCandidateGroupConverter taskCandidateGroupConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupAddedEventConverter(TaskCandidateGroupConverter identityLinkConverter, RuntimeBundleProperties runtimeBundleProperties) : base(runtimeBundleProperties)
        {
            this.taskCandidateGroupConverter = identityLinkConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public override IProcessEngineEvent from(IActivitiEvent @event)
        {
            return new TaskCandidateGroupAddedEventImpl(RuntimeBundleProperties.AppName, RuntimeBundleProperties.AppVersion, 
                RuntimeBundleProperties.ServiceName, 
                RuntimeBundleProperties.ServiceFullName, 
                RuntimeBundleProperties.ServiceType, 
                RuntimeBundleProperties.ServiceVersion, 
                @event.ExecutionId, 
                @event.ProcessDefinitionId, 
                @event.ProcessInstanceId, 
                taskCandidateGroupConverter.from((IIdentityLink)((IActivitiEntityEvent)@event).Entity));
        }

        /// <summary>
        /// 
        /// </summary>

        public override string handledType()
        {
            return "TaskCandidateGroup:" + ActivitiEventType.ENTITY_CREATED.ToString();
        }
    }

}