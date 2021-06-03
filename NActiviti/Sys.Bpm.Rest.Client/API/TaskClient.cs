using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Hateoas;
using Sys.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace Sys.Workflow.Rest.Client.API
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
        public async Task<Resources<TaskModel>> MyTasks(string userId, string businessKey = null)
        {
            return await httpProxy.GetAsync<Resources<TaskModel>>($"{serviceUrl}/{userId}/mytasks{(businessKey is object ? $"/{businessKey}" : "")}").ConfigureAwait(false);
        }


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
        public async Task<bool> CompleteTask(CompleteTaskCmd completeTaskCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/complete", completeTaskCmd).ConfigureAwait(false);
        }

        public async Task<CompleteTaskCmd[]> CompleteTask(CompleteTaskCmd[] cmds)
        {
            return await httpProxy.PostAsync<CompleteTaskCmd[]>($"{serviceUrl}/completes", cmds).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> Terminate(TerminateTaskCmd cmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/terminate", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTask(string taskId)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/{taskId}/remove").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TaskModel> CreateNewTask(CreateTaskCmd createTaskCmd)
        {
            return await httpProxy.PostAsync<TaskModel>($"{serviceUrl}/create", createTaskCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTask(UpdateTaskCmd updateTaskCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/update", updateTaskCmd).ConfigureAwait(false);
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
        public async Task<bool> Approvaled(ApprovaleTaskCmd cmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/approvaled", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> Reject(RejectTaskCmd cmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/reject", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> ReturnTo(ReturnToTaskCmd cmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/returnTo", cmd).ConfigureAwait(false);
        }
    }
}
