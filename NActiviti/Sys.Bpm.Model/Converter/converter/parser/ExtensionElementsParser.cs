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
    using org.activiti.bpmn.converter.child;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using System;

    /// 
    public class ExtensionElementsParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, IList<SubProcess> activeSubProcessList, Process activeProcess, BpmnModel model)
        {
            BaseElement parentElement;
            if (activeSubProcessList.Count > 0)
            {
                parentElement = activeSubProcessList[activeSubProcessList.Count - 1];

            }
            else
            {
                parentElement = activeProcess;
            }

            bool readyWithChildElements = false;
            while (readyWithChildElements == false && xtr.HasNext())
            {
                //xtr.next();

                if (xtr.IsStartElement())
                {
                    if (BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER.Equals(xtr.LocalName))
                    {
                        (new ExecutionListenerParser()).ParseChildElement(xtr, parentElement, model);
                    }
                    else if (BpmnXMLConstants.ELEMENT_EVENT_LISTENER.Equals(xtr.LocalName))
                    {
                        (new ActivitiEventListenerParser()).ParseChildElement(xtr, parentElement, model);
                    }
                    else if (BpmnXMLConstants.ELEMENT_POTENTIAL_STARTER.Equals(xtr.LocalName))
                    {
                        (new PotentialStarterParser()).Parse(xtr, activeProcess);
                    }
                    else
                    {
                        ExtensionElement extensionElement = BpmnXMLUtil.ParseExtensionElement(xtr);
                        parentElement.AddExtensionElement(extensionElement);
                    }

                }
                else if (xtr.EndElement && constants.BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                {
                    readyWithChildElements = true;
                }

                if (xtr.IsEmptyElement && string.Compare(xtr.LocalName, constants.BpmnXMLConstants.ELEMENT_EXTENSIONS, true) == 0)
                {
                    readyWithChildElements = true;
                }
            }
        }
    }
}