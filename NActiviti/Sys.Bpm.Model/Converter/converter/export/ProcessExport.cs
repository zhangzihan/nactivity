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
namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    public class ProcessExport : IBpmnXMLConstants
    {
        /// <summary>
        /// default attributes taken from process instance attributes
        /// </summary>
        public static readonly IList<ExtensionAttribute> defaultProcessAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_USERS),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_GROUPS)
        };
        public static void writeProcess(Process process, XMLStreamWriter xtw)
        {
            // start process element
            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_PROCESS);
            xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, process.Id);

            if (!string.IsNullOrWhiteSpace(process.Name))
            {
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, process.Name);
            }

            xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE, Convert.ToString(process.Executable));

            if (process.CandidateStarterUsers.Count > 0)
            {
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_USERS, BpmnXMLUtil.convertToDelimitedString(process.CandidateStarterUsers));
            }

            if (process.CandidateStarterGroups.Count > 0)
            {
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_CANDIDATE_GROUPS, BpmnXMLUtil.convertToDelimitedString(process.CandidateStarterGroups));
            }

            // write custom attributes
            BpmnXMLUtil.writeCustomAttributes(process.Attributes.Values, xtw, defaultProcessAttributes);

            if (!string.IsNullOrWhiteSpace(process.Documentation))
            {

                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DOCUMENTATION);
                xtw.writeCharacters(process.Documentation);
                xtw.writeEndElement();
            }

            bool didWriteExtensionStartElement = ActivitiListenerExport.writeListeners(process, false, xtw);
            didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(process, didWriteExtensionStartElement, xtw);

            if (didWriteExtensionStartElement)
            {
                // closing extensions element
                xtw.writeEndElement();
            }

            LaneExport.writeLanes(process, xtw);
        }
    }

}