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

    public class MultiInstanceExport : IBpmnXMLConstants
    {
        public static void writeMultiInstance(Activity activity, XMLStreamWriter xtw)
        {
            if (activity.LoopCharacteristics != null)
            {
                MultiInstanceLoopCharacteristics multiInstanceObject = activity.LoopCharacteristics;
                if (!string.IsNullOrWhiteSpace(multiInstanceObject.LoopCardinality) || !string.IsNullOrWhiteSpace(multiInstanceObject.InputDataItem) || !string.IsNullOrWhiteSpace(multiInstanceObject.CompletionCondition))
                {

                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MULTIINSTANCE);
                    BpmnXMLUtil.writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL, multiInstanceObject.Sequential.ToString().ToLower(), xtw);
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.InputDataItem))
                    {
                        BpmnXMLUtil.writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION, multiInstanceObject.InputDataItem, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.ElementVariable))
                    {
                        BpmnXMLUtil.writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE, multiInstanceObject.ElementVariable, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.LoopCardinality))
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CARDINALITY);
                        xtw.writeCharacters(multiInstanceObject.LoopCardinality);
                        xtw.writeEndElement();
                    }
                    if (!string.IsNullOrWhiteSpace(multiInstanceObject.CompletionCondition))
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CONDITION);
                        xtw.writeCharacters(multiInstanceObject.CompletionCondition);
                        xtw.writeEndElement();
                    }
                    xtw.writeEndElement();
                }
            }
        }
    }

}