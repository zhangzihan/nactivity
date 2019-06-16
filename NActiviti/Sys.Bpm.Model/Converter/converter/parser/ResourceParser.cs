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
namespace Sys.Workflow.Bpmn.Converters.Parsers
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class ResourceParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            string resourceId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            string resourceName = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);

            Resource resource;
            if (model.ContainsResourceId(resourceId))
            {
                resource = model.GetResource(resourceId);
                resource.Name = resourceName;
                foreach (Process process in model.Processes)
                {
                    foreach (FlowElement fe in process.FlowElements)
                    {
                        if (fe is UserTask && ((UserTask)fe).CandidateGroups.Contains(resourceId))
                        {
                            ((UserTask)fe).CandidateGroups.Remove(resourceId);
                            ((UserTask)fe).CandidateGroups.Add(resourceName);
                        }
                    }
                }
            }
            else
            {
                resource = new Resource(resourceId, resourceName);
                model.AddResource(resource);
            }

            BpmnXMLUtil.AddXMLLocation(resource, xtr);
        }
    }

}