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
namespace Sys.Workflow.bpmn.converter.parser
{

    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    public class ImportParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            Import importObject = new Import();
            BpmnXMLUtil.AddXMLLocation(importObject, xtr);
            importObject.ImportType = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IMPORT_TYPE);
            importObject.Namespace = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAMESPACE);
            importObject.Location = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LOCATION);
            model.Imports.Add(importObject);
        }
    }

}