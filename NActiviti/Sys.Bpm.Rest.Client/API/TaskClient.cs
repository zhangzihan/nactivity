using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class TaskClient : ITaskController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.TASK_ROUTER_V1;

        /// <inheritdoc />
        public TaskClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> getTasks(TaskQuery query)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}", query);
        }

        /// <inheritdoc />
        public async Task<TaskModel> getTaskById(string taskId)
        {
            return await httpProxy.GetAsync<TaskModel>($"{serviceUrl}/{taskId}");
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> MyTasks(string userId)
        {
            return await httpProxy.GetAsync<Resources<TaskModel>>($"{serviceUrl}/{userId}/mytasks");
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> NextForm(string userId)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}/{userId}/nextform");
        }

        /// <inheritdoc />
        public async Task<TaskModel> claimTask(string taskId)
        {
            return await httpProxy.GetAsync<TaskModel>($"{serviceUrl}/{taskId}/claim");
        }

        /// <inheritdoc />
        public async Task<TaskModel> releaseTask(string taskId)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/{taskId}/release");
        }

        /// <inheritdoc />
        public async Task<IActionResult> completeTask(string taskId, CompleteTaskCmd completeTaskCmd)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/{taskId}/complete", completeTaskCmd);
        }

        /// <inheritdoc />
        public async Task<IActionResult> terminate(TerminateTaskCmd cmd)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/terminate", cmd);
        }

        /// <inheritdoc />
        public async Task<IActionResult> deleteTask(string taskId)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/{taskId}/remove");
        }

        /// <inheritdoc />
        public async Task<TaskModel> createNewTask(CreateTaskCmd createTaskCmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/create", createTaskCmd);
        }

        /// <inheritdoc />
        public async Task<IActionResult> updateTask(string taskId, UpdateTaskCmd updateTaskCmd)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/{taskId}/update", updateTaskCmd);
        }

        /// <inheritdoc />
        public async Task<TaskModel> createSubtask(string taskId, CreateTaskCmd createSubtaskCmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/{taskId}/subtasks", createSubtaskCmd);
        }

        /// <inheritdoc />
        public async Task<TaskModel[]> appendCountersign(AppendCountersignCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/append", cmd);
        }

        /// <inheritdoc />
        public async Task<TaskModel[]> transferTask(TransferTaskCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/transfer", cmd);
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> getSubtasks(string taskId)
        {
            return await httpProxy.GetAsync<Resources<TaskModel>>($"{serviceUrl}/{taskId}/subtasks");
        }
    }
}
