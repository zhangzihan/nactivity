using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Hateoas;
using Sys.Net.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sys.Workflow.Rest.Client.API
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
            return await httpProxy.PostAsync<Resources<Deployment>>($"{serviceUrl}/list", queryObj).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Deployment> Deploy(ProcessDefinitionDeployer deployer)
        {
            return await httpProxy.PostAsync<Deployment>($"{serviceUrl}", deployer).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ActionResult<BpmnModel>> GetBpmnModel(string id)
        {
            HttpResponseMessage response = await httpProxy.GetAsync<HttpResponseMessage>($"{serviceUrl}/{id}/bpmnmodel").ConfigureAwait(false);

            string data = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            BpmnModel model = JsonConvert.DeserializeObject<BpmnModel>(data, new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            ActionResult<BpmnModel> result = new ObjectResult(model);

            return result;
        }

        /// <inheritdoc />
        public async Task<string> GetProcessModel(string id)
        {
            HttpResponseMessage response = await httpProxy.GetAsync<HttpResponseMessage>($"{serviceUrl}/{id}/processmodel").ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string> GetProcessModel(DeploymentQuery queryObj)
        {
            HttpResponseMessage response = await httpProxy.PostAsync<HttpResponseMessage>("processmodel", queryObj).ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Resources<Deployment>> Latest(DeploymentQuery queryObj)
        {
            return await httpProxy.PostAsync<Resources<Deployment>>($"{serviceUrl}/latest", queryObj).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> Remove(string deployId)
        {
            return await httpProxy.GetAsync<bool>($"{serviceUrl}/{deployId}/remove").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Deployment> Save(ProcessDefinitionDeployer deployer)
        {
            return await httpProxy.PostAsync<Deployment>($"{serviceUrl}/save", deployer).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Deployment> Draft(string tenantId, string name)
        {
            return await httpProxy.GetAsync<Deployment>($"{serviceUrl}/{tenantId}/{name}/draft").ConfigureAwait(false);
        }
    }
}
