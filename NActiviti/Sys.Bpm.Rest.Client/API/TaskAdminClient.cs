using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using Sys.Workflow.services.api.commands;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class TaskAdminClient : ITaskAdminController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.ADMIN_TASK_ROUTER_V1;

        /// <inheritdoc />
        public TaskAdminClient(IHttpClientProxy httpProxy)
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

        public async Task<Resources<TaskModel>> GetAllTasks(Pageable pageable)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}", pageable).ConfigureAwait(false);
        }

        public async Task<TaskModel[]> ReassignTaskUser(ReassignTaskUserCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/reassign", cmd).ConfigureAwait(false);
        }
    }
}
