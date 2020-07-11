using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Scripting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    public class ValueExpression : IValueExpression
    {
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="expectedType"></param>
        public ValueExpression(string expression, Type expectedType)
        {
            this.ExpressionString = expression;
            this.ValueType = expectedType;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExpressionString
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type ValueType
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elContext"></param>
        /// <param name="value"></param>
        public void SetValue(ELContext elContext, object value)
        {
            //if (string.IsNullOrWhiteSpace(this.ExpressionString))
            //{
            //    return;
            //}

            //var expstr = ExpressionString;

            //if (elContext.ELResolver is CompositeELResolver eLResolver)
            //{
            //    var variableScope = eLResolver.Resolvers.First(x => x.GetType() == typeof(VariableScopeElResolver)) as VariableScopeElResolver;

            //    variableScope.SetValue(elContext, value);
            //    return;
            //}

            throw new NotImplementedException("Value Expression not support!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elContext"></param>
        /// <returns></returns>
        public object GetValue(ELContext elContext)
        {
            if (string.IsNullOrWhiteSpace(this.ExpressionString))
            {
                return null;
            }

            var expstr = ExpressionString;

            if (elContext.ELResolver is CompositeELResolver eLResolver)
            {
                var variableScope = eLResolver.Resolvers.First(x => x.GetType() == typeof(VariableScopeElResolver)) as VariableScopeElResolver;

                return GetValue(elContext, variableScope.VariableScope);
            }

            throw new NotImplementedException("Value Expression not support!");
        }

        private object GetValue(ELContext context, IVariableScope variableScope)
        {
            string expstr = ExpressionString;

            if ((VariableScopeElResolver.EXECUTION_KEY.Equals(expstr) && variableScope is IExecutionEntity) || (VariableScopeElResolver.TASK_KEY.Equals(expstr) && variableScope is ITaskEntity))
            {
                context.IsPropertyResolved = true;
                return variableScope;
            }
            else if (VariableScopeElResolver.EXECUTION_KEY.Equals(expstr) && variableScope is ITaskEntity entity)
            {
                context.IsPropertyResolved = true;
                return entity.Execution;
            }
            else if (VariableScopeElResolver.LOGGED_IN_USER_KEY.Equals(expstr))
            {
                context.IsPropertyResolved = true;
                return Authentication.AuthenticatedUser.Id;
            }

            if (EXPR_PATTERN.IsMatch(expstr) == false)
            {
                return expstr;
            }

            List<string> sb = new List<string>();
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
                        sb.Add($"'{expstr[m.Index..]}'");
                    }
                }
                return r;
            });

            expstr = string.Join("+", sb);

            if (variableScope is IExecutionEntity execution)
            {
                ExpandoObject contextObject = new ExpandoObject();

                foreach (string key in execution.Variables.Keys)
                {
                    var value = execution.Variables[key];
                    object obj = ToObject(value);

                    (contextObject as IDictionary<string, object>).Add(key, obj);
                }

                return Sys.Expressions.ExpressionManager.GetValue(contextObject, expstr, execution.Variables);
            }

            // property resolution (eg. bean.value) will be done by the
            // BeanElResolver (part of the CompositeElResolver)
            // It will use the bean resolved in this resolver as base.

            return null;
        }

        private object ToObject(object value)
        {
            if (value == null)
            {
                return value;
            }

            if (value is JArray arry)
            {
                IList<object> list = new List<object>();
                foreach (var arr in arry)
                {
                    var obj = ToObject(arr);
                    list.Add(obj);
                }
                return list;
            }
            else if (value is JValue jValue)
            {
                return jValue.Value;
            }
            else if (value is JToken jToken)
            {

                var dict = jToken.ToObject<ExpandoObject>();
                return dict;
            }
            else
            {
                return value;
            }
        }
    }
}
