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
namespace Sys.Workflow.bpmn.converter.child
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    public class DataOutputAssociationParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_OUTPUT_ASSOCIATION;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {

            if (!(parentElement is Activity))
            {
                return;
            }
            DataAssociation dataAssociation = new DataAssociation();
            BpmnXMLUtil.AddXMLLocation(dataAssociation, xtr);
            DataAssociationParser.parseDataAssociation(dataAssociation, ElementName, xtr);

            ((Activity)parentElement).DataOutputAssociations.Add(dataAssociation);
        }
    }

}