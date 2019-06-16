﻿using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessInstanceTasksClient : IProcessInstanceTasksController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_INS_ROUTER_V1;

        /// <inheritdoc />
        public ProcessInstanceTasksClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<Resources<TaskModel>> GetTasks(ProcessInstanceTaskQuery query)
        {
            return await httpProxy.PostAsync<Resources<TaskModel>>($"{serviceUrl}/tasks", query).ConfigureAwait(false);
        }
    }
}
