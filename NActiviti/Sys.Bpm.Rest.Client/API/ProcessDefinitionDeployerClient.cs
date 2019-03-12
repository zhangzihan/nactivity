using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    class ProcessDefinitionDeployerClient : IProcessDefinitionDeployerController
    {
        private readonly HttpInvoker httpInvoker;

        private static readonly string serviceUrl = WorkflowConstants.PROC_DEP_ROUTER_V1;

        public ProcessDefinitionDeployerClient(HttpInvoker httpInvoker)
        {
            this.httpInvoker = httpInvoker;
        }

        public async Task<Resources<Deployment>> AllDeployments(DeploymentQuery queryObj)
        {
            return await httpInvoker.PostAsync<Resources<Deployment>>($"{serviceUrl}/list", queryObj);
        }

        public async Task<Deployment> Deploy(ProcessDefinitionDeployer deployer)
        {
            return await httpInvoker.PostAsync<Deployment>($"{serviceUrl}", deployer);
        }

        public async Task<BpmnModel> GetBpmnModel(string id)
        {
            return await httpInvoker.GetAsync<BpmnModel>($"{serviceUrl}/{id}/bpmnmodel");
        }

        public async Task<string> GetProcessModel(string id)
        {
            return await httpInvoker.GetAsync<string>($"{serviceUrl}/{id}/processmodel");
        }

        public async Task<Resources<Deployment>> Latest(DeploymentQuery queryObj)
        {
            return await httpInvoker.PostAsync<Resources<Deployment>>($"{serviceUrl}/latest", queryObj);
        }

        public async Task<IActionResult> Remove(string deployId)
        {
            return await httpInvoker.GetAsync<IActionResult>($"{serviceUrl}/{deployId}/remove");
        }

        public async Task<Deployment> Save(ProcessDefinitionDeployer deployer)
        {
            return await httpInvoker.PostAsync<Deployment>($"{serviceUrl}/save", deployer);
        }

        public async Task<Deployment> Draft(string tenantId, string name)
        {
            return await httpInvoker.GetAsync<Deployment>($"{serviceUrl}/{tenantId}/{name}/draft");
        }
    }
}
