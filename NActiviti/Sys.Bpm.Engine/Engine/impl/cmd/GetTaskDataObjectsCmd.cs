using System;
using System.Collections.Generic;

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

    [Serializable]
    public class GetTaskDataObjectsCmd : ICommand<IDictionary<string, IDataObject>>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal ICollection<string> variableNames;
        protected internal string locale;
        protected internal bool withLocalizationFallback;

        public GetTaskDataObjectsCmd(string taskId, ICollection<string> variableNames)
        {
            this.taskId = taskId;
            this.variableNames = variableNames;
        }

        public GetTaskDataObjectsCmd(string taskId, ICollection<string> variableNames, string locale, bool withLocalizationFallback)
        {
            this.taskId = taskId;
            this.variableNames = variableNames;
            this.locale = locale;
            this.withLocalizationFallback = withLocalizationFallback;
        }

        public virtual IDictionary<string, IDataObject> execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(taskId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            ITaskEntity task = commandContext.TaskEntityManager.findById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("task " + taskId + " doesn't exist", typeof(Task));
            }

            IDictionary<string, IDataObject> dataObjects = null;
            IDictionary<string, IVariableInstance> variables = null;
            if (variableNames == null)
            {
                variables = task.VariableInstances;
            }
            else
            {
                variables = task.getVariableInstances(variableNames, false);
            }

            if (variables != null)
            {
                dataObjects = new Dictionary<string, IDataObject>(variables.Count);

                foreach (KeyValuePair<string, IVariableInstance> entry in variables.SetOfKeyValuePairs())
                {
                    IVariableInstance variableEntity = entry.Value;

                    string localizedName = null;
                    string localizedDescription = null;

                    IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(variableEntity.ExecutionId);
                    while (!executionEntity.IsScope)
                    {
                        executionEntity = executionEntity.Parent;
                    }

                    BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(executionEntity.ProcessDefinitionId);
                    ValuedDataObject foundDataObject = null;
                    if (ReferenceEquals(executionEntity.ParentId, null))
                    {
                        foreach (ValuedDataObject dataObject in bpmnModel.MainProcess.DataObjects)
                        {
                            if (dataObject.Name.Equals(variableEntity.Name))
                            {
                                foundDataObject = dataObject;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SubProcess subProcess = (SubProcess)bpmnModel.getFlowElement(executionEntity.ActivityId);
                        foreach (ValuedDataObject dataObject in subProcess.DataObjects)
                        {
                            if (dataObject.Name.Equals(variableEntity.Name))
                            {
                                foundDataObject = dataObject;
                                break;
                            }
                        }
                    }

                    if (!ReferenceEquals(locale, null) && foundDataObject != null)
                    {
                        JToken languageNode = Context.getLocalizationElementProperties(locale, foundDataObject.Id, task.ProcessDefinitionId, withLocalizationFallback);

                        if (languageNode != null)
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
                        dataObjects[variableEntity.Name] = new DataObjectImpl(variableEntity.Name, variableEntity.Value, foundDataObject.Documentation, foundDataObject.Type, localizedName, localizedDescription, foundDataObject.Id);
                    }
                }
            }

            return dataObjects;
        }
    }

}