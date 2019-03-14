/*
 * Copyright 2018 Alfresco and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace org.activiti.cloud.services.events
{




    /// <summary>
    /// 
    /// </summary>
    public class ProcessStartedEventImpl : AbstractProcessEngineEvent, IProcessStartedEvent
    {

        private string nestedProcessDefinitionId;
        private string nestedProcessInstanceId;


        /// <summary>
        /// 
        /// </summary>
        public ProcessStartedEventImpl()
        {
        }

        /// <summary>
        /// 
        /// </summary>

        public ProcessStartedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, string nestedProcessDefinitionId, string nestedProcessInstanceId) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.nestedProcessDefinitionId = nestedProcessDefinitionId;
            this.nestedProcessInstanceId = nestedProcessInstanceId;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string NestedProcessInstanceId
        {
            get
            {
                return nestedProcessInstanceId;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string NestedProcessDefinitionId
        {
            get
            {
                return nestedProcessDefinitionId;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override string EventType
        {
            get
            {
                return "ProcessStartedEvent";
            }
        }
    }

}