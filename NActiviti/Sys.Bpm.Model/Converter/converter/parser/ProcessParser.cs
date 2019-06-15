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
namespace org.activiti.bpmn.converter.parser
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.export;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class ProcessParser : IBpmnXMLConstants
    {
        public virtual Process Parse(XMLStreamReader xtr, BpmnModel model)
        {
            Process process = null;
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID)))
            {
                string processId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                process = new Process
                {
                    Id = processId
                };
                BpmnXMLUtil.AddXMLLocation(process, xtr);
                process.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE)))
                {
                    process.Executable = bool.Parse(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE));
                }
                string candidateUsersString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_USERS);
                if (!string.IsNullOrWhiteSpace(candidateUsersString))
                {
                    IList<string> candidateUsers = BpmnXMLUtil.ParseDelimitedList(candidateUsersString);
                    process.CandidateStarterUsers = candidateUsers;
                }
                string candidateGroupsString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_GROUPS);
                if (!string.IsNullOrWhiteSpace(candidateGroupsString))
                {
                    IList<string> candidateGroups = BpmnXMLUtil.ParseDelimitedList(candidateGroupsString);
                    process.CandidateStarterGroups = candidateGroups;
                }

                BpmnXMLUtil.AddCustomAttributes(xtr, process, ProcessExport.defaultProcessAttributes);

                model.Processes.Add(process);

            }
            return process;
        }
    }

}