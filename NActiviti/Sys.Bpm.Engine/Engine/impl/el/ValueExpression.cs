using org.activiti.engine.@delegate;
using org.activiti.engine.impl.identity;
using org.activiti.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace org.activiti.engine.impl.el
{


    public class ValueExpression
    {
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);

        public ValueExpression(string expression, Type expectedType)
        {
            this.ExpressionString = expression;
            this.ValueType = expectedType;
        }

        public string ExpressionString
        {
            get; set;
        }

        public Type ValueType
        {
            get; set;
        }

        internal void setValue(ELContext context, object value)
        {
            throw new NotImplementedException("Value Expression 51 line");
        }

        internal object getValue(ELContext elContext)
        {
            if (string.IsNullOrWhiteSpace(this.ExpressionString))
            {
                return null;
            }

            var expstr = ExpressionString;

            if (elContext.ELResolver is CompositeELResolver eLResolver)
            {
                var variableScope = eLResolver.Resolvers.First(x => x.GetType() == typeof(VariableScopeElResolver)) as VariableScopeElResolver;

                return getValue(elContext, variableScope.VariableScope);
            }

            throw new NotImplementedException("Value Expression not support!");
        }

        private object getValue(ELContext context, IVariableScope variableScope)
        {
            string expstr = ExpressionString;

            if ((VariableScopeElResolver.EXECUTION_KEY.Equals(expstr) && variableScope is IExecutionEntity) || (VariableScopeElResolver.TASK_KEY.Equals(expstr) && variableScope is ITaskEntity))
            {
                context.IsPropertyResolved = true;
                return variableScope;
            }
            else if (VariableScopeElResolver.EXECUTION_KEY.Equals(expstr) && variableScope is ITaskEntity)
            {
                context.IsPropertyResolved = true;
                return ((ITaskEntity)variableScope).Execution;
            }
            else if (VariableScopeElResolver.LOGGED_IN_USER_KEY.Equals(expstr))
            {
                context.IsPropertyResolved = true;
                return Authentication.AuthenticatedUser.Id;
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
                            sb.Add($"'{expstr.Substring(m.Index, expstr.Length - m.Index)}'");
                        }
                    }
                    return r;
                });
            }
            else
            {
                return expstr;
            }

            expstr = string.Join("+", sb);

            if (variableScope is IExecutionEntity execution)
            {
                ExpandoObject contextObject = new ExpandoObject();

                foreach (string key in execution.Variables.Keys)
                {
                    (contextObject as IDictionary<string, object>).Add(key, execution.Variables[key]);
                }

                return Sys.Expressions.ExpressionManager.GetValue(contextObject, expstr, execution.Variables);
            }

            // property resolution (eg. bean.value) will be done by the
            // BeanElResolver (part of the CompositeElResolver)
            // It will use the bean resolved in this resolver as base.

            return null;
        }
    }
}
