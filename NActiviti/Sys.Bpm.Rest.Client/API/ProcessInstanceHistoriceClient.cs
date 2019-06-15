using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Net.Http;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client.API
{
    /// <inheritdoc />
    class ProcessInstanceHistoriceClient : IProcessInstanceHistoriceController
    {
        private readonly IHttpClientProxy httpProxy;

        private static readonly string serviceUrl = WorkflowConstants.PROC_HIS_INST_ROUTER_V1;

        /// <inheritdoc />
        public ProcessInstanceHistoriceClient(IHttpClientProxy httpProxy)
        {
            this.httpProxy = httpProxy;
        }

        /// <inheritdoc />
        public async Task<Resources<HistoricInstance>> ProcessInstances(HistoricInstanceQuery query)
        {
            return await httpProxy.PostAsync<Resources<HistoricInstance>>($"{serviceUrl}", query).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HistoricInstance> GetProcessInstanceById(string processInstanceId)
        {
            return await httpProxy.GetAsync<HistoricInstance>($"{serviceUrl}/{processInstanceId}").ConfigureAwait(false);
        }

        public async Task<Resources<HistoricVariableInstance>> GetVariables(string processInstanceId, string taskId)
        {
            return await httpProxy.GetAsync<Resources<HistoricVariableInstance>>($"{serviceUrl}/{processInstanceId}/variables/{taskId}").ConfigureAwait(false);
        }

        public async Task<Resources<HistoricVariableInstance>> GetVariables(ProcessVariablesQuery query)
        {
            return await httpProxy.PostAsync<Resources<HistoricVariableInstance>>($"{serviceUrl}/variables", query).ConfigureAwait(false);
        }
    }
}
