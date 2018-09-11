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
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TASK_BUSINESSRULE;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            BusinessRuleTask businessRuleTask = new BusinessRuleTask();
            BpmnXMLUtil.addXMLLocation(businessRuleTask, xtr);
            businessRuleTask.InputVariables = parseDelimitedList(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_VARIABLES_INPUT));
            businessRuleTask.RuleNames = parseDelimitedList(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RULES));
            businessRuleTask.ResultVariableName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RESULT_VARIABLE);
            businessRuleTask.ClassName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_CLASS);
            string exclude = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_EXCLUDE);
            if (org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(exclude, StringComparison.CurrentCultureIgnoreCase))
            {
                businessRuleTask.Exclude = true;
            }
            parseChildElements(XMLElementName, businessRuleTask, model, xtr);
            return businessRuleTask;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            BusinessRuleTask businessRuleTask = (BusinessRuleTask)element;
            string inputVariables = convertToDelimitedString(businessRuleTask.InputVariables);
            if (!string.IsNullOrWhiteSpace(inputVariables))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_VARIABLES_INPUT, inputVariables, xtw);
            }
            string ruleNames = convertToDelimitedString(businessRuleTask.RuleNames);
            if (!string.IsNullOrWhiteSpace(ruleNames))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RULES, ruleNames, xtw);
            }
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ResultVariableName))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_RESULT_VARIABLE, businessRuleTask.ResultVariableName, xtw);
            }
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ClassName))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_CLASS, businessRuleTask.ClassName, xtw);
            }
            if (businessRuleTask.Exclude)
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_RULE_EXCLUDE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
            }
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
    }

}