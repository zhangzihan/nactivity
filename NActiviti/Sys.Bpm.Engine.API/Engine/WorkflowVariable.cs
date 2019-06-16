using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Services.Api.Commands
{
    /// <summary>
    /// 工作流流程变量
    /// </summary>
    public class WorkflowVariable : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> workflowVariables = null;

        public static readonly string GLOBAL_APPROVALED_VARIABLE = "同意";
        public static readonly string GLOBAL_BUSINESSKEY_VARIABLE = "事项编号";
        public static readonly string GLOBAL_WORKFLOWDATA_VARIABLE = "流程数据";
        public static readonly string GLOBAL_APPROVALED_COMMENTS = "审批意见";
        public static readonly string GLOBAL_APPROVALED_COUNTERSIGN = "会签审批人";

        /// <summary>
        /// 流程主数据，存在整个工作流生命周期中
        /// </summary>
        public object WorkflowData
        {
            get
            {
                if (TryGetValue(GLOBAL_WORKFLOWDATA_VARIABLE, out object data))
                {
                    return data;
                }

                return null;
            }
            set
            {
                this[GLOBAL_WORKFLOWDATA_VARIABLE] = value;
            }
        }

        /// <summary>
        /// 流程主数据主键id
        /// </summary>
        public string BusinessKey
        {
            get
            {
                if (TryGetValue(GLOBAL_BUSINESSKEY_VARIABLE, out var businessKey))
                {
                    return businessKey?.ToString();
                }

                return null;
            }
            set
            {
                this[GLOBAL_BUSINESSKEY_VARIABLE] = value;
            }
        }

        /// <summary>
        /// 工作流全局审核状态
        /// </summary>
        public bool? Approvaled
        {
            get
            {
                if (TryGetValue(GLOBAL_APPROVALED_VARIABLE, out object approvaled) && !(approvaled is null))
                {
                    bool.TryParse(approvaled.ToString(), out bool result);

                    return result;
                }

                return null;
            }
            set
            {
                this[GLOBAL_APPROVALED_VARIABLE] = value;
            }
        }

        public WorkflowVariable() : this(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase))
        {
        }

        public WorkflowVariable(IDictionary<string, object> @params)
        {
            workflowVariables = new Dictionary<string, object>(@params ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase);

            this.Approvaled = false;
            this.WorkflowData = null;
            this.BusinessKey = null;
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                return workflowVariables[key];
            }
            set
            {
                workflowVariables[key] = value;
            }
        }

        /// <inheritdoc />
        public ICollection<string> Keys => workflowVariables.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => workflowVariables.Values;

        /// <inheritdoc />
        public int Count => workflowVariables.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            workflowVariables.Add(key, value);
        }

        public WorkflowVariable Put(string key, object value)
        {
            this[key] = value;

            return this;
        }

        public WorkflowVariable Put(Dictionary<string, object> @params)
        {
            foreach (var key in @params.Keys)
            {
                this[key] = @params[key];
            }

            return this;
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item)
        {
            workflowVariables.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            workflowVariables.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item)
        {
            return workflowVariables.Contains(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return workflowVariables.ContainsKey(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            workflowVariables.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return workflowVariables.GetEnumerator();
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            return workflowVariables.Remove(key);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item)
        {
            return workflowVariables.Remove(item);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            return workflowVariables.TryGetValue(key, out value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (workflowVariables.TryGetValue(key, out object res))
            {
                if (res == null)
                {
                    value = default;
                    return true;
                }

                if (res is T)
                {
                    value = (T)res;
                }
                else
                {
                    value = JToken.FromObject(res).ToObject<T>();
                }

                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return workflowVariables.GetEnumerator();
        }

        public static implicit operator Dictionary<string, object>(WorkflowVariable @params)
        {
            if (@params is null)
            {
                return null;
            }

            return @params.workflowVariables as Dictionary<string, object>;
        }

        public static implicit operator WorkflowVariable(Dictionary<string, object> dict)
        {
            if (dict is null)
            {
                return null;
            }

            return new WorkflowVariable(dict);
        }

        public static implicit operator JToken(WorkflowVariable @params)
        {
            if (@params is null)
            {
                return null;
            }

            return JToken.FromObject(@params);
        }

        public static implicit operator WorkflowVariable(JToken token)
        {
            if (token is null)
            {
                return null;
            }

            return token.ToObject<WorkflowVariable>();
        }
    }
}
