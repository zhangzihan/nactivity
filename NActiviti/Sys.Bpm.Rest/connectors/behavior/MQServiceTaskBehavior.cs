using org.activiti.engine.@delegate;
using org.activiti.engine.impl.@delegate;
using org.activiti.engine.impl.persistence.entity.integration;
using org.activiti.engine.runtime;
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

namespace org.activiti.services.connectors.behavior
{
    using RuntimeBundleProperties = org.activiti.cloud.services.events.configuration.RuntimeBundleProperties;

    using AbstractBpmnActivityBehavior = org.activiti.engine.impl.bpmn.behavior.AbstractBpmnActivityBehavior;
    using DefaultActivityBehaviorFactory = org.activiti.engine.impl.bpmn.parser.factory.DefaultActivityBehaviorFactory;
    using IntegrationRequestEvent = org.activiti.services.connectors.model.IntegrationRequestEvent;
    using org.activiti.engine.impl.persistence.entity;
    using org.springframework.context;

    public class MQServiceTaskBehavior : AbstractBpmnActivityBehavior, ITriggerableActivityBehavior
    {

        private readonly IIntegrationContextManager integrationContextManager;
        private readonly RuntimeBundleProperties runtimeBundleProperties;
        private readonly IApplicationEventPublisher eventPublisher;

        public MQServiceTaskBehavior(IIntegrationContextManager integrationContextManager, RuntimeBundleProperties runtimeBundleProperties, IApplicationEventPublisher eventPublisher)
        {
            this.integrationContextManager = integrationContextManager;
            this.runtimeBundleProperties = runtimeBundleProperties;
            this.eventPublisher = eventPublisher;
        }

        public override void execute(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = storeIntegrationContext(execution);

            publishSpringEvent(execution, integrationContext);
        }

        /// <summary>
        /// Publishes an custom event using the Spring <seealso cref="IApplicationEventPublisher"/>. This event will be caught by
        /// <seealso cref="IntegrationRequestSender#sendIntegrationRequest(IntegrationRequestEvent)"/> which is annotated with
        /// <seealso cref="TransactionalEventListener"/> on phase <seealso cref="TransactionPhase#AFTER_COMMIT"/>. </summary>
        /// <param name="execution"> the related execution </param>
        /// <param name="integrationContext"> the related integration context </param>
        private void publishSpringEvent(IExecutionEntity execution, IIntegrationContextEntity integrationContext)
        {
            IntegrationRequestEvent @event = new IntegrationRequestEvent(execution, integrationContext, runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion);

            eventPublisher.publishEvent(@event);
        }


        private IIntegrationContextEntity storeIntegrationContext(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = buildIntegrationContext(execution);
            integrationContextManager.insert(integrationContext);
            return integrationContext;
        }

        private IIntegrationContextEntity buildIntegrationContext(IExecutionEntity execution)
        {
            IIntegrationContextEntity integrationContext = integrationContextManager.create();
            integrationContext.ExecutionId = execution.Id;
            integrationContext.ProcessInstanceId = execution.ProcessInstanceId;
            integrationContext.ProcessDefinitionId = execution.ProcessDefinitionId;
            integrationContext.FlowNodeId = execution.CurrentActivityId;
            integrationContext.CreatedDate = DateTime.Now;
            return integrationContext;
        }

        public override void trigger(IExecutionEntity execution, string signalEvent, object signalData)
        {
            leave(execution);
        }
    }
}