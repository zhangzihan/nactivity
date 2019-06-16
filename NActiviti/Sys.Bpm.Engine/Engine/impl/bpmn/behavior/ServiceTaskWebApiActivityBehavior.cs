using Newtonsoft.Json;
using Sys.Workflow.bpmn.model;
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

namespace Sys.Workflow.engine.impl.bpmn.behavior
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.bpmn.webservice;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Net.Http;
    using System.Threading;

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
        /// <summary>
        /// 正则表达式
        /// </summary>
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);

        public ServiceTaskWebApiActivityBehavior()
        {
        }

        public override void Execute(IExecutionEntity execution)
        {
            execution.CurrentFlowElement.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY,
                out IList<ExtensionElement> pElements);

            if (pElements != null)
            {
                try
                {
                    string url = pElements.GetAttributeValue("url");
                    string taskRequest = pElements.GetAttributeValue("taskRequest");
                    string dataObj = pElements.GetAttributeValue("dataObj");
                    string method = pElements.GetAttributeValue("method");

                    ExpandoObject contextObject = new ExpandoObject();
                    foreach (string key in execution.Variables.Keys)
                    {
                        (contextObject as IDictionary<string, object>).Add(key, execution.Variables[key]);
                    }

                    url = GetValue(contextObject, url, execution.Variables).ToString();
                    object parameter = GetValue(contextObject, taskRequest, execution.Variables);

                    if (parameter?.GetType() == typeof(string))
                    {
                        var reg = new Regex(@"(^\{(.*?)(.*?)(\})$)|(^\[(.*?)(.*?)(\])$)");
                        if (reg.IsMatch(parameter.ToString()))
                        {
                            parameter = JsonConvert.DeserializeObject<JToken>(parameter.ToString());
                        }
                        else
                        {
                            parameter = JToken.FromObject(parameter.ToString());
                        }
                    }

                    var httpProxy = ProcessEngineServiceProvider.Resolve<IServiceWebApiHttpProxy>();

                    HttpContext httpContext = ProcessEngineServiceProvider.Resolve<IHttpContextAccessor>()?.HttpContext;

                    if (httpContext == null)
                    {
                        IAccessTokenProvider accessTokenProvider = ProcessEngineServiceProvider.Resolve<IAccessTokenProvider>();

                        accessTokenProvider.SetHttpClientRequestAccessToken(httpProxy.HttpClient, null, execution.TenantId);
                    }

                    switch (method?.ToLower())
                    {
                        default:
                        case "get":
                            if (string.IsNullOrWhiteSpace(dataObj))
                            {
                                AsyncHelper.RunSync(() => httpProxy.GetAsync(url));
                            }
                            else
                            {
                                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => httpProxy.GetAsync<HttpResponseMessage>(url, CancellationToken.None));

                                response.EnsureSuccessStatusCode();

                                object data = JsonConvert.DeserializeObject<object>(AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync()));

                                execution.SetVariable(dataObj, data);
                            }
                            break;
                        case "post":
                            if (string.IsNullOrWhiteSpace(dataObj))
                            {
                                AsyncHelper.RunSync(() => httpProxy.PostAsync(url, parameter));
                            }
                            else
                            {
                                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => httpProxy.PostAsync<HttpResponseMessage>(url, parameter, CancellationToken.None));

                                response.EnsureSuccessStatusCode();

                                object data = JsonConvert.DeserializeObject<object>(AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync()));

                                execution.SetVariable(dataObj, data);
                            }
                            break;
                    }

                    Leave(execution);
                }
                catch (Exception ex)
                {
                    throw new BpmnError(Context.CommandContext.ProcessEngineConfiguration.WebApiErrorCode, ex.Message);
                }
            }
        }

        /// <summary>
        /// 进行表达式运算
        /// </summary>
        /// <param name="contextObject"></param>
        /// <param name="expstr"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private object GetValue(ExpandoObject contextObject, string expstr, IDictionary<string, object> variables)
        {
            if (string.IsNullOrWhiteSpace(expstr))
                return expstr;

            //存在动态参数，进行替换
            if (EXPR_PATTERN.IsMatch(expstr))
            {
                string vexpre = GetExpression(expstr);
                return Sys.Expressions.ExpressionManager
                    .GetValue(contextObject, vexpre, variables);
            }

            return expstr;
        }


        /// <summary>
        /// 获取能被表达式处理的字符串
        /// </summary>
        /// <param name="expstr"></param>
        /// <returns></returns>
        public static string GetExpression(string expstr)
        {
            if (string.IsNullOrWhiteSpace(expstr)) return expstr;
            List<string> sb = new List<string>();
            if (EXPR_PATTERN.IsMatch(expstr))
            {
                EXPR_PATTERN.Replace(expstr, (m) =>
                {
                    if (sb.Count == 0 && m.Index > 0)
                    {
                        sb.Add($"'{expstr.Substring(0, m.Index)}'");
                    }

                    var r = m.Result("$1");
                    sb.Add(r);
                    var nm = m.NextMatch();
                    if (nm.Success)
                    {
                        sb.Add($"'{expstr.Substring(m.Index + m.Length, nm.Index - (m.Index + m.Length))}'");
                    }
                    else
                    {
                        if (expstr.Length > (m.Index + m.Length))
                        {
                            sb.Add($"'{expstr.Substring(m.Index + m.Length, expstr.Length - m.Index - m.Length)}'");
                        }
                    }

                    return r;
                });
                return string.Join("+", sb);
                ;
            }
            else
            {
                return expstr;
            }
        }


        public override void Trigger(IExecutionEntity execution, string signalEvent, object signalData, bool throwError = true)
        {
            //execution.setVariable();
            base.Trigger(execution, signalEvent, signalData, throwError);
        }

    }
}