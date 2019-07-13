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
namespace Sys.Workflow.Bpmn.Converters
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Childs;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

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
                return BpmnXMLConstants.ELEMENT_TASK_SCRIPT;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ScriptTask scriptTask = new ScriptTask();
            BpmnXMLUtil.AddXMLLocation(scriptTask, xtr);
            scriptTask.ScriptFormat = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_FORMAT);
            scriptTask.ResultVariable = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_RESULTVARIABLE);
            if (string.IsNullOrWhiteSpace(scriptTask.ResultVariable))
            {
                scriptTask.ResultVariable = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE);
            }
            string autoStoreVariables = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_AUTO_STORE_VARIABLE);
            if (!string.IsNullOrWhiteSpace(autoStoreVariables))
            {
                scriptTask.AutoStoreVariables = Convert.ToBoolean(autoStoreVariables);
            }
            ParseChildElements(XMLElementName, scriptTask, childParserMap, model, xtr);
            return scriptTask;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            ScriptTask scriptTask = (ScriptTask)element;
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_FORMAT, scriptTask.ScriptFormat, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_RESULTVARIABLE, scriptTask.ResultVariable, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_AUTO_STORE_VARIABLE, scriptTask.AutoStoreVariables.ToString(), xtw);
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            ScriptTask scriptTask = (ScriptTask)element;
            if (!string.IsNullOrWhiteSpace(scriptTask.Script))
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ATTRIBUTE_TASK_SCRIPT_TEXT, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteCharacters(scriptTask.Script);
                xtw.WriteEndElement();
            }
        }
    }

}