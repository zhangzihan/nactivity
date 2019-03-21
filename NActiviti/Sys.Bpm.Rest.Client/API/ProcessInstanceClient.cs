using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
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
            return await httpProxy.PostAsync<Resources<ProcessInstance>>($"{serviceUrl}");
        }

        /// <inheritdoc />
        public async Task<ProcessInstance> Start(StartProcessInstanceCmd cmd)
        {
            return await httpProxy.PostAsync<ProcessInstance>($"{serviceUrl}/start", cmd);
        }

        /// <inheritdoc />
        public async Task<ProcessInstance> GetProcessInstanceById(string processInstanceId)
        {
            return await httpProxy.GetAsync<ProcessInstance>($"{serviceUrl}/{processInstanceId}");
        }

        /// <inheritdoc />
        public async Task<string> GetProcessDiagram(string processInstanceId)
        {
            return await httpProxy.GetAsync<string>($"{serviceUrl}/{processInstanceId}/diagram");
        }

        /// <inheritdoc />
        public async Task<IActionResult> SendSignal(SignalCmd cmd)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/signal", cmd);
        }

        /// <inheritdoc />
        public async Task<IActionResult> Suspend(string processInstanceId)
        {
            return await httpProxy.GetAsync<IActionResult>($"{serviceUrl}/{processInstanceId}/suspend");
        }

        /// <inheritdoc />
        public async Task<IActionResult> Activate(string processInstanceId)
        {
            return await httpProxy.GetAsync<IActionResult>($"{serviceUrl}/{processInstanceId}/activate");
        }

        /// <inheritdoc />
        public async Task<IActionResult> Terminate(TerminateProcessInstanceCmd cmd)
        {
            return await httpProxy.PostAsync<IActionResult>($"{serviceUrl}/terminate");
        }
    }
}
