using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.engine.task;
using org.activiti.services.api.commands;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Linq;
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
    [Route(WorkflowConstants.TASK_ROUTER_V1)]
    [ApiController]
    public class TaskControllerImpl : ControllerBase, ITaskController
    {

        private readonly ProcessEngineWrapper processEngine;

        private readonly TaskResourceAssembler taskResourceAssembler;

        private AuthenticationWrapper authenticationWrapper;

        //private readonly AlfrescoPagedResourcesAssembler<Task> pagedResourcesAssembler;

        private readonly TaskConverter taskConverter;

        private readonly ITaskService taskService;

        private readonly IProcessEngine engine;


        /// <inheritdoc />
        public TaskControllerImpl(ProcessEngineWrapper processEngine,
            IProcessEngine engine,
            TaskResourceAssembler taskResourceAssembler,
            AuthenticationWrapper authenticationWrapper,
            TaskConverter taskConverter)
        {
            this.engine = engine;
            this.taskService = engine.TaskService;
            this.authenticationWrapper = authenticationWrapper;
            this.processEngine = processEngine;
            this.taskResourceAssembler = taskResourceAssembler;
            this.taskConverter = taskConverter;
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
        public virtual Task<ActionResult> CompleteTask([FromBody]CompleteTaskCmd completeTaskCmd)
        {
            processEngine.CompleteTask(completeTaskCmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("approvaled")]
        public Task<ActionResult> Approvaled(ApprovaleTaskCmd cmd)
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

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("reject")]
        public Task<ActionResult> Reject(RejectTaskCmd cmd)
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

            return Task.FromResult<ActionResult>(Ok());
        }

        [HttpPost("returnTo")]
        public Task<ActionResult> ReturnTo(ReturnToTaskCmd cmd)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        [HttpPost("terminate")]
        public virtual Task<ActionResult> Terminate(TerminateTaskCmd cmd)
        {
            processEngine.TerminateTask(cmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("{taskId}/remove")]
        public virtual Task<ActionResult> DeleteTask(string taskId)
        {
            processEngine.DeleteTask(taskId);

            return Task.FromResult<ActionResult>(Ok());
        }


        /// <inheritdoc />
        [HttpPost("create")]
        public virtual Task<TaskModel> CreateNewTask([FromBody]CreateTaskCmd createTaskCmd)
        {
            return Task.FromResult(taskResourceAssembler.ToResource(processEngine.CreateNewTask(createTaskCmd)).Content);
        }


        /// <inheritdoc />
        [HttpPost("update")]
        public virtual Task<ActionResult> UpdateTask(UpdateTaskCmd updateTaskCmd)
        {
            processEngine.UpdateTask(updateTaskCmd);

            return Task.FromResult<ActionResult>(Ok());
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