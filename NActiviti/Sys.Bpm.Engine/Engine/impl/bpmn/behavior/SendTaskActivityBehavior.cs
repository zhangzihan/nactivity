using Newtonsoft.Json;
using Sys.Workflow.Bpmn.Models;
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

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using Sys.Workflow.Util;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

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
    public class SendTaskActivityBehavior : TaskActivityBehavior
    {
        /// <summary>
        /// 正则表达式
        /// </summary>
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);

        private readonly ExternalConnectorProvider externalConnector;

        public SendTaskActivityBehavior()
        {
            externalConnector = ProcessEngineServiceProvider.Resolve<ExternalConnectorProvider>();
        }

        public override void Execute(IExecutionEntity execution)
        {
            execution.CurrentFlowElement.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY,
                out IList<ExtensionElement> pElements);

            if (pElements != null)
            {
                try
                {
                    string email = pElements.GetAttributeValue("email");
                    string wechat = pElements.GetAttributeValue("wechat");
                    string sms = pElements.GetAttributeValue("sms");

                    ExpandoObject contextObject = new ExpandoObject();
                    foreach (string key in execution.Variables.Keys)
                    {
                        (contextObject as IDictionary<string, object>).Add(key, execution.Variables[key]);
                    }

                    email = GetValue(contextObject, email, execution.Variables).ToString();
                    wechat = GetValue(contextObject, wechat, execution.Variables).ToString();
                    sms = GetValue(contextObject, sms, execution.Variables).ToString();

                    var mailServiceUrl = externalConnector.MailServiceUrl;
                    var wechatServiceUrl = externalConnector.WechatServiceUrl;
                    var smsServiceUrl = externalConnector.SmsServiceUrl;

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        if (string.IsNullOrWhiteSpace(mailServiceUrl))
                        {
                            throw new Exception("未配置邮件服务URL");
                        }
                        AsyncHelper.RunSync(() => SendMessageAsync(execution, email, mailServiceUrl));
                    }

                    if (!string.IsNullOrWhiteSpace(wechat))
                    {
                        if (string.IsNullOrWhiteSpace(wechatServiceUrl))
                        {
                            throw new Exception("未配置微信服务URL");
                        }
                        AsyncHelper.RunSync(() => SendMessageAsync(execution, wechat, wechatServiceUrl));
                    }

                    if (!string.IsNullOrWhiteSpace(sms))
                    {
                        if (string.IsNullOrWhiteSpace(smsServiceUrl))
                        {
                            throw new Exception("未配置短信服务URL");
                        }
                        AsyncHelper.RunSync(() => SendMessageAsync(execution, sms, smsServiceUrl));
                    }

                    Leave(execution);
                }
                catch (Exception ex)
                {
                    throw new BpmnError(Context.CommandContext.ProcessEngineConfiguration.WebApiErrorCode, ex.Message);
                }
            }
        }

        private async Task SendMessageAsync(IExecutionEntity execution, string msg, string msgServiceUrl)
        {

            var httpProxy = ProcessEngineServiceProvider.Resolve<IServiceWebApiHttpProxy>();

            HttpResponseMessage response = await httpProxy.PostAsync<HttpResponseMessage>(msgServiceUrl, new
            {
                MessageId = msg,
                execution.BusinessKey
            }, CancellationToken.None).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
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