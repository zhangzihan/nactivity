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
    public class ResourceParser : IBpmnXMLConstants
    {
        public virtual void parse(XMLStreamReader xtr, BpmnModel model)
        {
            string resourceId = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID);
            string resourceName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME);

            Resource resource;
            if (model.containsResourceId(resourceId))
            {
                resource = model.getResource(resourceId);
                resource.Name = resourceName;
                foreach (org.activiti.bpmn.model.Process process in model.Processes)
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
                model.addResource(resource);
            }

            BpmnXMLUtil.addXMLLocation(resource, xtr);
        }
    }

}