using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessDefinitionClient : IProcessDefinitionController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_DEF_ROUTER_V1;

        /// <inheritdoc />
        public ProcessDefinitionClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<BpmnModel> GetBpmnModel(string id)
        {
            return await httpProxy.GetAsync<BpmnModel>($"{serviceUrl}/{id}/processmodel");
        }

        /// <inheritdoc />
        public async Task<ProcessDefinition> GetProcessDefinition(string id)
        {
            return await httpProxy.GetAsync<ProcessDefinition>($"{serviceUrl}/{id}");
        }

        /// <inheritdoc />
        public Task<string> GetProcessDiagram(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<string> GetProcessModel(string id)
        {
            return await httpProxy.GetAsync<string>($"{serviceUrl}/{id}/processmodel");
        }

        /// <inheritdoc />
        public async Task<Resources<ProcessDefinition>> LatestProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            return await httpProxy.PostAsync<Resources<ProcessDefinition>>($"{serviceUrl}/latest", new JsonContent(queryObj));
        }

        /// <inheritdoc />
        public async Task<Resources<ProcessDefinition>> ProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            return await httpProxy.PostAsync<Resources<ProcessDefinition>>($"{serviceUrl}/list", new JsonContent(queryObj));
        }
    }
}
