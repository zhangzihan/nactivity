using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
using org.springframework.hateoas;
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
    [Route(WorkflowConstants.TASK_ROUTER_V1)]
    [ApiController]
    public class TaskControllerImpl : ControllerBase, ITaskController
    {

        private ProcessEngineWrapper processEngine;

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
        public virtual Task<Resources<TaskModel>> getTasks(TaskQuery query)
        {
            IPage<TaskModel> page = processEngine.getTasks(query.Pageable);

            return Task.FromResult(new Resources<TaskModel>(page.getContent(), page.getTotalItems(), query.Pageable));
        }


        /// <inheritdoc />
        [HttpGet("{userId}/mytasks")]
        public Task<Resources<TaskModel>> MyTasks(string userId)
        {
            List<ITask> tasks = this.taskService.createTaskQuery().taskAssignee(userId).list().ToList();

            tasks.AddRange(this.taskService.createTaskQuery().taskCandidateUser(userId).list());

            IList<TaskResource> resources = this.taskResourceAssembler.toResources(taskConverter.from(tasks));

            return Task.FromResult(new Resources<TaskModel>(resources.Select(x => x.Content), tasks.Count, 0, 0));
        }


        /// <inheritdoc />
        [HttpGet("{userId}/nextform")]
        public Task<Resources<TaskModel>> NextForm(string userId)
        {
            List<ITask> tasks = this.taskService.createTaskQuery().taskAssignee(userId).list().ToList();

            tasks.AddRange(this.taskService.createTaskQuery().taskCandidateUser(userId).list());

            IList<TaskResource> resources = this.taskResourceAssembler.toResources(taskConverter.from(tasks));

            return Task.FromResult(new Resources<TaskModel>(resources.Select(x => x.Content), tasks.Count, 0, 0));
        }


        /// <inheritdoc />
        [HttpGet("{taskId}")]
        public virtual Task<TaskModel> getTaskById(string taskId)
        {
            TaskModel task = processEngine.getTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            return Task.FromResult(taskResourceAssembler.toResource(task).Content);
        }


        /// <inheritdoc />
        [HttpPost("{taskId}/claim")]
        public virtual Task<TaskModel> claimTask(string taskId)
        {
            string assignee = authenticationWrapper.AuthenticatedUser.Id;
            if (string.ReferenceEquals(assignee, null))
            {
                throw new System.InvalidOperationException("Assignee must be resolved from the Identity/Security Layer");
            }

            var res = taskResourceAssembler.toResource(processEngine.claimTask(new ClaimTaskCmd(taskId, assignee)));

            return Task.FromResult(res.Content);
        }


        /// <inheritdoc />

        [HttpPost("{taskId}/release")]
        public virtual Task<TaskModel> releaseTask(string taskId)
        {

            return Task.FromResult(taskResourceAssembler.toResource(processEngine.releaseTask(new ReleaseTaskCmd(taskId))).Content);
        }


        /// <inheritdoc />
        [HttpPost("{taskId}/complete")]
        public virtual Task<IActionResult> completeTask(string taskId, [FromBody]CompleteTaskCmd completeTaskCmd)
        {
            processEngine.completeTask(completeTaskCmd);

            return Task.FromResult<IActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("terminate")]
        public virtual Task<IActionResult> terminate(TerminateTaskCmd cmd)
        {
            processEngine.terminateTask(cmd);

            return Task.FromResult<IActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("{taskId}/remove")]
        public virtual Task<IActionResult> deleteTask(string taskId)
        {
            processEngine.deleteTask(taskId);

            return Task.FromResult<IActionResult>(Ok());
        }


        /// <inheritdoc />
        [HttpPost("create")]
        public virtual Task<TaskModel> createNewTask([FromBody]CreateTaskCmd createTaskCmd)
        {
            return Task.FromResult(taskResourceAssembler.toResource(processEngine.createNewTask(createTaskCmd)).Content);
        }


        /// <inheritdoc />
        [HttpPost("{taskId}/update")]
        public virtual Task<IActionResult> updateTask(string taskId, UpdateTaskCmd updateTaskCmd)
        {
            processEngine.updateTask(taskId, updateTaskCmd);

            return Task.FromResult<IActionResult>(Ok());
        }


        /// <inheritdoc />
        [HttpPost("{taskId}/subtask")]
        public virtual Task<TaskModel> createSubtask(string taskId, [FromBody]CreateTaskCmd createSubtaskCmd)
        {
            TaskModel task = processEngine.createNewSubtask(taskId, createSubtaskCmd);

            return Task.FromResult(task);
        }


        /// <inheritdoc />
        [HttpPost("transfer")]
        public virtual Task<TaskModel[]> transferTask(TransferTaskCmd cmd)
        {
            TaskModel[] task = processEngine.transferTask(cmd);

            return Task.FromResult(task);
        }

        /// <inheritdoc />
        [HttpPost("append")]
        public virtual Task<TaskModel[]> appendCountersign(AppendCountersignCmd cmd)
        {
            TaskModel[] tasks = processEngine.appendCountersign(cmd);

            return Task.FromResult(tasks);
        }

        /// <inheritdoc />
        [HttpGet("{taskId}/subtasks")]
        public virtual Task<Resources<TaskModel>> getSubtasks(string taskId)
        {
            IList<ITask> tasks = processEngine.getSubtasks(taskId);

            IList<TaskModel> models = taskConverter.from(tasks);

            return Task.FromResult(new Resources<TaskModel>(models, models.Count, 1, models.Count));
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