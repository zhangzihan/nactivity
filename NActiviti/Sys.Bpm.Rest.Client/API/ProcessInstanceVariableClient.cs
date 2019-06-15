using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
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
        public async Task<ActionResult> SetVariables(string processInstanceId, SetProcessVariablesCmd setTaskVariablesCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}", setTaskVariablesCmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult> RemoveVariables(string processInstanceId, RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}/remove", removeProcessVariablesCmd).ConfigureAwait(false);
        }

        public async Task<ProcessInstanceVariable> GetVariable(string processInstanceId, string variableName)
        {
            return await httpProxy.GetAsync<ProcessInstanceVariable>($"{serviceUrl.Replace("{processInstanceId}", processInstanceId)}/{variableName}").ConfigureAwait(false);
        }
    }
}
