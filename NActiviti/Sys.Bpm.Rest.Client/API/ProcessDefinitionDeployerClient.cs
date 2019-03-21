using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessDefinitionDeployerClient : IProcessDefinitionDeployerController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_DEP_ROUTER_V1;

        /// <inheritdoc />
        public ProcessDefinitionDeployerClient(IHttpClientProxy httpInvoker)
        {
            this.httpProxy = httpInvoker;
        }

        /// <inheritdoc />
        public async Task<Resources<Deployment>> AllDeployments(DeploymentQuery queryObj)
        {
            return await httpProxy.PostAsync<Resources<Deployment>>($"{serviceUrl}/list", queryObj);
        }

        /// <inheritdoc />
        public async Task<Deployment> Deploy(ProcessDefinitionDeployer deployer)
        {
            return await httpProxy.PostAsync<Deployment>($"{serviceUrl}", deployer);
        }

        /// <inheritdoc />
        public async Task<BpmnModel> GetBpmnModel(string id)
        {
            return await httpProxy.GetAsync<BpmnModel>($"{serviceUrl}/{id}/bpmnmodel");
        }

        /// <inheritdoc />
        public async Task<string> GetProcessModel(string id)
        {
            return await httpProxy.GetAsync<string>($"{serviceUrl}/{id}/processmodel");
        }

        /// <inheritdoc />
        public async Task<Resources<Deployment>> Latest(DeploymentQuery queryObj)
        {
            return await httpProxy.PostAsync<Resources<Deployment>>($"{serviceUrl}/latest", queryObj);
        }

        /// <inheritdoc />
        public async Task<IActionResult> Remove(string deployId)
        {
            return await httpProxy.GetAsync<IActionResult>($"{serviceUrl}/{deployId}/remove");
        }

        /// <inheritdoc />
        public async Task<Deployment> Save(ProcessDefinitionDeployer deployer)
        {
            return await httpProxy.PostAsync<Deployment>($"{serviceUrl}/save", deployer);
        }

        /// <inheritdoc />
        public async Task<Deployment> Draft(string tenantId, string name)
        {
            return await httpProxy.GetAsync<Deployment>($"{serviceUrl}/{tenantId}/{name}/draft");
        }
    }
}
