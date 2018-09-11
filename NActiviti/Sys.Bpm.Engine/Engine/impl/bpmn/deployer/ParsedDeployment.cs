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

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// An intermediate representation of a DeploymentEntity which keeps track of all of the entity's
    /// ProcessDefinitionEntities and resources, and BPMN parses, models, and processes associated
    /// with each ProcessDefinitionEntity - all produced by parsing the deployment.
    /// 
    /// The ProcessDefinitionEntities are expected to be "not fully set-up" - they may be inconsistent with the 
    /// DeploymentEntity and/or the persisted versions, and if the deployment is new, they will not yet be persisted.
    /// </summary>
    public class ParsedDeployment
    {

        protected internal IDeploymentEntity deploymentEntity;

        protected internal IList<IProcessDefinitionEntity> processDefinitions;
        protected internal IDictionary<IProcessDefinitionEntity, BpmnParse> mapProcessDefinitionsToParses;
        protected internal IDictionary<IProcessDefinitionEntity, IResourceEntity> mapProcessDefinitionsToResources;

        public ParsedDeployment(IDeploymentEntity entity, IList<IProcessDefinitionEntity> processDefinitions, IDictionary<IProcessDefinitionEntity, BpmnParse> mapProcessDefinitionsToParses, IDictionary<IProcessDefinitionEntity, IResourceEntity> mapProcessDefinitionsToResources)
        {
            this.deploymentEntity = entity;
            this.processDefinitions = processDefinitions;
            this.mapProcessDefinitionsToParses = mapProcessDefinitionsToParses;
            this.mapProcessDefinitionsToResources = mapProcessDefinitionsToResources;
        }


        public virtual IDeploymentEntity Deployment
        {
            get
            {
                return deploymentEntity;
            }
        }

        public virtual IList<IProcessDefinitionEntity> AllProcessDefinitions
        {
            get
            {
                return processDefinitions;
            }
        }

        public virtual IResourceEntity getResourceForProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            mapProcessDefinitionsToResources.TryGetValue(processDefinition, out var res);

            return res;
        }

        public virtual BpmnParse getBpmnParseForProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            mapProcessDefinitionsToParses.TryGetValue(processDefinition, out var parser);

            return parser;
        }

        public virtual BpmnModel getBpmnModelForProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            BpmnParse parse = getBpmnParseForProcessDefinition(processDefinition);

            return (parse == null ? null : parse.BpmnModel);
        }

        public virtual Process getProcessModelForProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            BpmnModel model = getBpmnModelForProcessDefinition(processDefinition);

            return (model == null ? null : model.getProcessById(processDefinition.Key));
        }

    }


}