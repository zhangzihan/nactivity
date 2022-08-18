using System;

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
    using System.Collections.Generic;

    [Serializable]
    public class GetTaskDataObjectCmd : ICommand<IDataObject>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal string variableName;
        protected internal string locale;
        protected internal bool withLocalizationFallback;

        public GetTaskDataObjectCmd(string taskId, string variableName)
        {
            this.taskId = taskId;
            this.variableName = variableName;
        }

        public GetTaskDataObjectCmd(string taskId, string variableName, string locale, bool withLocalizationFallback)
        {
            this.taskId = taskId;
            this.variableName = variableName;
            this.locale = locale;
            this.withLocalizationFallback = withLocalizationFallback;
        }

        public virtual IDataObject Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(taskId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }

            ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task is null)
            {
                throw new ActivitiObjectNotFoundException("task " + taskId + " doesn't exist", typeof(TaskActivity));
            }

            IDataObject dataObject = null;
            IVariableInstance variableEntity = task.GetVariableInstance(variableName, false);

            string localizedName = null;
            string localizedDescription = null;

            if (variableEntity is object)
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(variableEntity.ExecutionId);
                while (!executionEntity.IsScope)
                {
                    executionEntity = executionEntity.Parent;
                }

                BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(executionEntity.ProcessDefinitionId);
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
                    SubProcess subProcess = (SubProcess)bpmnModel.GetFlowElement(executionEntity.ActivityId);
                    foreach (ValuedDataObject dataObjectDefinition in subProcess.DataObjects)
                    {
                        if (dataObjectDefinition.Name.Equals(variableEntity.Name))
                        {
                            foundDataObject = dataObjectDefinition;
                            break;
                        }
                    }
                }

                if (!ReferenceEquals(locale, null) && foundDataObject is not null)
                {
                    JToken languageNode = Context.GetLocalizationElementProperties(locale, foundDataObject.Id, task.ProcessDefinitionId, withLocalizationFallback);

                    if (languageNode is not null)
                    {
                        JToken nameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                        if (nameNode is not null)
                        {
                            localizedName = nameNode.ToString();
                        }
                        JToken descriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                        if (descriptionNode is not null)
                        {
                            localizedDescription = descriptionNode.ToString();
                        }
                    }
                }

                if (foundDataObject is not null)
                {
                    dataObject = new DataObjectImpl(variableEntity.Name, variableEntity.Value, foundDataObject.Documentation, foundDataObject.Type, localizedName, localizedDescription, foundDataObject.Id);
                }
            }

            return dataObject;
        }
    }

}