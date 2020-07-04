using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Hateoas;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_INS_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceControllerImpl : WorkflowController, IProcessInstanceController
    {
        private readonly ProcessEngineWrapper processEngineWrapper;

        private readonly IProcessEngine processEngine;

        private readonly IRepositoryService repositoryService;

        private readonly ProcessDiagramGeneratorWrapper processDiagramGenerator;

        private readonly ProcessInstanceResourceAssembler resourceAssembler;

        private readonly SecurityPoliciesApplicationService securityService;

        private readonly IRuntimeService runtimeService;

        private readonly PageableProcessInstanceRepositoryService pageableProcessInstanceService;

        private readonly ILogger<ProcessInstanceControllerImpl> logger;

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
            SecurityPoliciesApplicationService securityPoliciesApplicationService,
            ILoggerFactory loggerFactory)
        {
            this.processEngineWrapper = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.processEngine = engine;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityPoliciesApplicationService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
            this.logger = loggerFactory.CreateLogger<ProcessInstanceControllerImpl>();
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
        public async virtual Task<ProcessInstance[]> Start(StartProcessInstanceCmd[] cmds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                logger.LogInformation("开始调用工作流启动事件\r\n" + JsonConvert.SerializeObject(cmds));

                ProcessInstance[] instances = await processEngineWrapper.StartProcessAsync(cmds).ConfigureAwait(false);

                logger.LogInformation("调用工作流启动事件完成");

                return instances;
            }
            finally
            {
                sw.Stop();

                logger.LogInformation($"共计启动{cmds.Length}项任务,执行完成时间：{sw.ElapsedMilliseconds}");
            }
        }


        /// <inheritdoc />
        [HttpGet("{processInstanceId}")]
        public virtual Task<ProcessInstance> GetProcessInstanceById(string processInstanceId)
        {
            ProcessInstance processInstance = processEngineWrapper.GetProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
            }
            return Task.FromResult<ProcessInstance>(processInstance);
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
        public virtual Task<bool> SendSignal(SignalCmd cmd)
        {
            processEngineWrapper.Signal(cmd);

            return Task.FromResult(true);
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
        public virtual Task<bool> Terminate(TerminateProcessInstanceCmd[] cmds)
        {
            processEngineWrapper.TerminateProcessInstance(cmds);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        [HttpPost("activiti/{processDefifinitionId}/{activityId}/{businessKey?}")]
        public Task<ProcessInstance> StartByActiviti(string processDefinitionId, string businessKey, string activityId, [FromBody] IDictionary<string, object> variables)
        {
            this.processEngine.ManagementService.ExecuteCommand<IProcessInstance>(new Engine.Impl.Cmd.StartProcessInstanceByActivityCmd(processDefinitionId, businessKey, activityId, variables, securityService.User.TenantId, null));

            return null;
        }
    }
}