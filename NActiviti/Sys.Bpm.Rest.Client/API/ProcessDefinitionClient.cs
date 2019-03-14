using Newtonsoft.Json;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Bpm.api.http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    class ProcessDefinitionClient : IProcessDefinitionController
    {
        private readonly IHttpInvoker httpInvoker;

        private static readonly string serviceUrl = WorkflowConstants.PROC_DEF_ROUTER_V1;

        public ProcessDefinitionClient(IHttpInvoker httpInvoker)
        {
            this.httpInvoker = httpInvoker;
        }

        public async Task<BpmnModel> GetBpmnModel(string id)
        {
            return await httpInvoker.GetAsync<BpmnModel>($"{serviceUrl}/{id}/processmodel");
        }

        public async Task<ProcessDefinition> GetProcessDefinition(string id)
        {
            return await httpInvoker.GetAsync<ProcessDefinition>($"{serviceUrl}/{id}");
        }

        public Task<string> GetProcessDiagram(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetProcessModel(string id)
        {
            return await httpInvoker.GetAsync<string>($"{serviceUrl}/{id}/processmodel");
        }

        public async Task<Resources<ProcessDefinition>> LatestProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            return await httpInvoker.PostAsync<Resources<ProcessDefinition>>($"{serviceUrl}/latest", new JsonContent(queryObj));
        }

        public async Task<Resources<ProcessDefinition>> ProcessDefinitions(ProcessDefinitionQuery queryObj)
        {
            return await httpInvoker.PostAsync<Resources<ProcessDefinition>>($"{serviceUrl}/list", new JsonContent(queryObj));
        }
    }
}
