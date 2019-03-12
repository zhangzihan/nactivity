using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.engine.runtime;
using org.activiti.image.exception;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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
    [Route(WorkflowConstants.PROC_INS_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceControllerImpl : ControllerBase, IProcessInstanceController
    {
        private ProcessEngineWrapper processEngine;

        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessInstanceResourceAssembler resourceAssembler;

        private readonly SecurityPoliciesApplicationService securityService;

        private readonly IRuntimeService runtimeService;

        private readonly PageableProcessInstanceRepositoryService pageableProcessInstanceService;

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
            ProcessInstanceResourceAssembler resourceAssembler,
            PageableProcessInstanceRepositoryService pageableProcessInstanceService,
            IProcessEngine engine,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.processEngine = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.processDiagramGenerator = processDiagramGenerator;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityPoliciesApplicationService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
        }

        [HttpPost]
        public virtual Task<Resources<ProcessInstance>> ProcessInstances(ProcessInstanceQuery query)
        {
            IPage<ProcessInstance> instances = new QueryProcessInstanceCmd().loadPage(this.runtimeService, this.pageableProcessInstanceService, query);

            IList<ProcessInstanceResource> resources = resourceAssembler.toResources(instances.getContent());

            return Task.FromResult<Resources<ProcessInstance>>(new Resources<ProcessInstance>(resources.Select(x => x.Content), instances.getTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize));
        }

        [HttpPost("start")]
        public virtual Task<ProcessInstance> Start([FromBody]StartProcessInstanceCmd cmd)
        {
            ProcessInstance instance = processEngine.startProcess(cmd);

            return Task.FromResult<ProcessInstance>(instance);
        }

        [HttpGet("{processInstanceId}")]
        public virtual Task<ProcessInstance> GetProcessInstanceById(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
            }
            return Task.FromResult<ProcessInstance>(processInstance);
        }

        [HttpGet("{processInstanceId}/diagram")]
        public virtual Task<string> GetProcessDiagram(string processInstanceId)
        {
            ProcessInstance processInstance = processEngine.getProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process instance for the given id:'" + processInstanceId + "'");
            }

            IList<string> activityIds = processEngine.getActiveActivityIds(processInstanceId);
            BpmnModel bpmnModel = repositoryService.getBpmnModel(processInstance.ProcessDefinitionId);
            byte[] data = processDiagramGenerator.generateDiagram(bpmnModel, activityIds, new List<string>());

            return Task.FromResult<string>(Encoding.UTF8.GetString(data));
        }

        [HttpPost("signal")]
        public virtual Task<IActionResult> SendSignal([FromBody]SignalCmd cmd)
        {
            processEngine.signal(cmd);

            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("{processInstanceId}/suspend")]
        public virtual Task<IActionResult> Suspend(string processInstanceId)
        {
            processEngine.suspend(new SuspendProcessInstanceCmd(processInstanceId));

            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("{processInstanceId}/activate")]
        public virtual Task<IActionResult> Activate(string processInstanceId)
        {
            processEngine.activate(new ActivateProcessInstanceCmd(processInstanceId));

            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("{processInstanceId}/terminate")]
        public virtual Task<IActionResult> Terminate(string processInstanceId, string reason)
        {
            processEngine.deleteProcessInstance(processInstanceId, reason);

            return Task.FromResult<IActionResult>(Ok());
        }
    }
}