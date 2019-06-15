using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.activiti.services.api.commands;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Net;
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
        public async Task<Resources<TaskModel>> GetTasks(TaskQuery query)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}", query).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> GetTaskById(string taskId)
        {
            return await httpProxy.GetAsync<TaskModel>($"{serviceUrl}/{taskId}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> MyTasks(string userId)
        {
            return await httpProxy.GetAsync<Resources<TaskModel>>($"{serviceUrl}/{userId}/mytasks").ConfigureAwait(false);
        }

        /// <inheritdoc />
        //public async Task<Resources<TaskModel>> NextForm(string userId)
        //{
        //    return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}/{userId}/nextform");
        //}

        /// <inheritdoc />
        public async Task<TaskModel> ClaimTask(string taskId)
        {
            return await httpProxy.GetAsync<TaskModel>($"{serviceUrl}/{taskId}/claim").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> ReleaseTask(string taskId)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/{taskId}/release").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> ReleaseTask(ReleaseTaskCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/release", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> CompleteTask(CompleteTaskCmd completeTaskCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/complete", completeTaskCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> Terminate(TerminateTaskCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/terminate", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> DeleteTask(string taskId)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/{taskId}/remove").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> CreateNewTask(CreateTaskCmd createTaskCmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/create", createTaskCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> UpdateTask(UpdateTaskCmd updateTaskCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/update", updateTaskCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> CreateSubtask(CreateTaskCmd createSubtaskCmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/subtasks", createSubtaskCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel[]> AppendCountersign(AppendCountersignCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/append", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel[]> TransferTask(TransferTaskCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/transfer", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> GetSubtasks(string taskId)
        {
            return await httpProxy.GetAsync<Resources<TaskModel>>($"{serviceUrl}/{taskId}/subtasks").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> Approvaled(ApprovaleTaskCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/approvaled", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> Reject(RejectTaskCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/reject", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> ReturnTo(ReturnToTaskCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/returnTo", cmd).ConfigureAwait(false);
        }
    }
}
