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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Models;

    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Runtime;
    using System.Linq;

    [Serializable]
    public class GetDataObjectsCmd : ICommand<IDictionary<string, IDataObject>>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal IEnumerable<string> dataObjectNames;
        protected internal bool isLocal;
        protected internal string locale;
        protected internal bool withLocalizationFallback;

        public GetDataObjectsCmd(string executionId, IEnumerable<string> dataObjectNames, bool isLocal)
        {
            this.executionId = executionId;
            this.dataObjectNames = dataObjectNames;
            this.isLocal = isLocal;
        }

        public GetDataObjectsCmd(string executionId, IEnumerable<string> dataObjectNames, bool isLocal, string locale, bool withLocalizationFallback)
        {
            this.executionId = executionId;
            this.dataObjectNames = dataObjectNames;
            this.isLocal = isLocal;
            this.locale = locale;
            this.withLocalizationFallback = withLocalizationFallback;
        }

        public virtual IDictionary<string, IDataObject> Execute(ICommandContext commandContext)
        {
            // Verify existance of execution
            if (executionId is null)
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            IDictionary<string, IVariableInstance> variables;
            if ((dataObjectNames?.Count()).GetValueOrDefault(0) == 0)
            {
                // Fetch all
                if (isLocal)
                {
                    variables = execution.VariableInstancesLocal;
                }
                else
                {
                    variables = execution.VariableInstances;
                }

            }
            else
            {
                // Fetch specific collection of variables
                if (isLocal)
                {
                    variables = execution.GetVariableInstancesLocal(dataObjectNames, false);
                }
                else
                {
                    variables = execution.GetVariableInstances(dataObjectNames, false);
                }
            }

            IDictionary<string, IDataObject> dataObjects = null;
            if (variables != null)
            {
                dataObjects = new Dictionary<string, IDataObject>(variables.Count);

                foreach (KeyValuePair<string, IVariableInstance> entry in variables.SetOfKeyValuePairs())
                {
                    string name = entry.Key;
                    IVariableInstance variableEntity = entry.Value;

                    IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(variableEntity.ExecutionId);
                    while (!executionEntity.IsScope)
                    {
                        executionEntity = executionEntity.Parent;
                    }

                    BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(execution.ProcessDefinitionId);
                    ValuedDataObject foundDataObject = null;
                    if (executionEntity.ParentId is null)
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
                        SubProcess subProcess = (SubProcess)bpmnModel.GetFlowElement(execution.ActivityId);
                        foreach (ValuedDataObject dataObject in subProcess.DataObjects)
                        {
                            if (dataObject.Name.Equals(variableEntity.Name))
                            {
                                foundDataObject = dataObject;
                                break;
                            }
                        }
                    }

                    string localizedName = null;
                    string localizedDescription = null;

                    if (!(locale is null) && foundDataObject != null)
                    {
                        JToken languageNode = Context.GetLocalizationElementProperties(locale, foundDataObject.Id, execution.ProcessDefinitionId, withLocalizationFallback);

                        if (languageNode != null)
                        {
                            JToken nameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                            if (nameNode != null)
                            {
                                localizedName = nameNode.ToString();
                            }
                            JToken descriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                            if (descriptionNode != null)
                            {
                                localizedDescription = descriptionNode.ToString();
                            }
                        }
                    }

                    if (foundDataObject != null)
                    {
                        dataObjects[name] = new DataObjectImpl(variableEntity.Name, variableEntity.Value, foundDataObject.Documentation, foundDataObject.Type, localizedName, localizedDescription, foundDataObject.Id);
                    }
                }
            }

            return dataObjects;
        }
    }

}