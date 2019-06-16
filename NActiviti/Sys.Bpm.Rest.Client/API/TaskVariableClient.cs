using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.rest.api;
using Sys.Workflow.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
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
        public async Task<ActionResult> SetVariables(string taskId, SetTaskVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl.Replace("{taskId}", taskId)}", setTaskVariablesCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> SetVariablesLocal(string taskId, SetTaskVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl.Replace("{taskId}", taskId)}/local", setTaskVariablesCmd).ConfigureAwait(false);
        }
    }
}
