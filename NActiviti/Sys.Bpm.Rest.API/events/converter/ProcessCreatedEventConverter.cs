/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using org.activiti.cloud.services.api.events;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.impl.persistence.entity;

namespace org.activiti.cloud.services.events.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class ProcessCreatedEventConverter : AbstractEventConverter
    {
        private readonly ProcessInstanceConverter processInstanceConverter;


        /// <summary>
        /// 
        /// </summary>
        public ProcessCreatedEventConverter(ProcessInstanceConverter processInstanceConverter, RuntimeBundleProperties runtimeBundleProperties) : base(runtimeBundleProperties)
        {
            this.processInstanceConverter = processInstanceConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public override IProcessEngineEvent from(IActivitiEvent @event)
        {
            return new ProcessCreatedEventImpl(RuntimeBundleProperties.AppName, 
                RuntimeBundleProperties.AppVersion, 
                RuntimeBundleProperties.ServiceName, 
                RuntimeBundleProperties.ServiceFullName, 
                RuntimeBundleProperties.ServiceType, 
                RuntimeBundleProperties.ServiceVersion, 
                @event.ExecutionId, 
                @event.ProcessDefinitionId, 
                @event.ProcessInstanceId, 
                processInstanceConverter.from(((IExecutionEntity)((IActivitiEntityEvent)@event).Entity).ProcessInstance));
        }


        /// <summary>
        /// 
        /// </summary>
        public override string handledType()
        {
            return "ProcessInstance:" + ActivitiEventType.ENTITY_CREATED.ToString();
        }
    }

}