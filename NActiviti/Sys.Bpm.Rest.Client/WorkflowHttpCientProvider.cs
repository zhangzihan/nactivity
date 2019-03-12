using org.activiti.cloud.services.rest.api;
using Sys.Bpm.Rest.Client.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Bpm.Rest.Client
{
    public class WorkflowHttpCientProvider
    {
        private readonly HttpInvoker httpInvoker;

        public WorkflowHttpCientProvider(HttpInvoker httpInvoker)
        {
            this.httpInvoker = httpInvoker;
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
