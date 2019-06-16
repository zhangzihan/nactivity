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
using Newtonsoft.Json;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.api.model.converter;
using Sys.Workflow.cloud.services.api.utils;
using Sys.Workflow.cloud.services.core;
using Sys.Workflow.cloud.services.core.pageable;
using Sys.Workflow.cloud.services.rest.api;
using Sys.Workflow.cloud.services.rest.api.resources;
using Sys.Workflow.cloud.services.rest.assemblers;
using Sys.Workflow.engine;
using Sys.Workflow.engine.impl;
using Sys.Workflow.engine.repository;
using org.springframework.hateoas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Sys.Workflow.cloud.services.rest.controllers
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
            IPage<ProcessDefinition> defs = new QueryProcessDefinitionCmd().LoadPage(this.repositoryService, this.pageableRepositoryService, queryObj);

            IList<ProcessDefinitionResource> resources = resourceAssembler.ToResources(defs.GetContent());

            Resources<ProcessDefinition> list = new Resources<ProcessDefinition>(resources.Select(x => x.Content), defs.GetTotalItems(), queryObj.Pageable.PageNo, queryObj.Pageable.PageSize);

            return Task.FromResult(list);
        }


        /// <inheritdoc />
        [HttpGet("{id}")]
        public virtual Task<ProcessDefinition> GetProcessDefinition(string id)
        {
            IProcessDefinition processDefinition = RetrieveProcessDefinition(id);

            ProcessDefinitionResource resource = resourceAssembler.ToResource(processDefinitionConverter.From(processDefinition));

            return Task.FromResult(resource.Content);
        }


        /// <inheritdoc />
        private IProcessDefinition RetrieveProcessDefinition(string id)
        {
            IProcessDefinitionQuery query = repositoryService.CreateProcessDefinitionQuery().SetProcessDefinitionId(id);
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);
            IProcessDefinition processDefinition = query.SingleResult();
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
            }
            return processDefinition;
        }


        /// <inheritdoc />
        [HttpPost("processmodel")]
        public virtual Task<string> GetProcessModel(ProcessDefinitionQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            ProcessDefinitionQueryImpl qo = repositoryService.CreateProcessDefinitionQuery() as ProcessDefinitionQueryImpl;

            FastCopy.Copy<ProcessDefinitionQuery, ProcessDefinitionQueryImpl>(query, qo);
            qo.SetLatestVersion();

            IProcessDefinition processDefinition = qo.SingleResult();

            return GetProcessModel(processDefinition.Id);
        }


        /// <inheritdoc />
        [HttpGet("{id}/processmodel")]
        public virtual Task<string> GetProcessModel(string id)
        {
            // first check the user can see the process definition (which has same ID as process model in engine)
            //retrieveProcessDefinition(id);

            using (Stream resourceStream = repositoryService.GetProcessModel(id))
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
        public virtual Task<ActionResult<BpmnModel>> GetBpmnModel(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            RetrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.GetBpmnModel(id);

            return Task.FromResult<ActionResult<BpmnModel>>(new JsonResult(bpmnModel, new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }


        /// <inheritdoc />
        [HttpGet("{id}/diagram")]
        public virtual Task<string> GetProcessDiagram(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            RetrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.GetBpmnModel(id);

            byte[] data = processDiagramGenerator.GenerateDiagram(bpmnModel);

            return Task.FromResult<string>(Encoding.UTF8.GetString(data));
        }
    }

}