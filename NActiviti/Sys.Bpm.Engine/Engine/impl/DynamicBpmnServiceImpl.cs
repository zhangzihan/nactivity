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

namespace org.activiti.engine.impl
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.cmd;



    /// 
    public class DynamicBpmnServiceImpl : ServiceImpl, IDynamicBpmnService, IDynamicBpmnConstants
    {

        public virtual JToken getProcessDefinitionInfo(string processDefinitionId)
        {
            return commandExecutor.execute(new GetProcessDefinitionInfoCmd(processDefinitionId));
        }

        public virtual void saveProcessDefinitionInfo(string processDefinitionId, JToken infoNode)
        {
            commandExecutor.execute(new SaveProcessDefinitionInfoCmd(processDefinitionId, infoNode));
        }

        public virtual JToken changeServiceTaskClassName(string id, string className)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeServiceTaskClassName(id, className, infoNode);
            return infoNode;
        }

        public virtual void changeServiceTaskClassName(string id, string className, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.SERVICE_TASK_CLASS_NAME, className, infoNode);
        }

        public virtual JToken changeServiceTaskExpression(string id, string expression)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeServiceTaskExpression(id, expression, infoNode);
            return infoNode;
        }

        public virtual void changeServiceTaskExpression(string id, string expression, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.SERVICE_TASK_EXPRESSION, expression, infoNode);
        }

        public virtual JToken changeServiceTaskDelegateExpression(string id, string expression)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeServiceTaskDelegateExpression(id, expression, infoNode);
            return infoNode;
        }

        public virtual void changeServiceTaskDelegateExpression(string id, string expression, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.SERVICE_TASK_DELEGATE_EXPRESSION, expression, infoNode);
        }

        public virtual JToken changeScriptTaskScript(string id, string script)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeScriptTaskScript(id, script, infoNode);
            return infoNode;
        }

        public virtual void changeScriptTaskScript(string id, string script, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.SCRIPT_TASK_SCRIPT, script, infoNode);
        }

        public virtual JToken changeUserTaskName(string id, string name)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskName(id, name, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskName(string id, string name, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_NAME, name, infoNode);
        }

        public virtual JToken changeUserTaskDescription(string id, string description)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskDescription(id, description, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskDescription(string id, string description, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_DESCRIPTION, description, infoNode);
        }

        public virtual JToken changeUserTaskDueDate(string id, string dueDate)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskDueDate(id, dueDate, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskDueDate(string id, string dueDate, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_DUEDATE, dueDate, infoNode);
        }

        public virtual JToken changeUserTaskPriority(string id, string priority)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskPriority(id, priority, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskPriority(string id, string priority, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_PRIORITY, priority, infoNode);
        }

        public virtual JToken changeUserTaskCategory(string id, string category)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskCategory(id, category, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskCategory(string id, string category, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_CATEGORY, category, infoNode);
        }

        public virtual JToken changeUserTaskFormKey(string id, string formKey)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskFormKey(id, formKey, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskFormKey(string id, string formKey, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_FORM_KEY, formKey, infoNode);
        }

        public virtual JToken changeUserTaskAssignee(string id, string assignee)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskAssignee(id, assignee, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskAssignee(string id, string assignee, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_ASSIGNEE, assignee, infoNode);
        }

        public virtual JToken changeUserTaskOwner(string id, string owner)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskOwner(id, owner, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskOwner(string id, string owner, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_OWNER, owner, infoNode);
        }

        public virtual JToken changeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskCandidateUser(id, candidateUser, overwriteOtherChangedEntries, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries, JToken infoNode)
        {
            JArray valuesNode = null;
            if (overwriteOtherChangedEntries)
            {
                valuesNode = processEngineConfiguration.ObjectMapper.createArrayNode();
            }
            else
            {
                if (doesElementPropertyExist(id, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_USERS, infoNode))
                {
                    valuesNode = (JArray)infoNode[DynamicBpmnConstants_Fields.BPMN_NODE][id][DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_USERS];
                }

                if (valuesNode == null)
                {
                    valuesNode = processEngineConfiguration.ObjectMapper.createArrayNode();
                }
            }

            valuesNode.Add(candidateUser);
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_USERS, valuesNode.ToString(), infoNode);
        }

        public virtual JToken changeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeUserTaskCandidateGroup(id, candidateGroup, overwriteOtherChangedEntries, infoNode);
            return infoNode;
        }

        public virtual void changeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries, JToken infoNode)
        {
            JArray valuesNode = null;
            if (overwriteOtherChangedEntries)
            {
                valuesNode = processEngineConfiguration.ObjectMapper.createArrayNode();
            }
            else
            {
                if (doesElementPropertyExist(id, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_GROUPS, infoNode))
                {
                    valuesNode = (JArray)infoNode[DynamicBpmnConstants_Fields.BPMN_NODE][id][DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_GROUPS];
                }

                if (valuesNode == null)
                {
                    valuesNode = processEngineConfiguration.ObjectMapper.createArrayNode();
                }
            }

            valuesNode.Add(candidateGroup);
            setElementProperty(id, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_GROUPS, valuesNode, infoNode);
        }

        public virtual JToken changeDmnTaskDecisionTableKey(string id, string decisionTableKey)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeDmnTaskDecisionTableKey(id, decisionTableKey, infoNode);
            return infoNode;
        }

        public virtual void changeDmnTaskDecisionTableKey(string id, string decisionTableKey, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.DMN_TASK_DECISION_TABLE_KEY, decisionTableKey, infoNode);
        }

        public virtual JToken changeSequenceFlowCondition(string id, string condition)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeSequenceFlowCondition(id, condition, infoNode);
            return infoNode;
        }

        public virtual void changeSequenceFlowCondition(string id, string condition, JToken infoNode)
        {
            setElementProperty(id, DynamicBpmnConstants_Fields.SEQUENCE_FLOW_CONDITION, condition, infoNode);
        }

        public virtual JToken getBpmnElementProperties(string id, JToken infoNode)
        {
            JToken propertiesNode = null;
            JToken bpmnNode = getBpmnNode(infoNode);
            if (bpmnNode != null)
            {
                propertiesNode = (JToken)bpmnNode[id];
            }
            return propertiesNode;
        }

        public virtual JToken changeLocalizationName(string language, string id, string value)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeLocalizationName(language, id, value, infoNode);
            return infoNode;
        }

        public virtual void changeLocalizationName(string language, string id, string value, JToken infoNode)
        {
            setLocalizationProperty(language, id, DynamicBpmnConstants_Fields.LOCALIZATION_NAME, value, infoNode);
        }

        public virtual JToken changeLocalizationDescription(string language, string id, string value)
        {
            JToken infoNode = processEngineConfiguration.ObjectMapper.createObjectNode();
            changeLocalizationDescription(language, id, value, infoNode);
            return infoNode;
        }

        public virtual void changeLocalizationDescription(string language, string id, string value, JToken infoNode)
        {
            setLocalizationProperty(language, id, DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION, value, infoNode);
        }

        public virtual JToken getLocalizationElementProperties(string language, string id, JToken infoNode)
        {
            JToken propertiesNode = null;
            JToken localizationNode = getLocalizationNode(infoNode);
            if (localizationNode != null)
            {
                JToken languageNode = localizationNode[language];
                if (languageNode != null)
                {
                    propertiesNode = (JToken)languageNode[id];
                }
            }
            return propertiesNode;
        }

        protected internal virtual bool doesElementPropertyExist(string id, string propertyName, JToken infoNode)
        {
            bool exists = false;
            if (infoNode[DynamicBpmnConstants_Fields.BPMN_NODE] != null && infoNode[DynamicBpmnConstants_Fields.BPMN_NODE][id] != null && infoNode[DynamicBpmnConstants_Fields.BPMN_NODE][id][propertyName] != null)
            {
                JToken propNode = infoNode.SelectToken($"{DynamicBpmnConstants_Fields.BPMN_NODE}.{id}.{propertyName}");
                if (propNode != null)
                {
                    exists = true;
                }
            }
            return exists;
        }

        protected internal virtual void setElementProperty(string id, string propertyName, string propertyValue, JToken infoNode)
        {
            JToken bpmnNode = createOrGetBpmnNode(infoNode);
            if (bpmnNode[id] == null)
            {
                bpmnNode.AddAfterSelf(id);
            }

            bpmnNode[id][propertyName] = propertyValue;
        }

        protected internal virtual void setElementProperty(string id, string propertyName, JToken propertyValue, JToken infoNode)
        {
            JToken bpmnNode = createOrGetBpmnNode(infoNode);
            if (bpmnNode[id] == null)
            {
                bpmnNode.AddAfterSelf(id);
            }

            bpmnNode[id][propertyName] = propertyValue;
        }

        protected internal virtual JToken createOrGetBpmnNode(JToken infoNode)
        {
            if (infoNode[DynamicBpmnConstants_Fields.BPMN_NODE] == null)
            {
                infoNode.AddAfterSelf(DynamicBpmnConstants_Fields.BPMN_NODE);
            }
            return infoNode[DynamicBpmnConstants_Fields.BPMN_NODE];
        }

        protected internal virtual JToken getBpmnNode(JToken infoNode)
        {
            return infoNode[DynamicBpmnConstants_Fields.BPMN_NODE];
        }

        protected internal virtual void setLocalizationProperty(string language, string id, string propertyName, string propertyValue, JToken infoNode)
        {
            JToken localizationNode = createOrGetLocalizationNode(infoNode);
            if (localizationNode[language] == null)
            {
                localizationNode.AddAfterSelf(language);
            }

            JToken languageNode = (JToken)localizationNode[language];
            if (languageNode[id] == null)
            {
                languageNode.AddAfterSelf(id);
            }

            languageNode[id][propertyName] = propertyValue;
        }

        protected internal virtual JToken createOrGetLocalizationNode(JToken infoNode)
        {
            if (infoNode[DynamicBpmnConstants_Fields.LOCALIZATION_NODE] == null)
            {
                infoNode.AddAfterSelf(DynamicBpmnConstants_Fields.LOCALIZATION_NODE);
            }
            return infoNode[DynamicBpmnConstants_Fields.LOCALIZATION_NODE];
        }

        protected internal virtual JToken getLocalizationNode(JToken infoNode)
        {
            return (JToken)infoNode[DynamicBpmnConstants_Fields.LOCALIZATION_NODE];
        }

    }
}