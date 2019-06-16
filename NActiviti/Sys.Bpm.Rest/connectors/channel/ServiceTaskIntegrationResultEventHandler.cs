using Microsoft.Extensions.Logging;
using Sys.Workflow.Cloud.Services.Events.Configurations;
using Sys.Workflow.Cloud.Services.Events.Integration;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Persistence.Entity.Integration;
using Sys.Workflow.Engine.Integration;
using Sys.Workflow.Services.Connectors.Models;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;
using Sys.Workflow;
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

namespace Sys.Workflow.Services.Connectors.Channels
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceTaskIntegrationResultEventHandler
    {
        private readonly ILogger logger = null;

        private readonly IRuntimeService runtimeService;
        private readonly IIntegrationContextService integrationContextService;
        private readonly IMessageChannel<IIntegrationResultReceivedEvent[]> auditProducer;
        private readonly RuntimeBundleProperties runtimeBundleProperties;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtimeService"></param>
        /// <param name="integrationContextService"></param>
        /// <param name="auditProducer"></param>
        /// <param name="runtimeBundleProperties"></param>
        /// <param name="loggerFactory"></param>
        public ServiceTaskIntegrationResultEventHandler(IRuntimeService runtimeService,
            IIntegrationContextService integrationContextService,
            IMessageChannel<IIntegrationResultReceivedEvent[]> auditProducer,
            RuntimeBundleProperties runtimeBundleProperties,
            ILoggerFactory loggerFactory)
        {
            this.runtimeService = runtimeService;
            this.integrationContextService = integrationContextService;
            this.auditProducer = auditProducer;
            this.runtimeBundleProperties = runtimeBundleProperties;
            logger = loggerFactory.CreateLogger<ServiceTaskIntegrationResultEventHandler>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integrationResultEvent"></param>
        public virtual void Receive(IntegrationResultEvent integrationResultEvent)
        {
            IList<IIntegrationContextEntity> integrationContexts = integrationContextService.findIntegrationContextByExecutionId(integrationResultEvent.ExecutionId);

            if (integrationContexts == null || integrationContexts.Count == 0)
            {
                logger.LogDebug("No integration contexts found in this RB for execution Id `" + integrationResultEvent.ExecutionId + ", flow node id `" + integrationResultEvent.FlowNodeId + "`");
            }

            if (integrationContexts != null)
            {
                foreach (IIntegrationContextEntity integrationContext in integrationContexts)
                {
                    if (integrationContext != null)
                    {
                        integrationContextService.deleteIntegrationContext(integrationContext);
                    }
                    SendAuditMessage(integrationContext);
                }
            }

            if (runtimeService.CreateExecutionQuery().SetExecutionId(integrationResultEvent.ExecutionId).List().Count > 0)
            {
                runtimeService.Trigger(integrationResultEvent.ExecutionId, integrationResultEvent.Variables);
            }
            else
            {
                string message = "No task is in this RB is waiting for integration result with execution id `" + integrationResultEvent.ExecutionId +
                        ", flow node id `" + integrationResultEvent.FlowNodeId +
                        "`. The integration result `" + integrationResultEvent.Id + "` will be ignored.";
                logger.LogDebug(message);
            }
        }

        private void SendAuditMessage(IIntegrationContextEntity integrationContext)
        {
            if (runtimeBundleProperties.EventsProperties.IntegrationAuditEventsEnabled)
            {
                IMessage<IIntegrationResultReceivedEvent[]> message =
                    MessageBuilder<IIntegrationResultReceivedEvent[]>.WithPayload(new IIntegrationResultReceivedEvent[]
                    {
                        new IntegrationResultReceivedEventImpl(runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion, integrationContext.ExecutionId, integrationContext.ProcessDefinitionId, integrationContext.ProcessInstanceId, integrationContext.Id, integrationContext.FlowNodeId)
                    }).Build();

                auditProducer.Send(message);
            }
        }
    }

}