using org.activiti.cloud.services.rest.api;
using Sys.Bpm.Rest.Client.API;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Sys.Bpm.Rest.Client
{
    public class WorkflowHttpInvokerProvider
    {
        private readonly HttpClient httpClient;

        private IHttpInvoker httpInvoker;

        public WorkflowHttpInvokerProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            CreateHttpInvoker();
        }

        private void CreateHttpInvoker()
        {
            httpInvoker = new HttpInvoker(httpClient);
        }

        public IProcessDefinitionController GetProcessDefinitionClient()
        {
            return new ProcessDefinitionClient(httpInvoker);
        }

        public IProcessDefinitionDeployerController GetDefinitionDeployerClient()
        {
            return new ProcessDefinitionDeployerClient(httpInvoker);
        }
    }
}
