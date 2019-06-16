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
namespace Sys.Workflow.bpmn.converter.export
{

    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    public class MultiInstanceExport : IBpmnXMLConstants
    {
        public static void WriteMultiInstance(Activity activity, XMLStreamWriter xtw)
        {
            if (activity.LoopCharacteristics != null)
            {
                MultiInstanceLoopCharacteristics multiInstanceObject = activity.LoopCharacteristics;
                if (!string.IsNullOrWhiteSpace(multiInstanceObject.LoopCardinality) || !string.IsNullOrWhiteSpace(multiInstanceObject.InputDataItem) || !string.IsNullOrWhiteSpace(multiInstanceObject.CompletionCondition))
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE);
                    BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL, multiInstanceObject.Sequential ? "true" : "false", xtw);
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.InputDataItem))
                    {
                        BpmnXMLUtil.WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION, multiInstanceObject.InputDataItem, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.ElementVariable))
                    {
                        BpmnXMLUtil.WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE, multiInstanceObject.ElementVariable, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.LoopCardinality))
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CARDINALITY, BpmnXMLConstants.BPMN2_NAMESPACE);
                        xtw.WriteCharacters(multiInstanceObject.LoopCardinality);
                        xtw.WriteEndElement();
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.CompletionCondition))
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CONDITION, BpmnXMLConstants.BPMN2_NAMESPACE);
                        xtw.WriteCharacters(multiInstanceObject.CompletionCondition);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                }
                else
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE);
                    BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL, multiInstanceObject.Sequential ? "true" : "false", xtw);
                    xtw.WriteEndElement();
                }
            }
        }
    }

}