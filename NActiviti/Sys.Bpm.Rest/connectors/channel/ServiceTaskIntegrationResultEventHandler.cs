using Microsoft.Extensions.Logging;
using org.activiti.cloud.services.events.configuration;
using org.activiti.cloud.services.events.integration;
using org.activiti.engine;
using org.activiti.engine.impl.persistence.entity.integration;
using org.activiti.engine.integration;
using org.activiti.services.connectors.model;
using org.springframework.messaging;
using org.springframework.messaging.support;
using Sys;
using System.Collections.Generic;

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

namespace org.activiti.services.connectors.channel
{
    public class ServiceTaskIntegrationResultEventHandler
    {
        private static readonly ILogger LOGGER =  ProcessEngineServiceProvider.LoggerService<ServiceTaskIntegrationResultEventHandler>();


        private readonly IRuntimeService runtimeService;
        private readonly IIntegrationContextService integrationContextService;
        private readonly MessageChannel<IntegrationResultReceivedEvent[]> auditProducer;
        private readonly RuntimeBundleProperties runtimeBundleProperties;

        public ServiceTaskIntegrationResultEventHandler(IRuntimeService runtimeService, IIntegrationContextService integrationContextService, MessageChannel<IntegrationResultReceivedEvent[]> auditProducer, RuntimeBundleProperties runtimeBundleProperties)
        {
            this.runtimeService = runtimeService;
            this.integrationContextService = integrationContextService;
            this.auditProducer = auditProducer;
            this.runtimeBundleProperties = runtimeBundleProperties;
        }

        public virtual void receive(IntegrationResultEvent integrationResultEvent)
        {
            IList<IIntegrationContextEntity> integrationContexts = integrationContextService.findIntegrationContextByExecutionId(integrationResultEvent.ExecutionId);

            if (integrationContexts == null || integrationContexts.Count == 0)
            {
                LOGGER.LogDebug("No integration contexts found in this RB for execution Id `" + integrationResultEvent.ExecutionId + ", flow node id `" + integrationResultEvent.FlowNodeId + "`");
            }

            if (integrationContexts != null)
            {
                foreach (IIntegrationContextEntity integrationContext in integrationContexts)
                {
                    if (integrationContext != null)
                    {
                        integrationContextService.deleteIntegrationContext(integrationContext);
                    }
                    sendAuditMessage(integrationContext);
                }
            }

            if (runtimeService.createExecutionQuery().executionId(integrationResultEvent.ExecutionId).list().Count > 0)
            {
                runtimeService.trigger(integrationResultEvent.ExecutionId, integrationResultEvent.Variables);
            }
            else
            {
                string message = "No task is in this RB is waiting for integration result with execution id `" + integrationResultEvent.ExecutionId +
                        ", flow node id `" + integrationResultEvent.FlowNodeId +
                        "`. The integration result `" + integrationResultEvent.Id + "` will be ignored.";
                LOGGER.LogDebug(message);
            }
        }

        private void sendAuditMessage(IIntegrationContextEntity integrationContext)
        {
            if (runtimeBundleProperties.EventsProperties.IntegrationAuditEventsEnabled)
            {
                Message<IntegrationResultReceivedEvent[]> message =
                    MessageBuilder<IntegrationResultReceivedEvent[]>.withPayload(new IntegrationResultReceivedEvent[]
                    {
                        new IntegrationResultReceivedEventImpl(runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion, integrationContext.ExecutionId, integrationContext.ProcessDefinitionId, integrationContext.ProcessInstanceId, integrationContext.Id, integrationContext.FlowNodeId)
                    }).build();

                auditProducer.send(message);
            }
        }
    }

}