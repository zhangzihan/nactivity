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
    public class ScriptingExpression : IValueExpression
    {
        private readonly string language;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <param name="expression"></param>
        /// <param name="expectedType"></param>
        public ScriptingExpression(string language, string expression, Type expectedType)
        {
            this.ExpressionString = expression;
            this.language = language;
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

            IScriptingEnginesProvider scriptingEnginesProvider = ProcessEngineServiceProvider.Resolve<IScriptingEnginesProvider>();
            IScriptingEngines scriptingEngines = scriptingEnginesProvider.Create(language);

            var execution = variableScope is ITaskEntity te ? te.Execution : variableScope as IExecutionEntity;
            
            return scriptingEngines.Evaluate(expstr, execution);
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
