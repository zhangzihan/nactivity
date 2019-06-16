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
namespace Sys.Workflow.engine.impl.bpmn.deployer
{
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// Static methods for working with BPMN and image resource names.
    /// </summary>
    public class ResourceNameUtil
    {

        public static readonly string[] BPMN_RESOURCE_SUFFIXES = new string[] { "bpmn20.xml", "bpmn" };
        public static readonly string[] DIAGRAM_SUFFIXES = new string[] { "png", "jpg", "gif", "svg" };

        public static string StripBpmnFileSuffix(string bpmnFileResource)
        {
            foreach (string suffix in BPMN_RESOURCE_SUFFIXES)
            {
                if (bpmnFileResource.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return bpmnFileResource.Substring(0, bpmnFileResource.Length - suffix.Length);
                }
            }
            return bpmnFileResource;
        }

        public static string GetProcessDiagramResourceName(string bpmnFileResource, string processKey, string diagramSuffix)
        {
            string bpmnFileResourceBase = StripBpmnFileSuffix(bpmnFileResource);
            return bpmnFileResourceBase + processKey + "." + diagramSuffix;
        }

        /// <summary>
        /// Finds the name of a resource for the diagram for a process definition.  Assumes that the
        /// process definition's key and (BPMN) resource name are already set.
        /// 
        /// <para>It will first look for an image resource which matches the process specifically, before
        /// resorting to an image resource which matches the BPMN 2.0 xml file resource.
        /// 
        /// </para>
        /// <para>Example: if the deployment contains a BPMN 2.0 xml resource called 'abc.bpmn20.xml'
        /// containing only one process with key 'myProcess', then this method will look for an image resources
        /// called'abc.myProcess.png' (or .jpg, or .gif, etc.) or 'abc.png' if the previous one wasn't
        /// found.
        /// 
        /// </para>
        /// <para>Example 2: if the deployment contains a BPMN 2.0 xml resource called 'abc.bpmn20.xml'
        /// containing three processes (with keys a, b and c), then this method will first look for an image resource
        /// called 'abc.a.png' before looking for 'abc.png' (likewise for b and c). Note that if abc.a.png,
        /// abc.b.png and abc.c.png don't exist, all processes will have the same image: abc.png.
        /// 
        /// </para>
        /// </summary>
        /// <returns> name of an existing resource, or null if no matching image resource is found in the resources. </returns>
        public static string GetProcessDiagramResourceNameFromDeployment(IProcessDefinitionEntity processDefinition, IDictionary<string, IResourceEntity> resources)
        {

            if (string.IsNullOrWhiteSpace(processDefinition.ResourceName))
            {
                throw new System.InvalidOperationException("Provided process definition must have its resource name set.");
            }

            string bpmnResourceBase = StripBpmnFileSuffix(processDefinition.ResourceName);
            string key = processDefinition.Key;

            foreach (string diagramSuffix in DIAGRAM_SUFFIXES)
            {
                string possibleName = bpmnResourceBase + key + "." + diagramSuffix;
                if (resources.ContainsKey(possibleName))
                {
                    return possibleName;
                }

                possibleName = bpmnResourceBase + diagramSuffix;
                if (resources.ContainsKey(possibleName))
                {
                    return possibleName;
                }
            }

            return null;
        }
    }
}