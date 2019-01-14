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
namespace org.activiti.engine.impl.bpmn.deployer
{

    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.persistence.entity;

    public class ParsedDeploymentBuilder
    {
        protected internal IDeploymentEntity deployment;
        protected internal BpmnParser bpmnParser;
        protected internal IDictionary<string, object> deploymentSettings;

        public ParsedDeploymentBuilder(IDeploymentEntity deployment, BpmnParser bpmnParser, IDictionary<string, object> deploymentSettings)
        {
            this.deployment = deployment;
            this.bpmnParser = bpmnParser;
            this.deploymentSettings = deploymentSettings;
        }

        public virtual ParsedDeployment build()
        {
            IList<IProcessDefinitionEntity> processDefinitions = new List<IProcessDefinitionEntity>();
            IDictionary<IProcessDefinitionEntity, BpmnParse> processDefinitionsToBpmnParseMap = new Dictionary<IProcessDefinitionEntity, BpmnParse>();
            IDictionary<IProcessDefinitionEntity, IResourceEntity> processDefinitionsToResourceMap = new Dictionary<IProcessDefinitionEntity, IResourceEntity>();

            var resources = deployment.GetResources().Values;
            foreach (IResourceEntity resource in resources)
            {
                if (isBpmnResource(resource.Name))
                {
                    //log.debug("Processing BPMN resource {}", resource.Name);
                    BpmnParse parse = createBpmnParseFromResource(resource);
                    foreach (IProcessDefinitionEntity processDefinition in parse.ProcessDefinitions)
                    {
                        processDefinitions.Add(processDefinition);
                        processDefinitionsToBpmnParseMap[processDefinition] = parse;
                        processDefinitionsToResourceMap[processDefinition] = resource;
                    }
                }
            }

            return new ParsedDeployment(deployment, processDefinitions, processDefinitionsToBpmnParseMap, processDefinitionsToResourceMap);
        }

        protected internal virtual BpmnParse createBpmnParseFromResource(IResourceEntity resource)
        {
            string resourceName = resource.Name;
            System.IO.MemoryStream inputStream = new System.IO.MemoryStream(resource.Bytes);

            BpmnParse bpmnParse = bpmnParser.createParse().sourceInputStream(inputStream).setSourceSystemId(resourceName).SetName(resourceName);
            bpmnParse.Deployment = deployment;

            if (deploymentSettings != null)
            {

                // Schema validation if needed
                if (deploymentSettings.ContainsKey(org.activiti.engine.impl.cmd.DeploymentSettings_Fields.IS_BPMN20_XSD_VALIDATION_ENABLED))
                {
                    bpmnParse.ValidateSchema = Convert.ToBoolean(deploymentSettings[org.activiti.engine.impl.cmd.DeploymentSettings_Fields.IS_BPMN20_XSD_VALIDATION_ENABLED]);
                }

                // Process validation if needed
                if (deploymentSettings.ContainsKey(org.activiti.engine.impl.cmd.DeploymentSettings_Fields.IS_PROCESS_VALIDATION_ENABLED))
                {
                    bpmnParse.ValidateProcess = Convert.ToBoolean(deploymentSettings[org.activiti.engine.impl.cmd.DeploymentSettings_Fields.IS_PROCESS_VALIDATION_ENABLED]);
                }

            }
            else
            {
                // On redeploy, we assume it is validated at the first deploy
                bpmnParse.ValidateSchema = false;
                bpmnParse.ValidateProcess = false;
            }

            bpmnParse.execute();
            return bpmnParse;
        }

        protected internal virtual bool isBpmnResource(string resourceName)
        {
            foreach (string suffix in ResourceNameUtil.BPMN_RESOURCE_SUFFIXES)
            {
                if (resourceName.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

    }
}