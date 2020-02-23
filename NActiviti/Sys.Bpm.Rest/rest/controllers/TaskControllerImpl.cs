using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Sys.Workflow.Exceptions;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

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
    [Route(WorkflowConstants.TASK_ROUTER_V1)]
    [ApiController]
    public class TaskControllerImpl : WorkflowController, ITaskController
    {
        private readonly ProcessEngineWrapper processEngine;

        private readonly TaskResourceAssembler taskResourceAssembler;

        private AuthenticationWrapper authenticationWrapper;

        //private readonly AlfrescoPagedResourcesAssembler<Task> pagedResourcesAssembler;

        private readonly TaskConverter taskConverter;

        private readonly ITaskService taskService;

        private readonly IProcessEngine engine;

        private readonly ILogger<TaskControllerImpl> logger;

        /// <inheritdoc />
        public TaskControllerImpl(ProcessEngineWrapper processEngine,
            IProcessEngine engine,
            TaskResourceAssembler taskResourceAssembler,
            AuthenticationWrapper authenticationWrapper,
            TaskConverter taskConverter,
            ILoggerFactory loggerFactory)
        {
            this.engine = engine;
            this.taskService = engine.TaskService;
            this.authenticationWrapper = authenticationWrapper;
            this.processEngine = processEngine;
            this.taskResourceAssembler = taskResourceAssembler;
            this.taskConverter = taskConverter;
            this.logger = loggerFactory.CreateLogger<TaskControllerImpl>();
        }

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}


        /// <inheritdoc />
        [HttpPost]
        public virtual Task<Resources<TaskModel>> GetTasks(TaskQuery query)
        {
            IPage<TaskModel> page = processEngine.GetTasks(query.Pageable);

            return Task.FromResult(new Resources<TaskModel>(page.GetContent(), page.GetTotalItems(), query.Pageable));
        }


        /// <inheritdoc />
        [HttpGet("{userId}/mytasks")]
        public Task<Resources<TaskModel>> MyTasks(string userId)
        {
            IList<ITask> tasks = this.taskService.GetMyTasks(userId);

            IList<TaskResource> resources = this.taskResourceAssembler.ToResources(taskConverter.From(tasks.OrderByDescending(x => x.CreateTime)));

            return Task.FromResult(new Resources<TaskModel>(resources.Select(x => x.Content), tasks.Count, 0, 0));
        }

        /// <inheritdoc />
        [HttpGet("{taskId}")]
        public virtual Task<TaskModel> GetTaskById(string taskId)
        {
            TaskModel task = processEngine.GetTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            return Task.FromResult(taskResourceAssembler.ToResource(task).Content);
        }


        /// <inheritdoc />
        [HttpPost("{taskId}/claim")]
        public virtual Task<TaskModel> ClaimTask(string taskId)
        {
            string assignee = authenticationWrapper.AuthenticatedUser.Id;
            if (assignee is null)
            {
                throw new System.InvalidOperationException("Assignee must be resolved from the Identity/Security Layer");
            }

            var res = taskResourceAssembler.ToResource(processEngine.ClaimTask(new ClaimTaskCmd(taskId, assignee)));

            return Task.FromResult(res.Content);
        }


        /// <inheritdoc />

        [HttpPost("{taskId}/release")]
        public virtual Task<TaskModel> ReleaseTask(string taskId)
        {
            TaskModel model = processEngine.ReleaseTask(new ReleaseTaskCmd(taskId, "任务退回"));

            return Task.FromResult(taskResourceAssembler.ToResource(model).Content);
        }


        /// <inheritdoc />

        [HttpPost("release")]
        public virtual Task<TaskModel> ReleaseTask(ReleaseTaskCmd cmd)
        {
            TaskModel model = processEngine.ReleaseTask(cmd);

            return Task.FromResult(taskResourceAssembler.ToResource(model).Content);
        }

        /// <inheritdoc />
        [HttpPost("complete")]
        public virtual Task<bool> CompleteTask([FromBody]CompleteTaskCmd completeTaskCmd)
        {
            processEngine.CompleteTask(completeTaskCmd);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        [HttpPost("completes")]
        public virtual Task<CompleteTaskCmd[]> CompleteTask([FromBody]CompleteTaskCmd[] cmds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            logger.LogInformation("开始调用工作流完成事件\r\n" + JsonConvert.SerializeObject(cmds));

            List<CompleteTaskCmd> errors = new List<CompleteTaskCmd>();
            foreach (var cmd in cmds)
            {
                try
                {
                    logger.LogInformation("开始调用工作流完成事件\r\n" + JsonConvert.SerializeObject(cmd));

                    processEngine.CompleteTask(cmd);

                    logger.LogInformation("调用工作流完成事件完成");
                }
                catch (Exception ex)
                {
                    cmd.ErrorMessage = ex.Message;
                    cmd.Exception = ex.GetType().Name;
                    errors.Add(cmd);
                }
            }

            sw.Stop();
            logger.LogInformation($"共计执行{cmds.Length}项任务,执行完成时间：{sw.ElapsedMilliseconds}");

            return Task.FromResult(errors.ToArray());
        }

        /// <inheritdoc />
        [HttpPost("approvaled")]
        public Task<bool> Approvaled(ApprovaleTaskCmd cmd)
        {
            processEngine.CompleteApprovalTask(new CompleteTaskCmd
            {
                TaskId = cmd.TaskId,
                LocalScope = true,
                OutputVariables = new WorkflowVariable(cmd.Variables)
                {
                    [WorkflowVariable.GLOBAL_APPROVALED_VARIABLE] = true,
                    [WorkflowVariable.GLOBAL_APPROVALED_COMMENTS] = string.IsNullOrWhiteSpace(cmd.Comments) ? "同意" : cmd.Comments,
                }
            });

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        [HttpPost("reject")]
        public Task<bool> Reject(RejectTaskCmd cmd)
        {
            processEngine.CompleteApprovalTask(new CompleteTaskCmd
            {
                TaskId = cmd.TaskId,
                LocalScope = true,
                OutputVariables = new WorkflowVariable(cmd.Variables)
                {
                    [WorkflowVariable.GLOBAL_APPROVALED_VARIABLE] = false,
                    [WorkflowVariable.GLOBAL_APPROVALED_COMMENTS] = string.IsNullOrWhiteSpace(cmd.RejectReason) ? "拒绝" : cmd.RejectReason,
                }
            });

            return Task.FromResult(true);
        }

        [HttpPost("returnto")]
        public Task<bool> ReturnTo(ReturnToTaskCmd cmd)
        {
            processEngine.ReturnTo(cmd);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        [HttpPost("terminate")]
        public virtual Task<bool> Terminate(TerminateTaskCmd cmd)
        {
            processEngine.TerminateTask(cmd);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        [HttpPost("{taskId}/remove")]
        public virtual Task<bool> DeleteTask(string taskId)
        {
            processEngine.DeleteTask(taskId);

            return Task.FromResult(true);
        }


        /// <inheritdoc />
        [HttpPost("create")]
        public virtual Task<TaskModel> CreateNewTask([FromBody]CreateTaskCmd createTaskCmd)
        {
            return Task.FromResult(taskResourceAssembler.ToResource(processEngine.CreateNewTask(createTaskCmd)).Content);
        }


        /// <inheritdoc />
        [HttpPost("update")]
        public virtual Task<bool> UpdateTask(UpdateTaskCmd updateTaskCmd)
        {
            processEngine.UpdateTask(updateTaskCmd);

            return Task.FromResult(true);
        }


        /// <inheritdoc />
        [HttpPost("subtask")]
        public virtual Task<TaskModel> CreateSubtask([FromBody]CreateTaskCmd createSubtaskCmd)
        {
            TaskModel task = processEngine.CreateNewSubtask(createSubtaskCmd);

            return Task.FromResult(task);
        }


        /// <inheritdoc />
        [HttpPost("transfer")]
        public virtual Task<TaskModel[]> TransferTask(TransferTaskCmd cmd)
        {
            TaskModel[] task = processEngine.TransferTask(cmd);

            return Task.FromResult(task);
        }

        /// <inheritdoc />
        [HttpPost("append")]
        public virtual Task<TaskModel[]> AppendCountersign(AppendCountersignCmd cmd)
        {
            TaskModel[] tasks = processEngine.AppendCountersign(cmd);

            return Task.FromResult(tasks);
        }

        /// <inheritdoc />
        [HttpGet("{taskId}/subtasks")]
        public virtual Task<Resources<TaskModel>> GetSubtasks(string taskId)
        {
            IList<ITask> tasks = processEngine.GetSubtasks(taskId);

            IEnumerable<TaskModel> models = taskConverter.From(tasks);

            return Task.FromResult(new Resources<TaskModel>(models, models.Count(), 1, models.Count()));
        }

        /// <inheritdoc />
        public virtual AuthenticationWrapper AuthenticationWrapper
        {
            get
            {
                return authenticationWrapper;
            }
            set
            {
                this.authenticationWrapper = value;
            }
        }

    }

}