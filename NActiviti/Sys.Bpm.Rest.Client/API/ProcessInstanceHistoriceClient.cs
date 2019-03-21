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
            return await httpProxy.PostAsync<Resources<HistoricInstance>>($"{serviceUrl}", query);
        }

        /// <inheritdoc />
        public async Task<HistoricInstance> GetProcessInstanceById(string processInstanceId)
        {
            return await httpProxy.GetAsync<HistoricInstance>($"{serviceUrl}/{processInstanceId}");
        }
    }
}
