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
namespace org.activiti.bpmn.converter.parser
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class SubProcessParser : IBpmnXMLConstants
    {

        public virtual void Parse(XMLStreamReader xtr, IList<SubProcess> activeSubProcessList, Process activeProcess)
        {
            SubProcess subProcess;
            if (BpmnXMLConstants.ELEMENT_TRANSACTION.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
            {
                subProcess = new Transaction();

            }
            else if (BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
            {
                AdhocSubProcess adhocSubProcess = new AdhocSubProcess();
                string orderingAttributeValue = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ORDERING);
                if (!string.IsNullOrWhiteSpace(orderingAttributeValue))
                {
                    adhocSubProcess.Ordering = orderingAttributeValue;
                }

                if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_CANCEL_REMAINING_INSTANCES), StringComparison.CurrentCultureIgnoreCase))
                {
                    adhocSubProcess.CancelRemainingInstances = false;
                }

                subProcess = adhocSubProcess;

            }
            else if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_TRIGGERED_BY), StringComparison.CurrentCultureIgnoreCase))
            {
                subProcess = new EventSubProcess();

            }
            else
            {
                subProcess = new SubProcess();
            }

            BpmnXMLUtil.AddXMLLocation(subProcess, xtr);
            activeSubProcessList.Add(subProcess);

            subProcess.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            subProcess.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);

            bool async = false;
            string asyncString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(asyncString, StringComparison.CurrentCultureIgnoreCase))
            {
                async = true;
            }

            bool notExclusive = false;
            string exclusiveString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(exclusiveString, StringComparison.CurrentCultureIgnoreCase))
            {
                notExclusive = true;
            }

            bool forCompensation = false;
            string compensationString = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(compensationString, StringComparison.CurrentCultureIgnoreCase))
            {
                forCompensation = true;
            }

            subProcess.Asynchronous = async;
            subProcess.NotExclusive = notExclusive;
            subProcess.ForCompensation = forCompensation;
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DEFAULT)))
            {
                subProcess.DefaultFlow = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DEFAULT);
            }

            if (activeSubProcessList.Count > 1)
            {
                SubProcess parentSubProcess = activeSubProcessList[activeSubProcessList.Count - 2];
                parentSubProcess.AddFlowElement(subProcess);

            }
            else
            {
                activeProcess.AddFlowElement(subProcess);
            }
        }
    }

}