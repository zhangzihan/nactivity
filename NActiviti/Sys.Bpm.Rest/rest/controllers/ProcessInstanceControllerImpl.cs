using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.image.exception;
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Text;

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

namespace org.activiti.cloud.services.rest.controllers
{
    [Route("/v1/process-instances")]
    [ApiController]
    public class ProcessInstanceControllerImpl : ControllerBase, IProcessInstanceController
    {

        private ProcessEngineWrapper processEngine;

        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessInstanceResourceAssembler resourceAssembler;

        private readonly SecurityPoliciesApplicationService securityService;

        public virtual string handleAppException(ActivitiForbiddenException ex)
        {
            return ex.Message;
        }

        public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        {
            return ex.Message;
        }

        public virtual string handleActivitiInterchangeInfoNotFoundException(ActivitiInterchangeInfoNotFoundException ex)
        {
            return ex.Message;
        }

        public ProcessInstanceControllerImpl(ProcessEngineWrapper processEngine, IRepositoryService repositoryService, ProcessDiagramGeneratorWrapper processDiagramGenerator, ProcessInstanceResourceAssembler resourceAssembler, SecurityPoliciesApplicationService securityService)
        {
            this.processEngine = processEngine;
            this.repositoryService = repositoryService;
            this.processDiagramGenerator = processDiagramGenerator;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityService;
        }

        [HttpGet]
        public virtual PagedResources<ProcessInstanceResource> getProcessInstances(Pageable pageable)
        {
            return null;
            //return pagedResourcesAssembler.toResource(pageable, processEngine.getProcessInstances(pageable), resourceAssembler);
        }

        [HttpPost]
        public virtual Resource<ProcessInstance> startProcess([FromBody]StartProcessInstanceCmd cmd)
        {
            return resourceAssembler.toResource(processEngine.startProcess(cmd));
        }

        [HttpGet("{processInstanceId}")]
        public virtual Resource<ProcessInstance> getProcessInstanceById(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            //if (processInstance == null || !securityService.canRead(processInstance.ProcessDefinitionKey))
            //{
            //    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
            //}
            return resourceAssembler.toResource(processInstance);
        }

        [HttpGet("{processInstanceId}/diagram")]
        public virtual string getProcessDiagram(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            //if (processInstance == null || !securityService.canRead(processInstance.ProcessDefinitionKey))
            //{
            //    throw new ActivitiObjectNotFoundException("Unable to find process instance for the given id:'" + processInstanceId + "'");
            //}

            IList<string> activityIds = processEngine.getActiveActivityIds(processInstanceId);
            BpmnModel bpmnModel = repositoryService.getBpmnModel(processInstance.ProcessDefinitionId);
            byte[] data = processDiagramGenerator.generateDiagram(bpmnModel, activityIds, new List<string>());

            return Encoding.UTF8.GetString(data);
        }

        [HttpPost("signal")]
        public virtual IActionResult sendSignal([FromBody]SignalCmd cmd)
        {
            processEngine.signal(cmd);

            return Ok();
        }

        [HttpGet("{processInstanceId}/suspend")]
        public virtual IActionResult suspend([FromQuery]string processInstanceId)
        {
            processEngine.suspend(new SuspendProcessInstanceCmd(processInstanceId));

            return Ok();
        }

        [HttpGet("{processInstanceId}/activate")]
        public virtual IActionResult activate([FromQuery]string processInstanceId)
        {
            processEngine.activate(new ActivateProcessInstanceCmd(processInstanceId));

            return Ok();
        }

        [HttpDelete("{processInstanceId}")]
        public virtual void deleteProcessInstance(string processInstanceId)
        {
            processEngine.deleteProcessInstance(processInstanceId);
        }
    }
}