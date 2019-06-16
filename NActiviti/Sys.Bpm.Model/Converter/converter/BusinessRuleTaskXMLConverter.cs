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
namespace Sys.Workflow.bpmn.converter
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    public class BusinessRuleTaskXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(BusinessRuleTask);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_TASK_BUSINESSRULE;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            BusinessRuleTask businessRuleTask = new BusinessRuleTask();
            BpmnXMLUtil.AddXMLLocation(businessRuleTask, xtr);
            businessRuleTask.InputVariables = ParseDelimitedList(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_RULE_VARIABLES_INPUT));
            businessRuleTask.RuleNames = ParseDelimitedList(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RULES));
            businessRuleTask.ResultVariableName = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RESULT_VARIABLE);
            businessRuleTask.ClassName = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_RULE_CLASS);
            string exclude = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_RULE_EXCLUDE);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(exclude, StringComparison.CurrentCultureIgnoreCase))
            {
                businessRuleTask.Exclude = true;
            }
            ParseChildElements(XMLElementName, businessRuleTask, model, xtr);
            return businessRuleTask;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            BusinessRuleTask businessRuleTask = (BusinessRuleTask)element;
            string inputVariables = ConvertToDelimitedString(businessRuleTask.InputVariables);
            if (!string.IsNullOrWhiteSpace(inputVariables))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_RULE_VARIABLES_INPUT, inputVariables, xtw);
            }
            string ruleNames = ConvertToDelimitedString(businessRuleTask.RuleNames);
            if (!string.IsNullOrWhiteSpace(ruleNames))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RULES, ruleNames, xtw);
            }
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ResultVariableName))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RESULT_VARIABLE, businessRuleTask.ResultVariableName, xtw);
            }
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ClassName))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_RULE_CLASS, businessRuleTask.ClassName, xtw);
            }
            if (businessRuleTask.Exclude)
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_RULE_EXCLUDE, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
            }
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
    }

}