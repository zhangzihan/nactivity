/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Linq;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.activiti.image.exception;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace org.activiti.cloud.services.rest.controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_DEF_ROUTER_V1)]
    [ApiController]
    public class ProcessDefinitionControllerImpl : ControllerBase, IProcessDefinitionController
    {
        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessDefinitionConverter processDefinitionConverter;

        private readonly ProcessDefinitionResourceAssembler resourceAssembler;

        private readonly PageableProcessDefinitionRepositoryService pageableRepositoryService;

        private readonly SecurityPoliciesApplicationService securityService;

        //private readonly AlfrescoPagedResourcesAssembler<ProcessDefinition> pagedResourcesAssembler;

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}


        //public virtual string handleDiagramInterchangeInfoNotFoundException(ActivitiInterchangeInfoNotFoundException ex)
        //{
        //    return ex.Message;
        //}


        /// <inheritdoc />
        public ProcessDefinitionControllerImpl(IProcessEngine processEngine,
            ProcessDefinitionConverter processDefinitionConverter,
            ProcessDefinitionResourceAssembler resourceAssembler,
            PageableProcessDefinitionRepositoryService pageableRepositoryService,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.repositoryService = processEngine.RepositoryService;
            this.processDefinitionConverter = processDefinitionConverter;
            this.resourceAssembler = resourceAssembler;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityService = securityPoliciesApplicationService;
        }


        /// <inheritdoc />
        [HttpPost("latest")]
        public virtual Task<Resources<ProcessDefinition>> LatestProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            queryObj = queryObj ?? new ProcessDefinitionQuery();
            queryObj.Latest = true;

            return ProcessDefinitions(queryObj);
        }


        /// <inheritdoc />
        [HttpPost("list")]
        public virtual Task<Resources<ProcessDefinition>> ProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            IPage<ProcessDefinition> defs = new QueryProcessDefinitionCmd().loadPage(this.repositoryService, this.pageableRepositoryService, queryObj);

            IList<ProcessDefinitionResource> resources = resourceAssembler.toResources(defs.getContent());

            Resources<ProcessDefinition> list = new Resources<ProcessDefinition>(resources.Select(x => x.Content), defs.getTotalItems(), queryObj.Pageable.PageNo, queryObj.Pageable.PageSize);

            return Task.FromResult(list);
        }


        /// <inheritdoc />
        [HttpGet("{id}")]
        public virtual Task<ProcessDefinition> GetProcessDefinition(string id)
        {
            engine.repository.IProcessDefinition processDefinition = retrieveProcessDefinition(id);

            ProcessDefinitionResource resource = resourceAssembler.toResource(processDefinitionConverter.from(processDefinition));

            return Task.FromResult(resource.Content);
        }


        /// <inheritdoc />
        private IProcessDefinition retrieveProcessDefinition(string id)
        {
            IProcessDefinitionQuery query = repositoryService.createProcessDefinitionQuery().processDefinitionId(id);
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);
            engine.repository.IProcessDefinition processDefinition = query.singleResult();
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
            }
            return processDefinition;
        }


        /// <inheritdoc />
        [HttpGet("{id}/processmodel")]
        public virtual Task<string> GetProcessModel(string id)
        {
            // first check the user can see the process definition (which has same ID as process model in engine)
            retrieveProcessDefinition(id);

            using (Stream resourceStream = repositoryService.getProcessModel(id))
            {
                resourceStream.Seek(0, SeekOrigin.Begin);
                byte[] data = new byte[resourceStream.Length];
                resourceStream.Read(data, 0, data.Length);

                string xml = Encoding.UTF8.GetString(data);

                return Task.FromResult<string>(xml);
            }
        }

        /// <inheritdoc />
        [HttpGet("{id}/bpmnmodel")]
        public virtual Task<BpmnModel> GetBpmnModel(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            retrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.getBpmnModel(id);

            return Task.FromResult<BpmnModel>(bpmnModel);
        }


        /// <inheritdoc />
        [HttpGet("{id}/diagram")]
        public virtual Task<string> GetProcessDiagram(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            retrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.getBpmnModel(id);

            byte[] data = processDiagramGenerator.generateDiagram(bpmnModel);

            return Task.FromResult<string>(Encoding.UTF8.GetString(data));
        }
    }

}