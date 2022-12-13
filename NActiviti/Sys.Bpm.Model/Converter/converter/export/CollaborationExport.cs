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
    using Sys.Workflow.Bpmn.Models;

    public class CollaborationExport : IBpmnXMLConstants
    {
        public static void WritePools(BpmnModel model, XMLStreamWriter xtw)
        {
            if ((model.Pools?.Count).GetValueOrDefault() > 0)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN2_PREFIX, BpmnXMLConstants.ELEMENT_COLLABORATION, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "Collaboration");
                foreach (Pool pool in model.Pools)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN2_PREFIX, BpmnXMLConstants.ELEMENT_PARTICIPANT, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, pool.Id);
                    if (!string.IsNullOrWhiteSpace(pool.Name))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, pool.Name);
                    }
                    if (!string.IsNullOrWhiteSpace(pool.ProcessRef))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_PROCESS_REF, pool.ProcessRef);
                    }
                    xtw.WriteEndElement();
                }

                foreach (MessageFlow messageFlow in model.MessageFlows.Values)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN2_PREFIX, BpmnXMLConstants.ELEMENT_MESSAGE_FLOW, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, messageFlow.Id);
                    if (!string.IsNullOrWhiteSpace(messageFlow.Name))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, messageFlow.Name);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.SourceRef))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF, messageFlow.SourceRef);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.TargetRef))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF, messageFlow.TargetRef);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.MessageRef))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_MESSAGE_REF, messageFlow.MessageRef);
                    }
                    xtw.WriteEndElement();
                }

                xtw.WriteEndElement();
            }
        }
    }

}