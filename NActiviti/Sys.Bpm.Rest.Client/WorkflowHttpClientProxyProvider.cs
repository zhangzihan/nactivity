using Microsoft.AspNetCore.Http;
using org.activiti.cloud.services.rest.api;
using Sys.Bpm.Rest.Client.API;
using Sys.Net.Http;
using System.Net.Http;

namespace Sys.Bpm.Rest.Client
{
    /// <summary>
    /// 工作流客户端调用创建工厂
    /// </summary>
    public class WorkflowHttpClientProxyProvider
    {
        private readonly HttpClient httpClient;
        private readonly IHttpClientProxy httpProxy;
        private readonly IAccessTokenProvider accessTokenProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        public WorkflowHttpClientProxyProvider(HttpClient httpClient, IAccessTokenProvider accessTokenProvider, IHttpContextAccessor httpContextAccessor) : this(httpClient, accessTokenProvider, httpContextAccessor.HttpContext)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        public WorkflowHttpClientProxyProvider(HttpClient httpClient, IAccessTokenProvider accessTokenProvider, HttpContext httpContext)
        {
            this.httpClient = httpClient;
            this.accessTokenProvider = accessTokenProvider;
            httpProxy = new HttpClientProxy(httpClient, this.accessTokenProvider, httpContext);
        }

        /// <summary>
        /// 流程定义客户端-已发布
        /// </summary>
        /// <returns></returns>
        public IProcessDefinitionController GetProcessDefinitionClient()
        {
            return new ProcessDefinitionClient(httpProxy);
        }

        /// <summary>
        /// 流程部署客户端-未发布+已发布
        /// </summary>
        /// <returns></returns>
        public IProcessDefinitionDeployerController GetDefinitionDeployerClient()
        {
            return new ProcessDefinitionDeployerClient(httpProxy);
        }

        /// <summary>
        /// 流程实例客户端
        /// </summary>
        /// <returns></returns>
        public IProcessInstanceController GetProcessInstanceClient()
        {
            return new ProcessInstanceClient(httpProxy);
        }

        /// <summary>
        /// 流程历史实例客户端
        /// </summary>
        /// <returns></returns>
        public IProcessInstanceHistoriceController GetProcessInstanceHistoriceClient()
        {
            return new ProcessInstanceHistoriceClient(httpProxy);
        }

        /// <summary>
        /// 流程实例任务客户端
        /// </summary>
        /// <returns></returns>
        public IProcessInstanceTasksController GetProcessInstanceTasksClient()
        {
            return new ProcessInstanceTasksClient(httpProxy);
        }

        /// <summary>
        /// 流程任务客户端
        /// </summary>
        /// <returns></returns>
        public ITaskController GetTaskClient()
        {
            return new TaskClient(httpProxy);
        }
    }
}
