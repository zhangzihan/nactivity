using System;

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
namespace org.activiti.bpmn.converter
{
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class ManualTaskXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(ManualTask);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TASK_MANUAL;
            }
        }

        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ManualTask manualTask = new ManualTask();
            BpmnXMLUtil.addXMLLocation(manualTask, xtr);
            parseChildElements(XMLElementName, manualTask, model, xtr);
            return manualTask;
        }

        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

        }
    }

}