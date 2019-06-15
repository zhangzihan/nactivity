using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
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
        public async Task<ActionResult> SendSignal(SignalCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/signal", cmd).ConfigureAwait(false);
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
        public async Task<ActionResult> Terminate(TerminateProcessInstanceCmd cmd)
        {
            return await httpProxy.PostAsync<ActionResult>($"{serviceUrl}/terminate", cmd).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<ProcessInstance> StartByActiviti(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> variables)
        {
            throw new System.NotImplementedException();
        }
    }
}
