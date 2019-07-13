using Newtonsoft.Json;
using Sys.Workflow.Bpmn.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Net.Http;
    using System.Threading;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    /// <summary>
    /// ActivityBehavior that evaluates an expression when executed. Optionally, it sets the result of the expression as a variable on the execution.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ServiceTaskWebApiActivityBehavior : TaskActivityBehavior
    {
        private static readonly Regex OBJECT_PATTERN = new Regex(@"(^(\s*{)(.*?)(\}\s*)$)|(^\s*\[(.*?)(\]\s*)$)", RegexOptions.Compiled);

        private readonly static ILogger logger = ProcessEngineServiceProvider.LoggerService<ServiceTaskWebApiActivityBehavior>();


        public ServiceTaskWebApiActivityBehavior()
        {
        }

        public override void Execute(IExecutionEntity execution)
        {
            execution.CurrentFlowElement.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY,
                out IList<ExtensionElement> pElements);

            if (pElements != null)
            {
                Exception exception = null;
                Stopwatch sw = new Stopwatch();
                string url = null;
                string dataObj = null;
                object request = null;

                try
                {
                    sw.Start();

                    WebApiParameter parameter = new WebApiParameter(execution, pElements);
                    url = parameter.Url;
                    dataObj = parameter.VariableName;
                    request = parameter.Request;
                    string method = parameter.Method;

                    var httpProxy = ProcessEngineServiceProvider.Resolve<IServiceWebApiHttpProxy>();

                    HttpContext httpContext = ProcessEngineServiceProvider.Resolve<IHttpContextAccessor>()?.HttpContext;

                    if (httpContext == null)
                    {
                        IAccessTokenProvider accessTokenProvider = ProcessEngineServiceProvider.Resolve<IAccessTokenProvider>();

                        accessTokenProvider.SetHttpClientRequestAccessToken(httpProxy.HttpClient, execution.StartUserId, execution.TenantId);
                    }

                    switch (method?.ToLower())
                    {
                        default:
                        case "get":
                            ExecuteGet(execution, url, request, dataObj, httpProxy);
                            break;
                        case "post":
                            ExecutePost(execution, url, request, dataObj, httpProxy);
                            break;
                    }

                    sw.Stop();

                    logger.LogInformation($"调用外部服务共计({sw.ElapsedMilliseconds}ms) url={url} request={(request is null ? "" : JsonConvert.SerializeObject(request))}");

                    Leave(execution);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={url} request={(request is null ? "" : JsonConvert.SerializeObject(request))}：\r\n" + ex.Message + ex.StackTrace);

                    throw new BpmnError(Context.CommandContext.ProcessEngineConfiguration.WebApiErrorCode, ex.Message);
                }
            }
        }

        private void ExecutePost(IExecutionEntity execution, string url, object request, string dataObj, IServiceWebApiHttpProxy httpProxy)
        {
            url = QueryParameter(execution, url, request, false);

            if (string.IsNullOrWhiteSpace(dataObj))
            {
                AsyncHelper.RunSync(() => httpProxy.PostAsync(url, request));
            }
            else
            {
                HttpResponseMessage response = AsyncHelper.RunSync(() => httpProxy.PostAsync<HttpResponseMessage>(url, request, CancellationToken.None));

                object data = null;
                if (response is null == false)
                {
                    response.EnsureSuccessStatusCode();
                    data = AsyncHelper.RunSync(() => ToObject(response));
                }

                execution.SetVariable(dataObj, data);
            }
        }

        private void ExecuteGet(IExecutionEntity execution, string url, object request, string dataObj, IServiceWebApiHttpProxy httpProxy)
        {
            url = QueryParameter(execution, url, request, true);

            if (string.IsNullOrWhiteSpace(dataObj))
            {
                AsyncHelper.RunSync(() => httpProxy.GetAsync(url));
            }
            else
            {
                HttpResponseMessage response = AsyncHelper.RunSync(() => httpProxy.GetAsync<HttpResponseMessage>(url, CancellationToken.None));

                object data = null;
                if (response is null == false)
                {
                    response.EnsureSuccessStatusCode();
                    data = AsyncHelper.RunSync(() => ToObject(response));
                }

                execution.SetVariable(dataObj, data);
            }
        }

        private static string QueryParameter(IExecutionEntity execution, string url, object request, bool concatQueryString)
        {
            url = WebUtility.UrlDecode(url);
            string queryParam = (request ?? "").ToString();
            if (new Regex(@"\?=").IsMatch(url))
            {
                url = string.Concat(url, "&", queryParam, "&businessKey=", execution.BusinessKey);
            }
            else
            {
                if (concatQueryString)
                {
                    url = string.Concat(url, string.IsNullOrWhiteSpace(queryParam) ? "?businessKey=" + execution.BusinessKey : string.Concat("?", queryParam, "&businessKey=", execution.BusinessKey));
                }
                else
                {
                    url = string.Concat(url, "?businessKey=" + execution.BusinessKey);
                }
            }

            return url;
        }

        private async Task<object> ToObject(HttpResponseMessage response)
        {
            string data = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            if (OBJECT_PATTERN.IsMatch(data))
            {
                return JsonConvert.DeserializeObject<object>(data);
            }

            return data;
        }

        public override void Trigger(IExecutionEntity execution, string signalEvent, object signalData, bool throwError = true)
        {
            //execution.setVariable();
            base.Trigger(execution, signalEvent, signalData, throwError);
        }

    }
}