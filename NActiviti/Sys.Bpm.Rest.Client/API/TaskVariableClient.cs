using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Workflow.Rest.Client.API
{
    /// <inheritdoc />
    class TaskVariableClient : ITaskVariableController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.TASK_VAR_ROUTER_V1;

        /// <inheritdoc />
        public TaskVariableClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        public async Task<TaskVariableResource> GetVariable(string taskId, string variableName)
        {
            return await httpProxy.GetAsync<TaskVariableResource>($"{serviceUrl.Replace("{taskId}", taskId)}/{variableName}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<TaskVariableResource>> GetVariables(string taskId)
        {
            return await httpProxy.GetAsync<Resources<TaskVariableResource>>($"{serviceUrl.Replace("{taskId}", taskId)}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<TaskVariableResource>> GetVariablesLocal(string taskId)
        {
            return await httpProxy.GetAsync<Resources<TaskVariableResource>>($"{serviceUrl.Replace("{taskId}", taskId)}/local").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> SetVariables(string taskId, SetTaskVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl.Replace("{taskId}", taskId)}", setTaskVariablesCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> SetVariablesLocal(string taskId, SetTaskVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl.Replace("{taskId}", taskId)}/local", setTaskVariablesCmd).ConfigureAwait(false);
        }
    }
}
