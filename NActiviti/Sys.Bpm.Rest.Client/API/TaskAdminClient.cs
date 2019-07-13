using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
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
    class TaskAdminClient : ITaskAdminController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.ADMIN_TASK_ROUTER_V1;

        /// <inheritdoc />
        public TaskAdminClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        public async Task<Resources<TaskModel>> GetAllTasks(TaskQuery query)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}", query).ConfigureAwait(false);
        }

        public async Task<TaskModel[]> ReassignTaskUser(ReassignTaskUserCmd cmd)
        {
            return await httpProxy.PostAsync<TaskModel[]>($"{serviceUrl}/reassign", cmd).ConfigureAwait(false);
        }
    }
}
