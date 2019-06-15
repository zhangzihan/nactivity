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
using Sys.Bpm.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_INS_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceControllerImpl : ControllerBase, IProcessInstanceController
    {
        private readonly ProcessEngineWrapper processEngineWrapper;

        private readonly IProcessEngine processEngine;

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


        /// <inheritdoc />
        public ProcessInstanceControllerImpl(ProcessEngineWrapper processEngine,
            ProcessInstanceResourceAssembler resourceAssembler,
            PageableProcessInstanceRepositoryService pageableProcessInstanceService,
            IProcessEngine engine,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.processEngineWrapper = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.processEngine = engine;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityPoliciesApplicationService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
        }


        /// <inheritdoc />
        [HttpPost]
        public virtual Task<Resources<ProcessInstance>> ProcessInstances(ProcessInstanceQuery query)
        {
            IPage<ProcessInstance> instances = new QueryProcessInstanceCmd().LoadPage(this.runtimeService, this.pageableProcessInstanceService, query);

            IList<ProcessInstanceResource> resources = resourceAssembler.ToResources(instances.GetContent());

            return Task.FromResult<Resources<ProcessInstance>>(new Resources<ProcessInstance>(resources.Select(x => x.Content), instances.GetTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize));
        }


        /// <inheritdoc />
        [HttpPost("start")]
        public virtual Task<ProcessInstance[]> Start(StartProcessInstanceCmd[] cmds)
        {
            try
            {
                ProcessInstance[] instances = processEngineWrapper.StartProcess(cmds);

                return Task.FromResult<ProcessInstance[]>(instances);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <inheritdoc />
        [HttpGet("{processInstanceId}")]
        public virtual Task<ProcessInstance> GetProcessInstanceById(string processInstanceId)
        {
            try
            {
                ProcessInstance processInstance = processEngineWrapper.GetProcessInstanceById(processInstanceId);
                if (processInstance == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
                }
                return Task.FromResult<ProcessInstance>(processInstance);
            }
            catch (ActivitiObjectNotFoundException ex)
            {
                throw new Http400Exception(new Http400
                {
                    Code = "activitiObjectNotFound",
                    Message = "数据不存在",
                    Target = this.GetType().Name,
                }, ex);
            }
        }


        /// <inheritdoc />
        [HttpGet("{processInstanceId}/diagram")]
        public virtual Task<string> GetProcessDiagram(string processInstanceId)
        {
            ProcessInstance processInstance = processEngineWrapper.GetProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process instance for the given id:'" + processInstanceId + "'");
            }

            IList<string> activityIds = processEngineWrapper.GetActiveActivityIds(processInstanceId);
            BpmnModel bpmnModel = repositoryService.GetBpmnModel(processInstance.ProcessDefinitionId);
            byte[] data = processDiagramGenerator.GenerateDiagram(bpmnModel, activityIds, new List<string>());

            return Task.FromResult<string>(Encoding.UTF8.GetString(data));
        }


        /// <inheritdoc />
        [HttpPost("signal")]
        public virtual Task<ActionResult> SendSignal(SignalCmd cmd)
        {
            processEngineWrapper.Signal(cmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />

        [HttpGet("{processInstanceId}/suspend")]
        public virtual Task<ProcessInstance> Suspend(string processInstanceId)
        {
            ProcessInstance instance = processEngineWrapper.Suspend(new SuspendProcessInstanceCmd(processInstanceId));

            return Task.FromResult(instance);
        }


        /// <inheritdoc />
        [HttpGet("{processInstanceId}/activate")]
        public virtual Task<ProcessInstance> Activate(string processInstanceId)
        {
            ProcessInstance instance = processEngineWrapper.Activate(new ActivateProcessInstanceCmd(processInstanceId));

            return Task.FromResult(instance);
        }


        /// <inheritdoc />
        [HttpPost("terminate")]
        public virtual Task<ActionResult> Terminate(TerminateProcessInstanceCmd cmd)
        {
            processEngineWrapper.TerminateProcessInstance(cmd.ProcessInstanceId, cmd.Reason);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        public Task<ProcessInstance> StartByActiviti(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> variables)
        {
            this.processEngine.ManagementService.ExecuteCommand<IProcessInstance>(new engine.impl.cmd.StartProcessInstanceByActivityCmd(processDefinitionId, businessKey, activityId, variables, securityService.User.TenantId, null));

            return null;
        }
    }
}