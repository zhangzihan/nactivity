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
namespace Sys.Workflow.Bpmn.Converters.Exports
{

    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    public class ProcessExport : IBpmnXMLConstants
    {
        /// <summary>
        /// default attributes taken from process instance attributes
        /// </summary>
        public static readonly IList<ExtensionAttribute> defaultProcessAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_ID),
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_NAME),
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_USERS),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_GROUPS)
        };
        public static void WriteProcess(Process process, XMLStreamWriter xtw)
        {
            // start process element
            xtw.WriteStartElement(BpmnXMLConstants.BPMN2_PREFIX, BpmnXMLConstants.ELEMENT_PROCESS, BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, process.Id);

            if (!string.IsNullOrWhiteSpace(process.Name))
            {
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, process.Name);
            }

            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE, process.Executable ? "true" : "false");

            if (process.CandidateStarterUsers.Count > 0)
            {
                xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_USERS, BpmnXMLUtil.ConvertToDelimitedString(process.CandidateStarterUsers));
            }

            if (process.CandidateStarterGroups.Count > 0)
            {
                xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_GROUPS, BpmnXMLUtil.ConvertToDelimitedString(process.CandidateStarterGroups));
            }

            // write custom attributes
            BpmnXMLUtil.WriteCustomAttributes(process.Attributes.Values, xtw, defaultProcessAttributes);

            if (!string.IsNullOrWhiteSpace(process.Documentation))
            {

                xtw.WriteStartElement(BpmnXMLConstants.BPMN2_PREFIX, BpmnXMLConstants.ELEMENT_DOCUMENTATION, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteCharacters(process.Documentation);
                xtw.WriteEndElement();
            }

            bool didWriteExtensionStartElement = ActivitiListenerExport.WriteListeners(process, false, xtw);
            didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(process, didWriteExtensionStartElement, xtw);

            if (didWriteExtensionStartElement)
            {
                // closing extensions element
                xtw.WriteEndElement();
            }

            LaneExport.WriteLanes(process, xtw);
        }
    }

}