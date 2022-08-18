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

namespace Sys.Workflow.Engine.Impl
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Cmd;



    /// 
    public class DynamicBpmnServiceImpl : ServiceImpl, IDynamicBpmnService, IDynamicBpmnConstants
    {

        public virtual JToken GetProcessDefinitionInfo(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetProcessDefinitionInfoCmd(processDefinitionId));
        }

        public virtual void SaveProcessDefinitionInfo(string processDefinitionId, JToken infoNode)
        {
            commandExecutor.Execute(new SaveProcessDefinitionInfoCmd(processDefinitionId, infoNode));
        }

        public virtual JToken ChangeServiceTaskClassName(string id, string className)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeServiceTaskClassName(id, className, infoNode);
            return infoNode;
        }

        public virtual void ChangeServiceTaskClassName(string id, string className, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.SERVICE_TASK_CLASS_NAME, className, infoNode);
        }

        public virtual JToken ChangeServiceTaskExpression(string id, string expression)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeServiceTaskExpression(id, expression, infoNode);
            return infoNode;
        }

        public virtual void ChangeServiceTaskExpression(string id, string expression, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.SERVICE_TASK_EXPRESSION, expression, infoNode);
        }

        public virtual JToken ChangeServiceTaskDelegateExpression(string id, string expression)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeServiceTaskDelegateExpression(id, expression, infoNode);
            return infoNode;
        }

        public virtual void ChangeServiceTaskDelegateExpression(string id, string expression, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.SERVICE_TASK_DELEGATE_EXPRESSION, expression, infoNode);
        }

        public virtual JToken ChangeScriptTaskScript(string id, string script)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeScriptTaskScript(id, script, infoNode);
            return infoNode;
        }

        public virtual void ChangeScriptTaskScript(string id, string script, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.SCRIPT_TASK_SCRIPT, script, infoNode);
        }

        public virtual JToken ChangeUserTaskName(string id, string name)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskName(id, name, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskName(string id, string name, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_NAME, name, infoNode);
        }

        public virtual JToken ChangeUserTaskDescription(string id, string description)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskDescription(id, description, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskDescription(string id, string description, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_DESCRIPTION, description, infoNode);
        }

        public virtual JToken ChangeUserTaskDueDate(string id, string dueDate)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskDueDate(id, dueDate, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskDueDate(string id, string dueDate, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_DUEDATE, dueDate, infoNode);
        }

        public virtual JToken ChangeUserTaskPriority(string id, string priority)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskPriority(id, priority, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskPriority(string id, string priority, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_PRIORITY, priority, infoNode);
        }

        public virtual JToken ChangeUserTaskCategory(string id, string category)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskCategory(id, category, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskCategory(string id, string category, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_CATEGORY, category, infoNode);
        }

        public virtual JToken ChangeUserTaskFormKey(string id, string formKey)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskFormKey(id, formKey, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskFormKey(string id, string formKey, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_FORM_KEY, formKey, infoNode);
        }

        public virtual JToken ChangeUserTaskAssignee(string id, string assignee)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskAssignee(id, assignee, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskAssignee(string id, string assignee, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_ASSIGNEE, assignee, infoNode);
        }

        public virtual JToken ChangeUserTaskOwner(string id, string owner)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskOwner(id, owner, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskOwner(string id, string owner, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_OWNER, owner, infoNode);
        }

        public virtual JToken ChangeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskCandidateUser(id, candidateUser, overwriteOtherChangedEntries, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries, JToken infoNode)
        {
            JArray valuesNode = null;
            if (overwriteOtherChangedEntries)
            {
                valuesNode = processEngineConfiguration.ObjectMapper.CreateArrayNode();
            }
            else
            {
                if (DoesElementPropertyExist(id, DynamicBpmnConstants.USER_TASK_CANDIDATE_USERS, infoNode))
                {
                    valuesNode = (JArray)infoNode[DynamicBpmnConstants.BPMN_NODE][id][DynamicBpmnConstants.USER_TASK_CANDIDATE_USERS];
                }

                if (valuesNode is null)
                {
                    valuesNode = processEngineConfiguration.ObjectMapper.CreateArrayNode();
                }
            }

            valuesNode.Add(candidateUser);
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_CANDIDATE_USERS, valuesNode.ToString(), infoNode);
        }

        public virtual JToken ChangeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeUserTaskCandidateGroup(id, candidateGroup, overwriteOtherChangedEntries, infoNode);
            return infoNode;
        }

        public virtual void ChangeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries, JToken infoNode)
        {
            JArray valuesNode = null;
            if (overwriteOtherChangedEntries)
            {
                valuesNode = processEngineConfiguration.ObjectMapper.CreateArrayNode();
            }
            else
            {
                if (DoesElementPropertyExist(id, DynamicBpmnConstants.USER_TASK_CANDIDATE_GROUPS, infoNode))
                {
                    valuesNode = (JArray)infoNode[DynamicBpmnConstants.BPMN_NODE][id][DynamicBpmnConstants.USER_TASK_CANDIDATE_GROUPS];
                }

                if (valuesNode is null)
                {
                    valuesNode = processEngineConfiguration.ObjectMapper.CreateArrayNode();
                }
            }

            valuesNode.Add(candidateGroup);
            SetElementProperty(id, DynamicBpmnConstants.USER_TASK_CANDIDATE_GROUPS, valuesNode, infoNode);
        }

        public virtual JToken ChangeDmnTaskDecisionTableKey(string id, string decisionTableKey)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeDmnTaskDecisionTableKey(id, decisionTableKey, infoNode);
            return infoNode;
        }

        public virtual void ChangeDmnTaskDecisionTableKey(string id, string decisionTableKey, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.DMN_TASK_DECISION_TABLE_KEY, decisionTableKey, infoNode);
        }

        public virtual JToken ChangeSequenceFlowCondition(string id, string condition)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeSequenceFlowCondition(id, condition, infoNode);
            return infoNode;
        }

        public virtual void ChangeSequenceFlowCondition(string id, string condition, JToken infoNode)
        {
            SetElementProperty(id, DynamicBpmnConstants.SEQUENCE_FLOW_CONDITION, condition, infoNode);
        }

        public virtual JToken GetBpmnElementProperties(string id, JToken infoNode)
        {
            JToken propertiesNode = null;
            JToken bpmnNode = GetBpmnNode(infoNode);
            if (bpmnNode is not null)
            {
                propertiesNode = (JToken)bpmnNode[id];
            }
            return propertiesNode;
        }

        public virtual JToken ChangeLocalizationName(string language, string id, string value)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeLocalizationName(language, id, value, infoNode);
            return infoNode;
        }

        public virtual void ChangeLocalizationName(string language, string id, string value, JToken infoNode)
        {
            SetLocalizationProperty(language, id, DynamicBpmnConstants.LOCALIZATION_NAME, value, infoNode);
        }

        public virtual JToken ChangeLocalizationDescription(string language, string id, string value)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.CreateObjectNode();
            ChangeLocalizationDescription(language, id, value, infoNode);
            return infoNode;
        }

        public virtual void ChangeLocalizationDescription(string language, string id, string value, JToken infoNode)
        {
            SetLocalizationProperty(language, id, DynamicBpmnConstants.LOCALIZATION_DESCRIPTION, value, infoNode);
        }

        public virtual JToken GetLocalizationElementProperties(string language, string id, JToken infoNode)
        {
            JToken propertiesNode = null;
            JToken localizationNode = GetLocalizationNode(infoNode);
            if (localizationNode is not null)
            {
                JToken languageNode = localizationNode[language];
                if (languageNode is not null)
                {
                    propertiesNode = (JToken)languageNode[id];
                }
            }
            return propertiesNode;
        }

        protected internal virtual bool DoesElementPropertyExist(string id, string propertyName, JToken infoNode)
        {
            bool exists = false;
            if (infoNode[DynamicBpmnConstants.BPMN_NODE] is not null && infoNode[DynamicBpmnConstants.BPMN_NODE][id] is not null && infoNode[DynamicBpmnConstants.BPMN_NODE][id][propertyName] is not null)
            {
                JToken propNode = infoNode.SelectToken($"{DynamicBpmnConstants.BPMN_NODE}.{id}.{propertyName}");
                if (propNode is not null)
                {
                    exists = true;
                }
            }
            return exists;
        }

        protected internal virtual void SetElementProperty(string id, string propertyName, string propertyValue, JToken infoNode)
        {
            JToken bpmnNode = CreateOrGetBpmnNode(infoNode);
            if (bpmnNode[id] is null)
            {
                bpmnNode.AddAfterSelf(id);
            }

            bpmnNode[id][propertyName] = propertyValue;
        }

        protected internal virtual void SetElementProperty(string id, string propertyName, JToken propertyValue, JToken infoNode)
        {
            JToken bpmnNode = CreateOrGetBpmnNode(infoNode);
            if (bpmnNode[id] is null)
            {
                bpmnNode.AddAfterSelf(id);
            }

            bpmnNode[id][propertyName] = propertyValue;
        }

        protected internal virtual JToken CreateOrGetBpmnNode(JToken infoNode)
        {
            if (infoNode[DynamicBpmnConstants.BPMN_NODE] is null)
            {
                infoNode.AddAfterSelf(DynamicBpmnConstants.BPMN_NODE);
            }
            return infoNode[DynamicBpmnConstants.BPMN_NODE];
        }

        protected internal virtual JToken GetBpmnNode(JToken infoNode)
        {
            return infoNode[DynamicBpmnConstants.BPMN_NODE];
        }

        protected internal virtual void SetLocalizationProperty(string language, string id, string propertyName, string propertyValue, JToken infoNode)
        {
            JToken localizationNode = CreateOrGetLocalizationNode(infoNode);
            if (localizationNode[language] is null)
            {
                localizationNode.AddAfterSelf(language);
            }

            JToken languageNode = (JToken)localizationNode[language];
            if (languageNode[id] is null)
            {
                languageNode.AddAfterSelf(id);
            }

            languageNode[id][propertyName] = propertyValue;
        }

        protected internal virtual JToken CreateOrGetLocalizationNode(JToken infoNode)
        {
            if (infoNode[DynamicBpmnConstants.LOCALIZATION_NODE] is null)
            {
                infoNode.AddAfterSelf(DynamicBpmnConstants.LOCALIZATION_NODE);
            }
            return infoNode[DynamicBpmnConstants.LOCALIZATION_NODE];
        }

        protected internal virtual JToken GetLocalizationNode(JToken infoNode)
        {
            return (JToken)infoNode[DynamicBpmnConstants.LOCALIZATION_NODE];
        }
    }
}