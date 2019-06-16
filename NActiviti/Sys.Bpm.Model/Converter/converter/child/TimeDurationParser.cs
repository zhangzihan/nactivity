﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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
namespace Sys.Workflow.bpmn.converter.child
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.model;

    /// 
    public class TimeDurationParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ATTRIBUTE_TIMER_DURATION;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is TimerEventDefinition))
            {
                return;
            }

            TimerEventDefinition eventDefinition = (TimerEventDefinition)parentElement;
            eventDefinition.TimeDuration = xtr.ElementText;
        }
    }

}