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

namespace Sys.Workflow.cloud.services.events
{

    /// <summary>
    /// 
    /// </summary>
    public class VariableDeletedEventImpl : AbstractProcessEngineEvent, IVariableDeletedEvent
    {
        private readonly string variableName;
        private readonly string variableType;
        private readonly string taskId;


        /// <summary>
        /// 
        /// </summary>
        public VariableDeletedEventImpl()
        {
        }

        /// <summary>
        /// 
        /// </summary>

        public VariableDeletedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, string variableName, string variableType, string taskId) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.variableName = variableName;
            this.variableType = variableType;
            this.taskId = taskId;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string VariableName
        {
            get
            {
                return variableName;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string VariableType
        {
            get
            {
                return variableType;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override string EventType
        {
            get
            {
                return "VariableDeletedEvent";
            }
        }
    }

}