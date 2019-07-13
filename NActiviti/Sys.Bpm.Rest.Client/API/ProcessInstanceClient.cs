using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Hateoas;
using Sys.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessInstanceClient : IProcessInstanceController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_INS_ROUTER_V1;

        /// <inheritdoc />
        public ProcessInstanceClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<Resources<ProcessInstance>> ProcessInstances(ProcessInstanceQuery query)
        {
            return await httpProxy.PostAsync<Resources<ProcessInstance>>($"{serviceUrl}", query).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ProcessInstance[]> Start(StartProcessInstanceCmd[] cmds)
        {
            ProcessInstance[] insts = await httpProxy.PostAsync<ProcessInstance[]>($"{serviceUrl}/start", cmds).ConfigureAwait(false);

            return insts;
        }

        /// <inheritdoc />
        public async Task<ProcessInstance> GetProcessInstanceById(string processInstanceId)
        {
            return await httpProxy.GetAsync<ProcessInstance>($"{serviceUrl}/{processInstanceId}").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string> GetProcessDiagram(string processInstanceId)
        {
            return await httpProxy.GetAsync<string>($"{serviceUrl}/{processInstanceId}/diagram").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> SendSignal(SignalCmd cmd)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/signal", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ProcessInstance> Suspend(string processInstanceId)
        {
            return await httpProxy.GetAsync<ProcessInstance>($"{serviceUrl}/{processInstanceId}/suspend").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ProcessInstance> Activate(string processInstanceId)
        {
            return await httpProxy.GetAsync<ProcessInstance>($"{serviceUrl}/{processInstanceId}/activate").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> Terminate(TerminateProcessInstanceCmd[] cmds)
        {
            return await httpProxy.PostAsync<bool>($"{serviceUrl}/terminate", cmds).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<ProcessInstance> StartByActiviti(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> variables)
        {
            throw new System.NotImplementedException();
        }
    }
}
