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

using Newtonsoft.Json.Linq;

namespace org.activiti.engine
{
    /// <summary>
    /// Service providing access to the repository of process definitions and deployments.
    /// 
    /// 
    /// </summary>
    public interface IDynamicBpmnService
    {

        JToken getProcessDefinitionInfo(string processDefinitionId);

        void saveProcessDefinitionInfo(string processDefinitionId, JToken infoNode);

        JToken changeServiceTaskClassName(string id, string className);

        void changeServiceTaskClassName(string id, string className, JToken infoNode);

        JToken changeServiceTaskExpression(string id, string expression);

        void changeServiceTaskExpression(string id, string expression, JToken infoNode);

        JToken changeServiceTaskDelegateExpression(string id, string expression);

        void changeServiceTaskDelegateExpression(string id, string expression, JToken infoNode);

        JToken changeScriptTaskScript(string id, string script);

        void changeScriptTaskScript(string id, string script, JToken infoNode);

        JToken changeUserTaskName(string id, string name);

        void changeUserTaskName(string id, string name, JToken infoNode);

        JToken changeUserTaskDescription(string id, string description);

        void changeUserTaskDescription(string id, string description, JToken infoNode);

        JToken changeUserTaskDueDate(string id, string dueDate);

        void changeUserTaskDueDate(string id, string dueDate, JToken infoNode);

        JToken changeUserTaskPriority(string id, string priority);

        void changeUserTaskPriority(string id, string priority, JToken infoNode);

        JToken changeUserTaskCategory(string id, string category);

        void changeUserTaskCategory(string id, string category, JToken infoNode);

        JToken changeUserTaskFormKey(string id, string formKey);

        void changeUserTaskFormKey(string id, string formKey, JToken infoNode);

        JToken changeUserTaskAssignee(string id, string assignee);

        void changeUserTaskAssignee(string id, string assignee, JToken infoNode);

        JToken changeUserTaskOwner(string id, string owner);

        void changeUserTaskOwner(string id, string owner, JToken infoNode);

        JToken changeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries);

        void changeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries, JToken infoNode);

        JToken changeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries);

        void changeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries, JToken infoNode);

        JToken changeDmnTaskDecisionTableKey(string id, string decisionTableKey);

        void changeDmnTaskDecisionTableKey(string id, string decisionTableKey, JToken infoNode);

        JToken changeSequenceFlowCondition(string id, string condition);

        void changeSequenceFlowCondition(string id, string condition, JToken infoNode);

        JToken getBpmnElementProperties(string id, JToken infoNode);

        JToken changeLocalizationName(string language, string id, string value);

        void changeLocalizationName(string language, string id, string value, JToken infoNode);

        JToken changeLocalizationDescription(string language, string id, string value);

        void changeLocalizationDescription(string language, string id, string value, JToken infoNode);

        JToken getLocalizationElementProperties(string language, string id, JToken infoNode);
    }
}