using Sys.Workflow.cloud.services.events.configuration;
using Sys.Workflow.engine.@delegate;
using Sys.Workflow.engine.impl.bpmn.behavior;
using Sys.Workflow.engine.impl.bpmn.parser.factory;
using Sys.Workflow.engine.impl.@delegate;
using Sys.Workflow.engine.impl.persistence.entity.integration;
using Sys.Workflow.engine.runtime;
using Sys.Workflow.services.connectors.model;
using System;

/*
 * Copyright 2017 Alfresco, Inc. and/or its affiliates.
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

namespace Sys.Workflow.services.connectors.behavior
{
    using Sys.Workflow.engine.impl.persistence.entity;
    using org.springframework.context;

    /// <summary>
    /// 
    /// </summary>
    public class MQServiceTaskBehavior : AbstractBpmnActivityBehavior, ITriggerableActivityBehavior
    {

        private readonly IIntegrationContextManager integrationContextManager;
        private readonly RuntimeBundleProperties runtimeBundleProperties;
        private readonly IApplicationEventPublisher eventPublisher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integrationContextManager"></param>
        /// <param name="runtimeBundleProperties"></param>
        /// <param name="eventPublisher"></param>
        public MQServiceTaskBehavior(IIntegrationContextManager integrationContextManager, RuntimeBundleProperties runtimeBundleProperties, IApplicationEventPublisher eventPublisher)
        {
            this.integrationContextManager = integrationContextManager;
            this.runtimeBundleProperties = runtimeBundleProperties;
            this.eventPublisher = eventPublisher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Execute(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = StoreIntegrationContext(execution);

            PublishSpringEvent(execution, integrationContext);
        }

        /// <summary>
        /// Publishes an custom event using the Spring <seealso cref="IApplicationEventPublisher"/>. This event will be caught by
        /// <seealso ref="IntegrationRequestSender#sendIntegrationRequest(IntegrationRequestEvent)"/> which is annotated with
        /// <seealso ref="TransactionalEventListener"/> on phase <seealso ref="TransactionPhase#AFTER_COMMIT"/>. </summary>
        /// <param name="execution"> the related execution </param>
        /// <param name="integrationContext"> the related integration context </param>
        private void PublishSpringEvent(IExecutionEntity execution, IIntegrationContextEntity integrationContext)
        {
            IntegrationRequestEvent @event = new IntegrationRequestEvent(execution, integrationContext, runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion);

            eventPublisher.PublishEvent(@event);
        }


        private IIntegrationContextEntity StoreIntegrationContext(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = BuildIntegrationContext(execution);
            integrationContextManager.Insert(integrationContext);
            return integrationContext;
        }

        private IIntegrationContextEntity BuildIntegrationContext(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = integrationContextManager.Create();
            integrationContext.ExecutionId = execution.Id;
            integrationContext.ProcessInstanceId = execution.ProcessInstanceId;
            integrationContext.ProcessDefinitionId = execution.ProcessDefinitionId;
            integrationContext.FlowNodeId = execution.CurrentActivityId;
            integrationContext.CreatedDate = DateTime.Now;
            return integrationContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="signalEvent"></param>
        /// <param name="signalData"></param>
        public override void Trigger(IExecutionEntity execution, string signalEvent, object signalData, bool throwError = true)
        {
            Leave(execution);
        }
    }
}