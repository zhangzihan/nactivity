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

namespace Sys.Workflow.engine
{
    /// <summary>
    /// Service providing access to the repository of process definitions and deployments.
    /// 
    /// 
    /// </summary>
    public interface IDynamicBpmnService
    {

        JToken GetProcessDefinitionInfo(string processDefinitionId);

        void SaveProcessDefinitionInfo(string processDefinitionId, JToken infoNode);

        JToken ChangeServiceTaskClassName(string id, string className);

        void ChangeServiceTaskClassName(string id, string className, JToken infoNode);

        JToken ChangeServiceTaskExpression(string id, string expression);

        void ChangeServiceTaskExpression(string id, string expression, JToken infoNode);

        JToken ChangeServiceTaskDelegateExpression(string id, string expression);

        void ChangeServiceTaskDelegateExpression(string id, string expression, JToken infoNode);

        JToken ChangeScriptTaskScript(string id, string script);

        void ChangeScriptTaskScript(string id, string script, JToken infoNode);

        JToken ChangeUserTaskName(string id, string name);

        void ChangeUserTaskName(string id, string name, JToken infoNode);

        JToken ChangeUserTaskDescription(string id, string description);

        void ChangeUserTaskDescription(string id, string description, JToken infoNode);

        JToken ChangeUserTaskDueDate(string id, string dueDate);

        void ChangeUserTaskDueDate(string id, string dueDate, JToken infoNode);

        JToken ChangeUserTaskPriority(string id, string priority);

        void ChangeUserTaskPriority(string id, string priority, JToken infoNode);

        JToken ChangeUserTaskCategory(string id, string category);

        void ChangeUserTaskCategory(string id, string category, JToken infoNode);

        JToken ChangeUserTaskFormKey(string id, string formKey);

        void ChangeUserTaskFormKey(string id, string formKey, JToken infoNode);

        JToken ChangeUserTaskAssignee(string id, string assignee);

        void ChangeUserTaskAssignee(string id, string assignee, JToken infoNode);

        JToken ChangeUserTaskOwner(string id, string owner);

        void ChangeUserTaskOwner(string id, string owner, JToken infoNode);

        JToken ChangeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries);

        void ChangeUserTaskCandidateUser(string id, string candidateUser, bool overwriteOtherChangedEntries, JToken infoNode);

        JToken ChangeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries);

        void ChangeUserTaskCandidateGroup(string id, string candidateGroup, bool overwriteOtherChangedEntries, JToken infoNode);

        JToken ChangeDmnTaskDecisionTableKey(string id, string decisionTableKey);

        void ChangeDmnTaskDecisionTableKey(string id, string decisionTableKey, JToken infoNode);

        JToken ChangeSequenceFlowCondition(string id, string condition);

        void ChangeSequenceFlowCondition(string id, string condition, JToken infoNode);

        JToken GetBpmnElementProperties(string id, JToken infoNode);

        JToken ChangeLocalizationName(string language, string id, string value);

        void ChangeLocalizationName(string language, string id, string value, JToken infoNode);

        JToken ChangeLocalizationDescription(string language, string id, string value);

        void ChangeLocalizationDescription(string language, string id, string value, JToken infoNode);

        JToken GetLocalizationElementProperties(string language, string id, JToken infoNode);
    }
}