using Newtonsoft.Json;
using org.activiti.bpmn.model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
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

namespace org.activiti.engine.impl.bpmn.behavior
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;
    using System.Net.Http;

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

        private readonly IHttpClientFactory httpClientFactory;

        public ServiceTaskWebApiActivityBehavior()
        {
            httpClientFactory = ProcessEngineServiceProvider.Resolve<IHttpClientFactory>();
        }

        public override void execute(IExecutionEntity execution)
        {
            base.execute(execution);

            execution.CurrentFlowElement.ExtensionElements.TryGetValue("property",
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
                    taskRequest = GetValue(contextObject, taskRequest, execution.Variables).ToString();

                    //调用外部服务
                    HttpClient client = httpClientFactory.CreateClient();
                    HttpRequestMessage message = new HttpRequestMessage("get".Equals(method, StringComparison.OrdinalIgnoreCase) ? HttpMethod.Get : HttpMethod.Post, url);
                    if (string.IsNullOrWhiteSpace(taskRequest) == false)
                    {
                        message.Content = new StringContent(taskRequest, Encoding.UTF8, "application/json");
                    }
                    HttpResponseMessage result = client.SendAsync(message).Result;
                    if (result.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    {
                        if (string.IsNullOrWhiteSpace(dataObj) == false)
                        {
                            JToken data = JsonConvert.DeserializeObject<JToken>(result.Content.ReadAsStringAsync().Result);
                            execution.setVariable(dataObj, data);
                        }
                    }
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
                expstr = Sys.Expressions.ExpressionManager
                    .GetValue(contextObject, vexpre, variables).ToString();
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


        public override void trigger(IExecutionEntity execution, string signalEvent, object signalData)
        {
            //execution.setVariable();
            base.trigger(execution, signalEvent, signalData);
        }

    }
}