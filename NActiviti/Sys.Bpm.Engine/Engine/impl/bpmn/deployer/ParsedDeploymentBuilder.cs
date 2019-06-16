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
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.bpmn.parser;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow;
    using System.IO;

    public class ParsedDeploymentBuilder
    {
        protected internal IDeploymentEntity deployment;
        protected internal BpmnParser bpmnParser;
        protected internal IDictionary<string, object> deploymentSettings;

        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ParsedDeploymentBuilder>();

        public ParsedDeploymentBuilder(IDeploymentEntity deployment, BpmnParser bpmnParser, IDictionary<string, object> deploymentSettings)
        {
            this.deployment = deployment;
            this.bpmnParser = bpmnParser;
            this.deploymentSettings = deploymentSettings;
        }

        public virtual ParsedDeployment Build(BpmnDeploymentHelper bpmnDeploymentHelper)
        {
            IList<IProcessDefinitionEntity> processDefinitions = new List<IProcessDefinitionEntity>();
            IDictionary<IProcessDefinitionEntity, BpmnParse> processDefinitionsToBpmnParseMap = new Dictionary<IProcessDefinitionEntity, BpmnParse>();
            IDictionary<IProcessDefinitionEntity, IResourceEntity> processDefinitionsToResourceMap = new Dictionary<IProcessDefinitionEntity, IResourceEntity>();

            var resources = deployment.GetResources().Values;
            foreach (IResourceEntity resource in resources)
            {
                if (IsBpmnResource(resource.Name))
                {
                    log.LogDebug($"Processing BPMN resource {resource.Name}");
                    BpmnParse parse = CreateBpmnParseFromResource(bpmnDeploymentHelper, resource);
                    foreach (IProcessDefinitionEntity processDefinition in parse.ProcessDefinitions)
                    {
                        processDefinition.BusinessKey = parse.Deployment.BusinessKey;
                        processDefinition.StartForm = parse.Deployment.StartForm;
                        processDefinition.BusinessPath = parse.Deployment.BusinessPath;
                        processDefinitions.Add(processDefinition);
                        processDefinitionsToBpmnParseMap[processDefinition] = parse;
                        processDefinitionsToResourceMap[processDefinition] = resource;
                    }
                }
            }

            return new ParsedDeployment(deployment, processDefinitions, processDefinitionsToBpmnParseMap, processDefinitionsToResourceMap);
        }

        private BpmnParse CreateBpmnParseFromResource(BpmnDeploymentHelper helper, IResourceEntity resource)
        {
            string resourceName = resource.Name;
            MemoryStream inputStream = new MemoryStream(resource.Bytes);
            var changed = helper.AddCamundaNamespace(inputStream);
            if (changed.Item1)
            {
                resource.Bytes = changed.Item2.ToArray();
                inputStream = changed.Item2;
            };
            inputStream.Seek(0, SeekOrigin.Begin);

            BpmnParse bpmnParse = bpmnParser.CreateParse().SourceInputStream(inputStream).SetSourceSystemId(resourceName).SetName(resourceName);
            bpmnParse.Deployment = deployment;

            if (deploymentSettings != null)
            {
                // Schema validation if needed
                if (deploymentSettings.ContainsKey(cmd.DeploymentSettingsFields.IS_BPMN20_XSD_VALIDATION_ENABLED))
                {
                    bpmnParse.ValidateSchema = Convert.ToBoolean(deploymentSettings[cmd.DeploymentSettingsFields.IS_BPMN20_XSD_VALIDATION_ENABLED]);
                }

                // Process validation if needed
                if (deploymentSettings.ContainsKey(cmd.DeploymentSettingsFields.IS_PROCESS_VALIDATION_ENABLED))
                {
                    bpmnParse.ValidateProcess = Convert.ToBoolean(deploymentSettings[cmd.DeploymentSettingsFields.IS_PROCESS_VALIDATION_ENABLED]);
                }
            }
            else
            {
                // On redeploy, we assume it is validated at the first deploy
                bpmnParse.ValidateSchema = false;
                bpmnParse.ValidateProcess = false;
            }

            bpmnParse.Execute();
            return bpmnParse;
        }

        protected internal virtual bool IsBpmnResource(string resourceName)
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