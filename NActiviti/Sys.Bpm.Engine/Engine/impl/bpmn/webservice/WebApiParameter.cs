using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys.Expressions;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Services.Api.Commands;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sys.Workflow.Engine.Impl.Bpmn.Webservice
{
    /// <summary>
    /// WebApi参数
    /// </summary>
    public class WebApiParameter
    {
        public const string WEBAPI_URL_VARNAME = "url";
        public const string WEBAPI_METHOD_VARNAME = "method";
        public const string WEBAPI_REQUEST_PARAMETER = "taskRequest";
        public const string WEBAPI_RETURN_VARNAME = "dataObject";

        private static readonly Regex JSONOBJECT_PATTERN = new Regex(@"(^(\s*{)(.*?)(\}\s*)$)|(^\s*\[(.*?)(\]\s*)$)");

        /// <summary>
        /// 正则表达式
        /// </summary>
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);

        private readonly IList<ExtensionElement> extensionElements;
        private readonly IExecutionEntity execution;
        private readonly ExpandoObject contextObject;
        private readonly string businessKey;

        public WebApiParameter(IExecutionEntity execution, IList<ExtensionElement> extensionElements)
        {
            this.extensionElements = extensionElements;
            this.execution = execution;
            contextObject = new ExpandoObject();
            foreach (string key in execution.Variables.Keys)
            {
                (contextObject as IDictionary<string, object>).Add(key, execution.Variables[key]);
            }
            execution.Variables.TryGetValue(WorkflowVariable.GLOBAL_PROCESSINSTANCE_BUSINESSKEY_VARNAME, out var value);
            businessKey = value?.ToString();
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
            {
                return null;
            }

            //存在动态参数，进行替换
            if (EXPR_PATTERN.IsMatch(expstr))
            {
                string vexpre = GetExpression(expstr);
                return ExpressionManager.GetValue(contextObject, vexpre, variables);
            }

            return expstr;
        }


        /// <summary>
        /// 获取能被表达式处理的字符串
        /// </summary>
        /// <param name="expstr"></param>
        /// <returns></returns>
        private static string GetExpression(string expstr)
        {
            if (string.IsNullOrWhiteSpace(expstr))
            {
                return expstr;
            }

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
        /// 请求的远程服务器URL地址，可以使用变量表达式来动态解析：${变量表达式}
        /// </summary>
        public string Url
        {
            get
            {
                string url = extensionElements.GetAttributeValue("url");

                url = GetValue(contextObject, url, execution.Variables).ToString();

                return url;
            }
        }

        /// <summary>
        /// 请求的远程服务器访问Token
        /// </summary>
        public string AccessToken
        {
            get
            {
                string token = extensionElements.GetAttributeValue("access_token");

                token = GetValue(contextObject, token, execution.Variables).ToString();

                return token;
            }
        }

        /// <summary>
        /// 如果当前流程在debug模式，计算mock数据
        /// </summary>
        public JToken MockData
        {
            get
            {
                string mockData = extensionElements.GetAttributeValue("mockData");
                if (string.IsNullOrEmpty(mockData))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<JToken>(mockData);
            }
        }

        /// <summary>
        /// 启用Mock模式，不执行实际的Webapi调用，只计算mock数据
        /// </summary>
        public bool? IsMock
        {
            get
            {
                string isMock = extensionElements.GetAttributeValue("isMock");

                if (bool.TryParse(isMock, out var mock))
                {
                    return mock;
                }

                return null;
            }
        }

        /// <summary>
        /// 请求参数,变量表达式，如果设置包括在{}或[]，则说明该变量为JSON表达式，运行时会先做表达式求值然后再解析为JToken类型，否则返回表达式解析字符串.
        /// </summary>
        public object Request
        {
            get
            {
                string taskRequest = extensionElements.GetAttributeValue("taskRequest");

                try
                {
                    object parameter = GetValue(contextObject, taskRequest, execution.Variables);

                    if (parameter is object)
                    {
                        string strParam = parameter.ToString();

                        JToken token;
                        if (JSONOBJECT_PATTERN.IsMatch(strParam))
                        {
                            token = JsonConvert.DeserializeObject<JToken>(parameter.ToString());

                            if (token is JArray == false && token is JValue == false)
                            {
                                token["businessKey"] = execution.BusinessKey;
                            }

                            parameter = token;
                        }
                    }

                    return parameter;
                }
                catch (Exception ex)
                {
                    throw new Exception($"【参数错误】ExecutionId={execution.Id}，TaskRequest={taskRequest}，ContextObject={(contextObject is null ? "【空值】" : JsonConvert.SerializeObject(contextObject, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))}，Variables={(execution.Variables is null ? "【空值】" : JsonConvert.SerializeObject(execution.Variables, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))}", ex);
                }
            }
        }

        /// <summary>
        /// Rest服务调用方法：get post
        /// </summary>
        public string Method
        {
            get
            {
                string method = extensionElements.GetAttributeValue("method");

                return string.IsNullOrWhiteSpace(method) ? "post" : method;
            }
        }

        /// <summary>
        /// 获取WebApi调用返回值保存流程变量名
        /// </summary>
        public string VariableName
        {
            get
            {
                string dataObj = extensionElements.GetAttributeValue("dataObj");

                return dataObj;
            }
        }
    }
}
