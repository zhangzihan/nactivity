﻿using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
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
 */
namespace org.activiti.engine.impl.cmd
{
    using Newtonsoft.Json.Linq;
    using org.activiti.bpmn.model;

    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    [Serializable]
    public class GetDataObjectCmd : ICommand<IDataObject>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal string dataObjectName;
        protected internal bool isLocal;
        protected internal string locale;
        protected internal bool withLocalizationFallback;

        public GetDataObjectCmd(string executionId, string dataObjectName, bool isLocal)
        {
            this.executionId = executionId;
            this.dataObjectName = dataObjectName;
            this.isLocal = isLocal;
        }

        public GetDataObjectCmd(string executionId, string dataObjectName, bool isLocal, string locale, bool withLocalizationFallback)
        {
            this.executionId = executionId;
            this.dataObjectName = dataObjectName;
            this.isLocal = isLocal;
            this.locale = locale;
            this.withLocalizationFallback = withLocalizationFallback;
        }

        public virtual IDataObject execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }
            if (ReferenceEquals(dataObjectName, null))
            {
                throw new ActivitiIllegalArgumentException("dataObjectName is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", executionId));

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            IDataObject dataObject = null;

            IVariableInstance variableEntity = null;
            if (isLocal)
            {
                variableEntity = execution.getVariableInstanceLocal(dataObjectName, false);
            }
            else
            {
                variableEntity = execution.getVariableInstance(dataObjectName, false);
            }

            string localizedName = null;
            string localizedDescription = null;

            if (variableEntity != null)
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", variableEntity.ExecutionId));
                while (!executionEntity.IsScope)
                {
                    executionEntity = executionEntity.Parent;
                }

                BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(executionEntity.ProcessDefinitionId);
                ValuedDataObject foundDataObject = null;
                if (ReferenceEquals(executionEntity.ParentId, null))
                {
                    foreach (ValuedDataObject dataObjectDefinition in bpmnModel.MainProcess.DataObjects)
                    {
                        if (dataObjectDefinition.Name.Equals(variableEntity.Name))
                        {
                            foundDataObject = dataObjectDefinition;
                            break;
                        }
                    }

                }
                else
                {
                    SubProcess subProcess = (SubProcess)bpmnModel.getFlowElement(execution.ActivityId);
                    foreach (ValuedDataObject dataObjectDefinition in subProcess.DataObjects)
                    {
                        if (dataObjectDefinition.Name.Equals(variableEntity.Name))
                        {
                            foundDataObject = dataObjectDefinition;
                            break;
                        }
                    }
                }

                if (!ReferenceEquals(locale, null) && foundDataObject != null)
                {
                    JToken languageNode = Context.getLocalizationElementProperties(locale, foundDataObject.Id, execution.ProcessDefinitionId, withLocalizationFallback);

                    if (variableEntity != null && languageNode != null)
                    {
                        JToken nameNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_NAME];
                        if (nameNode != null)
                        {
                            localizedName = nameNode.ToString();
                        }
                        JToken descriptionNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION];
                        if (descriptionNode != null)
                        {
                            localizedDescription = descriptionNode.ToString();
                        }
                    }
                }

                if (foundDataObject != null)
                {
                    dataObject = new DataObjectImpl(variableEntity.Name, variableEntity.Value, foundDataObject.Documentation, foundDataObject.Type, localizedName, localizedDescription, foundDataObject.Id);
                }
            }

            return dataObject;
        }
    }

}