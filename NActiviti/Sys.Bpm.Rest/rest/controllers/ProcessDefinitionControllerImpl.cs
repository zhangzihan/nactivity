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
using org.activiti.bpmn.model;
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
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.controllers
{
    [Route("workflow/process-definitions")]
    [ApiController]
    public class ProcessDefinitionControllerImpl : ControllerBase, IProcessDefinitionController
    {
        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessDefinitionConverter processDefinitionConverter;

        private readonly ProcessDefinitionResourceAssembler resourceAssembler;

        private readonly PageableRepositoryService pageableRepositoryService;

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

        public ProcessDefinitionControllerImpl(IProcessEngine processEngine,
            ProcessDefinitionConverter processDefinitionConverter,
            ProcessDefinitionResourceAssembler resourceAssembler,
            PageableRepositoryService pageableRepositoryService,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.repositoryService = processEngine.RepositoryService;
            this.processDefinitionConverter = processDefinitionConverter;
            this.resourceAssembler = resourceAssembler;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityService = securityPoliciesApplicationService;
        }

        //public ProcessDefinitionControllerImpl(IRepositoryService repositoryService, 
        //    ProcessDiagramGeneratorWrapper processDiagramGenerator, 
        //    ProcessDefinitionConverter processDefinitionConverter, 
        //    ProcessDefinitionResourceAssembler resourceAssembler, 
        //    PageableRepositoryService pageableRepositoryService, 
        //    SecurityPoliciesApplicationService securityPoliciesApplicationService)
        //{
        //    this.repositoryService = repositoryService;
        //    this.processDiagramGenerator = processDiagramGenerator;
        //    this.processDefinitionConverter = processDefinitionConverter;
        //    this.resourceAssembler = resourceAssembler;
        //    this.pageableRepositoryService = pageableRepositoryService;
        //    this.securityService = securityPoliciesApplicationService;
        //}

        [HttpGet("latest")]
        public virtual Task<IList<ProcessDefinitionResource>> GetLatestProcessDefinitions([FromQuery]Pageable pageable)
        {
            IList<IProcessDefinition> defs = repositoryService.createProcessDefinitionQuery().latestVersion().list();

            IList<ProcessDefinitionResource> resources = resourceAssembler.toResources(processDefinitionConverter.from(defs));

            //Page<ProcessDefinition> page = pageableRepositoryService.getProcessDefinitions(pageable);

            //pagedResourcesAssembler.toResource(pageable, page, resourceAssembler);

            return System.Threading.Tasks.Task.FromResult<IList<ProcessDefinitionResource>>(resources);
        }

        [HttpGet]
        public virtual Task<IList<ProcessDefinitionResource>> GetProcessDefinitions([FromQuery]Pageable pageable = null)
        {
            IList<IProcessDefinition> defs = repositoryService.createProcessDefinitionQuery().list();

            IList<ProcessDefinitionResource> resources = resourceAssembler.toResources(processDefinitionConverter.from(defs));

            //Page<ProcessDefinition> page = pageableRepositoryService.getProcessDefinitions(pageable);

            //pagedResourcesAssembler.toResource(pageable, page, resourceAssembler);

            return System.Threading.Tasks.Task.FromResult<IList<ProcessDefinitionResource>>(resources);
        }

        [HttpGet("{id}")]
        public virtual ProcessDefinitionResource GetProcessDefinition(string id)
        {
            IProcessDefinition processDefinition = retrieveProcessDefinition(id);
            return resourceAssembler.toResource(processDefinitionConverter.from(processDefinition));
        }

        private IProcessDefinition retrieveProcessDefinition(string id)
        {
            IProcessDefinitionQuery query = repositoryService.createProcessDefinitionQuery().processDefinitionId(id);
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);
            IProcessDefinition processDefinition = query.singleResult();
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
            }
            return processDefinition;
        }

        [HttpGet("{id}/processmodel")]
        public virtual Task<ContentResult> GetProcessModel(string id)
        {
            try
            {
                // first check the user can see the process definition (which has same ID as process model in engine)
                retrieveProcessDefinition(id);

                using (System.IO.Stream resourceStream = repositoryService.getProcessModel(id))
                {
                    resourceStream.Seek(0, SeekOrigin.Begin);
                    byte[] data = new byte[resourceStream.Length];
                    resourceStream.Read(data, 0, data.Length);

                    string xml = Encoding.UTF8.GetString(data);

                    return System.Threading.Tasks.Task.FromResult<ContentResult>(
                        new ContentResult
                        {
                            ContentType = "application/xml",
                            StatusCode = 200,
                            Content = xml,
                        });
                }
            }
            catch(ActivitiObjectNotFoundException e)
            {
                return System.Threading.Tasks.Task.FromResult<ContentResult>(
                           new ContentResult
                           {
                               ContentType = "application/xml",
                               StatusCode = 200,
                               Content = null,
                           });
            }
            catch (IOException e)
            {
                throw new ActivitiException("Error occured while getting process model '" + id + "' : " + e.Message, e);
            }
        }

        [HttpGet("{id}/bpmnmodel")]
        [Produces("application/json")]
        public virtual string GetBpmnModel(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            retrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.getBpmnModel(id);
            JToken json = JToken.FromObject(bpmnModel);

            return json.ToString();
        }

        [HttpGet("{id}/diagram")]
        [Produces("image/svg+xml")]
        public virtual string GetProcessDiagram(string id)
        {
            // first check the user can see the process definition (which has same ID as BPMN model in engine)
            retrieveProcessDefinition(id);

            BpmnModel bpmnModel = repositoryService.getBpmnModel(id);

            byte[] data = processDiagramGenerator.generateDiagram(bpmnModel);

            return Encoding.UTF8.GetString(data);
        }
    }

}