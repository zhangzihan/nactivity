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

namespace org.activiti.cloud.services.events.integration
{


    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseIntegrationEventImpl : AbstractProcessEngineEvent, IntegrationEvent
    {

        /// <summary>
        /// 
        /// </summary>
        public override abstract string EventType { get; }

        private string integrationContextId;
        private string flowNodeId;


        /// <summary>
        /// 
        /// </summary>
        //used to deserialize from jsons
        public BaseIntegrationEventImpl()
        {
        }

        /// <summary>
        /// 
        /// </summary>

        public BaseIntegrationEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, string integrationContextId, string flowNodeId) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.integrationContextId = integrationContextId;
            this.flowNodeId = flowNodeId;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string IntegrationContextId
        {
            get
            {
                return integrationContextId;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string FlowNodeId
        {
            get
            {
                return flowNodeId;
            }
        }

    }

}