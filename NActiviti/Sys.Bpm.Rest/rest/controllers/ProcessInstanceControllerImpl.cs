using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.engine.runtime;
using org.activiti.image.exception;
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
    [Route("workflow/process-instances")]
    [ApiController]
    public class ProcessInstanceControllerImpl : ControllerBase, IProcessInstanceController
    {
        private ProcessEngineWrapper processEngine;

        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessInstanceResourceAssembler resourceAssembler;

        private readonly SecurityPoliciesApplicationService securityService;

        //public virtual string handleAppException(ActivitiForbiddenException ex)
        //{
        //    return ex.Message;
        //}

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        //public virtual string handleActivitiInterchangeInfoNotFoundException(ActivitiInterchangeInfoNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        public ProcessInstanceControllerImpl(ProcessEngineWrapper processEngine,
            ProcessInstanceResourceAssembler resourceAssembler)
        {
            this.processEngine = processEngine;
            this.repositoryService = repositoryService;
            this.processDiagramGenerator = processDiagramGenerator;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityService;
        }

        //public ProcessInstanceControllerImpl(ProcessEngineWrapper processEngine, 
        //    IRepositoryService repositoryService, 
        //    ProcessDiagramGeneratorWrapper processDiagramGenerator, 
        //    ProcessInstanceResourceAssembler resourceAssembler, 
        //    SecurityPoliciesApplicationService securityService)
        //{
        //    this.processEngine = processEngine;
        //    this.repositoryService = repositoryService;
        //    this.processDiagramGenerator = processDiagramGenerator;
        //    this.resourceAssembler = resourceAssembler;
        //    this.securityService = securityService;
        //}

        [HttpGet]
        public virtual Task<PagedResources<ProcessInstanceResource>> GetProcessInstances(Pageable pageable)
        {
            return null;
            //return pagedResourcesAssembler.toResource(pageable, processEngine.getProcessInstances(pageable), resourceAssembler);
        }

        [HttpPost("start")]
        public virtual Task<ProcessInstance> Start([FromBody]StartProcessInstanceCmd cmd)
        {
            ProcessInstance instance = processEngine.startProcess(cmd);

            return System.Threading.Tasks.Task.FromResult<ProcessInstance>(instance);
        }

        [HttpGet("{processInstanceId}")]
        public virtual Task<Resource<ProcessInstance>> GetProcessInstanceById(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            //if (processInstance == null || !securityService.canRead(processInstance.ProcessDefinitionKey))
            //{
            //    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
            //}
            return System.Threading.Tasks.Task.FromResult<Resource<ProcessInstance>>(resourceAssembler.toResource(processInstance));
        }

        [HttpGet("{processInstanceId}/diagram")]
        public virtual Task<string> GetProcessDiagram(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            //if (processInstance == null || !securityService.canRead(processInstance.ProcessDefinitionKey))
            //{
            //    throw new ActivitiObjectNotFoundException("Unable to find process instance for the given id:'" + processInstanceId + "'");
            //}

            IList<string> activityIds = processEngine.getActiveActivityIds(processInstanceId);
            BpmnModel bpmnModel = repositoryService.getBpmnModel(processInstance.ProcessDefinitionId);
            byte[] data = processDiagramGenerator.generateDiagram(bpmnModel, activityIds, new List<string>());

            return System.Threading.Tasks.Task.FromResult<string>(Encoding.UTF8.GetString(data));
        }

        [HttpPost("signal")]
        public virtual Task<IActionResult> SendSignal([FromBody]SignalCmd cmd)
        {
            processEngine.signal(cmd);

            return System.Threading.Tasks.Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("{processInstanceId}/suspend")]
        public virtual Task<IActionResult> Suspend([FromQuery]string processInstanceId)
        {
            processEngine.suspend(new SuspendProcessInstanceCmd(processInstanceId));

            return System.Threading.Tasks.Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("{processInstanceId}/activate")]
        public virtual Task<IActionResult> Activate([FromQuery]string processInstanceId)
        {
            processEngine.activate(new ActivateProcessInstanceCmd(processInstanceId));

            return System.Threading.Tasks.Task.FromResult<IActionResult>(Ok());
        }

        [HttpDelete("{processInstanceId}")]
        public virtual System.Threading.Tasks.Task DeleteProcessInstance(string processInstanceId)
        {
            processEngine.deleteProcessInstance(processInstanceId);

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}