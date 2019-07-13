using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Workflow.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessInstanceVariableClient : IProcessInstanceVariableController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_INS_VAR_ROUTER_V1;

        /// <inheritdoc />
        public ProcessInstanceVariableClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<Resources<ProcessInstanceVariable>> GetVariables(string processInstanceId)
        {
            return await httpProxy.GetAsync<Resources<ProcessInstanceVariable>>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<ProcessInstanceVariable>> GetVariablesLocal(string processInstanceId)
        {
            return await httpProxy.GetAsync<Resources<ProcessInstanceVariable>>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}/local").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> SetVariables(SetProcessVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl.Replace("{processInstanceId}", setTaskVariablesCmd.ProcessId)}", setTaskVariablesCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveVariables(RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl.Replace("{processInstanceId}", removeProcessVariablesCmd.ProcessId)}/remove", removeProcessVariablesCmd).ConfigureAwait(false);
        }

        public async Task<ProcessInstanceVariable> GetVariable(string processInstanceId, string variableName)
        {
            return await httpProxy.GetAsync<ProcessInstanceVariable>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}/{variableName}").ConfigureAwait(false);
        }
    }
}
