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
namespace Sys.Workflow.Bpmn.Converters.Childs
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class FlowNodeRefParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_FLOWNODE_REF;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (parentElement is not Lane)
            {
                return;
            }

            Lane lane = (Lane)parentElement;
            lane.FlowReferences.Add(xtr.ElementText);
        }
    }

}