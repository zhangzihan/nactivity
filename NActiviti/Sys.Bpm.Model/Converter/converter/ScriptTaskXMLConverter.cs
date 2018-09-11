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
namespace org.activiti.bpmn.converter
{
    using org.activiti.bpmn.converter.child;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class ScriptTaskXMLConverter : BaseBpmnXMLConverter
    {

        protected internal IDictionary<string, BaseChildElementParser> childParserMap = new Dictionary<string, BaseChildElementParser>();

        public ScriptTaskXMLConverter()
        {
            ScriptTextParser scriptTextParser = new ScriptTextParser();
            childParserMap[scriptTextParser.ElementName] = scriptTextParser;
        }

        public override Type BpmnElementType
        {
            get
            {
                return typeof(ScriptTask);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TASK_SCRIPT;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ScriptTask scriptTask = new ScriptTask();
            BpmnXMLUtil.addXMLLocation(scriptTask, xtr);
            scriptTask.ScriptFormat = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_FORMAT);
            scriptTask.ResultVariable = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_RESULTVARIABLE);
            if (string.IsNullOrWhiteSpace(scriptTask.ResultVariable))
            {
                scriptTask.ResultVariable = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE);
            }
            string autoStoreVariables = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_AUTO_STORE_VARIABLE);
            if (!string.IsNullOrWhiteSpace(autoStoreVariables))
            {
                scriptTask.AutoStoreVariables = Convert.ToBoolean(autoStoreVariables);
            }
            parseChildElements(XMLElementName, scriptTask, childParserMap, model, xtr);
            return scriptTask;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            ScriptTask scriptTask = (ScriptTask)element;
            writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_FORMAT, scriptTask.ScriptFormat, xtw);
            writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_RESULTVARIABLE, scriptTask.ResultVariable, xtw);
            writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_AUTO_STORE_VARIABLE, scriptTask.AutoStoreVariables.ToString(), xtw);
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            ScriptTask scriptTask = (ScriptTask)element;
            if (!string.IsNullOrWhiteSpace(scriptTask.Script))
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_TEXT);
                xtw.writeCData(scriptTask.Script);
                xtw.writeEndElement();
            }
        }
    }

}