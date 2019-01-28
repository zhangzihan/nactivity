using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using org.activiti.bpmn.model;
using org.activiti.engine.impl.@delegate;

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
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;

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


        public override void execute(IExecutionEntity execution)
        {
            base.execute(execution);

            execution.CurrentFlowElement.ExtensionElements.TryGetValue("property",
                out IList<ExtensionElement> pElements);
            if (pElements != null)
            {
                string url = GetAttributeValue(pElements, "url");
                string taskRequest = GetAttributeValue(pElements, "taskRequest");
                string dataObj = GetAttributeValue(pElements, "dataObj");
                string method = GetAttributeValue(pElements, "method");

                ExpandoObject contextObject = new ExpandoObject();
                foreach (string key in execution.Variables.Keys)
                {
                    (contextObject as IDictionary<string, object>).Add(key, execution.Variables[key]);
                }

                url = GetValue(contextObject, url, execution.Variables).ToString();
                taskRequest =GetValue(contextObject, taskRequest, execution.Variables).ToString();
                
                //调用外部服务
                HttpResult result = HttpHelper.Post(url, taskRequest);
                if (result.Code == 200)
                {
                    JToken j = JsonConvert.DeserializeObject<JToken>(result.Content);
                    execution.setVariable(
                        string.IsNullOrWhiteSpace(dataObj) ? execution.CurrentFlowElement.Name : dataObj, j);
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
            if(string.IsNullOrWhiteSpace(expstr))
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


        /// <summary>
        /// 获取节点的值
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetAttributeValue(IList<ExtensionElement> elements, string name)
        {
            foreach (var element in elements)
            {
                string ename = element.getAttributeValue(null, "name");
                if (ename == name)
                {
                    return element.getAttributeValue(null, "value");
                }
            }

            return null;
        }



        public override void trigger(IExecutionEntity execution, string signalEvent, object signalData)
        {
            //execution.setVariable();
            base.trigger(execution, signalEvent, signalData);
        }

    }

    /// <summary>
    /// http响应封装
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code">返回状态码</param>
        /// <param name="response">返回数据</param>
        public HttpResult(HttpStatusCode code, string response)
        {
            this.Code = (int)code;
            this.Content = response;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public string Content { get; set; }
    }

    public class HttpHelper
    {
        private static Encoding encoding = Encoding.UTF8;


        /// <summary>
        /// 设置
        /// </summary>
        private static readonly JsonSerializerSettings SerializerSettingsAllField = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        /// <summary>
        /// json 格式 Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="obj">对象可以的，会进行 JSON.SerializeAllField()</param>
        /// <param name="timeoutSeconds">TimeOut 秒数</param>
        /// <returns></returns>
        public static HttpResult Post(string url, string data = null, int timeoutSeconds = 60)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new HttpResult(HttpStatusCode.NotFound,"url empty");

            Encoding encode = Encoding.UTF8;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.ContentType = "application/json;charset=utf-8";
            myRequest.Method = "POST";
            if (timeoutSeconds < 3)
                timeoutSeconds = 3;

            byte[] bs = null;
            myRequest.Timeout = timeoutSeconds * 1000;
            if (!string.IsNullOrWhiteSpace(data))
            {
                bs = encoding.GetBytes(data);
                myRequest.ContentLength = bs.Length;
            }
            else
                myRequest.ContentLength = 0;


            using (Stream reqStream = myRequest.GetRequestStream())
            {
                if (!string.IsNullOrWhiteSpace(data))
                    reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        var responseData = reader.ReadToEnd().ToString();
                        return new HttpResult(response.StatusCode, responseData);
                    }
                }
            }
            catch (WebException ex)
            {}
            return new HttpResult(HttpStatusCode.InternalServerError, "InternalServerError"); ;
        }

    }
}